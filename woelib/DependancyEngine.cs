using System.Text;

namespace woelib
{
    public class Action<T> where T : Enum
    {
        readonly List<T> m_remainingDependacies = [];
        readonly int m_id;
        internal DependancyEngine<T>.StartTaskCallback ResolvedCallback { get; private set; }
        public int Id { get { return m_id; } }
        public string Name { get; set; } = string.Empty;
        public bool HasDependancies { get { return m_remainingDependacies.Any(); } }

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
    /// admin permission level: 
    /// user id: trouble ticket id
    /// user id: user email
    /// user address: user id, admin permission level
    /// user email: user id, admin permission level
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DependancyEngine<T> where T : Enum
    {
        public delegate void FinishedCallback(DependancyEngine<T> dependancyEngine);
        public delegate void StartTaskCallback(DependancyEngine<T> dependancyEngine, int actionId);

        public event FinishedCallback OnFinished = delegate { };

        private readonly IDependancyEngineDataHandler m_dependancyEngineDataHandler;
        public IDependancyEngineDataHandler DataHandler { get { return m_dependancyEngineDataHandler; } }

        /// <summary>
        /// 
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataHandler"></param>
        public DependancyEngine(IDependancyEngineDataHandler dataHandler)
        {
            m_dependancyEngineDataHandler = dataHandler;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resolveCallback"></param>
        /// <param name="dependancies"></param>
        public Action<T> Add(StartTaskCallback resolveCallback, params T[] dependancies)
        {
            lock (m_lock)
            {
                Action<T> action = new Action<T>(resolveCallback, ++m_nextActionId, dependancies);
                m_actions.Add(action);
                m_awaitingActions.Add(action.Id);
                return action;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="Exception"></exception>
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
        /// 
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
        /// 
        /// </summary>
        /// <param name="actionId"></param>
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
        /// 
        /// </summary>
        /// <returns></returns>
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
