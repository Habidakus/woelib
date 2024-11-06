using System.Text;

namespace woelib
{
    /// <summary>
    /// Class used by <c>DependancyEngine</c> to track which actions haven't been fufilled yet.
    /// </summary>
    public class Action<T> where T : Enum
    {
        readonly List<T> m_remainingDependacies = [];
        readonly int m_id;
        internal int Id { get { return m_id; } }
        internal DependancyEngine<T>.StartTaskCallback ResolvedCallback { get; private set; }
        internal bool HasDependancies { get { return m_remainingDependacies.Any(); } }

        /// <summary>
        /// Useful mutator to name and track an action.
        /// This <c>Name</c> will be used when <c>DependancyEngine.GetState()</c> is called.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        internal Action(DependancyEngine<T>.StartTaskCallback callback, int id, params T[] dependancies)
        {
            ResolvedCallback = callback;
            m_id = id;
            foreach (T dep in dependancies)
            {
                m_remainingDependacies.Add(dep);
            }
        }

        internal bool DependsOn(T dependancy)
        {
            return m_remainingDependacies.Contains(dependancy);
        }

        internal bool ClearDependancy(T dependancy)
        {
            m_remainingDependacies.Remove(dependancy);
            return m_remainingDependacies.Count == 0;
        }

        internal string GetState()
        {
            string deps = string.Join(", ", m_remainingDependacies.Select(a => Enum.GetName(a.GetType(), a)));
            string task = string.IsNullOrWhiteSpace(Name) ? $"#{Id}" : Name;
            if (string.IsNullOrWhiteSpace(deps))
            {
                return $"Task[{task}] is running";
            }
            else
            {
                return $"Task[{task}] waiting on {deps}";
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IDependancyEngineDataHandler { }

    /// <summary>
    /// A utility class to help with marshaling all the things that need to be done, given various dependancies.
    /// 
    /// <code>
    /// UserInfoCache userInfoCache = new UserInfoCache();
    /// DependancyEngine&lt;UserInfoDeps&gt; infoDepEngine = new(userInfoCache);
    /// infoDepEngine.Add(userInfoCache.FetchAdminPermissionLevel, UserInfoDeps.STARTED);
    /// infoDepEngine.Add(userInfoCache.FetchUserId, UserInfoDeps.UserEmailAddr).Name = "Fetch User Id via email";
    /// infoDepEngine.Add(userInfoCache.FetchUserId, UserInfoDeps.TroubleTicketId).Name = "Fetch User Id via trouble ticket";
    /// infoDepEngine.Add(userInfoCache.FetchUserEmail, UserInfoDeps.UserId, UserInfoDeps.AdminPermissionLevel).Name = "Fetch Email";
    /// infoDepEngine.Add(userInfoCache.FetchUserAddress, UserInfoDeps.UserId, UserInfoDeps.AdminPermissionLevel).Name = "Fetch Addr";
    /// infoDepEngine.OnFinished += userInfoCache.FetchCompleted;
    /// infoDepEngine.Resolve(UserInfoDeps.STARTED);
    /// /* Either seed and resolve email, or trouble ticket id */
    /// infoDepEngine.Start();
    /// </code>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DependancyEngine<T> where T : Enum
    {
        public delegate void FinishedCallback(DependancyEngine<T> dependancyEngine);
        public delegate void StartTaskCallback(DependancyEngine<T> dependancyEngine, int actionId);

        public event FinishedCallback OnFinished = delegate { };

        private readonly IDependancyEngineDataHandler? m_dependancyEngineDataHandler;

        /// <summary>
        /// If the <c>DependancyEngine</c> was given an <c>IDependancyEngineDataHandler</c> object when it was
        /// constructed, that same object can always be retried through this mutator.
        /// </summary>
        public IDependancyEngineDataHandler? DataHandler { get { return m_dependancyEngineDataHandler; } }

        /// <summary>
        /// Returns true if there are outstanding actions (registered via <c>Add()</c>) that have not yet reported
        /// completion. Note that <c>IsRunning</c> will return false if there are actions that have never been run
        /// because their dependancies were never resolved.
        /// </summary>
        public bool IsRunning
        {
            get 
            {
                foreach (var action in m_actions)
                {
                    if (action.HasDependancies)
                        continue;
                    if (m_awaitingActions.Contains(action.Id))
                        return true;
                }

                return false;
            }
        }

        private readonly object m_lock = new();
        private HashSet<T>? m_dependanciesResolvedBeforeStart = [];
        private readonly HashSet<int> m_awaitingActions = [];
        private int m_nextActionId = 0;
        private readonly List<Action<T>> m_actions = [];

        /// <summary>Constructor for the dependancy engine.</summary>
        /// <param name="dataHandler">A cache where the user can store any information they might need through-out the running of this dependancy resolution.</param>
        public DependancyEngine(IDependancyEngineDataHandler? dataHandler = null)
        {
            m_dependancyEngineDataHandler = dataHandler;
        }

        /// <summary>
        /// Registers an action (<c>ResolveCallback</c>) to take once the given <c>dependancies</c> are completed.
        /// For instance you could expect an information gathering one to have:
        /// 
        /// <code>
        /// infoDepEngine.Add(userInfoCache.FetchUserEmail, UserInfoDeps.UserId, UserInfoDeps.AdminPermissionLevel).Name = "Fetch Email";
        /// </code>
        /// Where the userInfoCache class would have the following function defined:
        /// <code>
        /// internal void FetchUserEmail(DependancyEngine&lt;UserInfoDeps&gt; dependancyEngine, int actionId)
        /// {
        ///     if (!string.IsNullOrWhiteSpace(UserEmail))
        ///     {
        ///         dependancyEngine.MarkCompleted(actionId);
        ///     }
        ///     else if (!HasSuperviorPermissions)
        ///     {
        ///         UserEmail = "(redacted)";
        ///         dependancyEngine.MarkCompleted(actionId);
        ///     }
        ///     else
        ///         FetchEmailFromDBAndThenMarkItResolvedAndCompleted(UserId, dependancyEngine, actionId);
        /// }
        /// </code>
        /// 
        /// </summary>
        /// <param name="function">The method to call once all the given <c>dependancies</c> are met.</param>
        /// <param name="dependancies">A list of one or more dependancies that all must be marked as resolved (via <c>Resolve()</c>) before the given <c>function</c> will be invoked.</param>
        public Action<T> Add(StartTaskCallback function, params T[] dependancies)
        {
            lock (m_lock)
            {
                Action<T> action = new Action<T>(function, ++m_nextActionId, dependancies);
                m_actions.Add(action);
                m_awaitingActions.Add(action.Id);
                return action;
            }
        }

        /// <summary>
        /// Start processing the dependancy engine - at least one call to <c>Resolve()</c> should have been called before <c>Start()</c> is invoked.
        /// </summary>
        /// <exception cref="Exception">Thrown if invoked twice on the same dependancy engine.</exception>
        public void Start()
        {
            HashSet<T>? dependanciesToResolve = null;

            lock (m_lock)
            {
                if (m_dependanciesResolvedBeforeStart == null)
                {
                    throw new Exception($"Start() called twice");
                }

                dependanciesToResolve = m_dependanciesResolvedBeforeStart;
                m_dependanciesResolvedBeforeStart = null;
            }

            foreach (T dependancy in dependanciesToResolve)
            {
                Resolve(dependancy);
            }
        }

        /// <summary>
        /// Used when we know that a dependancy has been resolved successfully and can be used by any action awaiting it.
        /// </summary>
        /// <param name="dependancy"></param>
        public void Resolve(T dependancy)
        {
            Dictionary<int, StartTaskCallback> pendingCallbacks = [];
            lock (m_lock)
            {
                if (m_dependanciesResolvedBeforeStart != null)
                {
                    m_dependanciesResolvedBeforeStart.Add(dependancy);
                    return;
                }

                foreach (Action<T> action in m_actions)
                {
                    if (action.DependsOn(dependancy))
                    {
                        if (action.ClearDependancy(dependancy))
                        {
                            pendingCallbacks[action.Id] = action.ResolvedCallback;
                        }
                    }
                }
            }

            foreach (var pc in pendingCallbacks)
            {
                pc.Value(this, pc.Key);
            }
        }

        /// <summary>
        /// Mark any action that was added to the system via the <c>Add()</c> method as completed. Until an action is
        /// marked completed it will be considered to be still running, and the dependancy engine's <c>IsRunning</c>
        /// will return <c>true</c>.
        /// <code>
        /// internal void FetchEmailFromDBAndThenMarkItResolvedAndCompleted(long userId, DependancyEngine&lt;UserInfoDeps&gt; dependancyEngine, int actionId)
        /// {
        ///     Task.Run(() => {
        ///         UserEmail = DB.FetchUserEmailFromUserIdAsync(userId).Result;
        ///         dependancyEngine.Resolve(UserInfoDeps.UserEmailAddr);
        ///         dependancyEngine.MarkCompleted(actionId);
        ///     });
        /// }
        /// </code>
        /// </summary>
        /// <param name="actionId">The action we are marking completed, this was the int passed in when the <c>StartTaskCallback</c> was invoked.</param>
        /// <exception cref="Exception"></exception>
        public void MarkCompleted(int actionId)
        {
            lock (m_lock)
            {
                IEnumerable<Action<T>> matchingActions = m_actions.Where(a => a.Id == actionId);

                if (!matchingActions.Any())
                    throw new Exception($"Action #{actionId} is does not exist. Was the right action ID given?");
                if (matchingActions.Count() > 1)
                    throw new Exception($"Why are there {matchingActions.Count()} action with ID #{actionId}?");

                Action<T> action = matchingActions.First();

                if (!m_awaitingActions.Contains(action.Id))
                {
                    throw new Exception($"Action #{action.Id} is no awaiting completion. Was isCompleted set to true twice on the same action ID?");
                }

                m_awaitingActions.Remove(action.Id);

                if (m_awaitingActions.Count == 0)
                {
                    OnFinished.Invoke(this);
                }
            }
        }

        /// <summary>
        /// At any time this can be called to get a debug readout of what actions are still awaiting their dependancies.
        /// </summary>
        /// <returns>A multi-line string describing each action and it's unresolved dependancies.</returns>
        public string GetState()
        {
            StringBuilder sb = new();
            lock (m_lock)
            {
                foreach (Action<T> action in m_actions)
                {
                    if (m_awaitingActions.Contains(action.Id))
                    {
                        sb.AppendLine(action.GetState());
                    }
                }
            }

            return sb.ToString();
        }
    }
}
