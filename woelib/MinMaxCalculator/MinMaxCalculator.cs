namespace woelib.MinMaxCalculator
{
    ///<summary>
    /// An generic alpha beta pruning tree to implement deterministic best move calculations.
    ///
    /// Every time you wish to have the artificial
    /// opponent take a turn, you'll need to supply the MinMaxCalculator's get_best_action() function
    /// with a representation of the current game state. To do this first create an extension of the
    /// [MMCGameState] class which can hold within it a flyweight representation of the current state of
    /// the game - your class will need to implement your own version of the three base functions of that
    /// class; apply_action(), get_moves(), and get_score(). Next you'll need to extend both the [MMCAction]
    /// and [MMCScore] classes. Once you have all three, you will be able to ask the MinMaxCalculator for
    /// an optimal move for the artificial opponent to take.
    ///
    /// This class is an implementation of https://en.wikipedia.org/wiki/Negamax
    ///</summary>
    public class MinMaxCalculator
    {
        ///<summary>
        /// Given a game state, calculate which potential action is the optimal next move. For very simple
        /// game states with limited moves, the depth value can be omitted and the engine will attempt to
        /// calculate the entire game tree. However for most interesting games (eg, not tic-tac-toe) you will
        /// want to specify a maximum depth for the engine to consider, otherwise the search space will be
        /// far too large and the game will appear to stop. If you are having trouble figuring out what is
        /// going on with your implementation, you can hand in an optional MMCDebug object which you can
        /// then call the [method MMCDebug.dump] function on to get the entire tree that the function derived (you might need
        /// to implement the _to_string() function on your implementation of the Game State, Action, and Score
        /// classes to get understandable output).
        ///</summary>
        public static IMMCAction? GetBestAction(IMMCGameState gameState, Int32 depth = Int32.MaxValue, MMCDebug? debug = null)
        {
            MMCResult? bestResult = GetBestAction_Internal(gameState, IMMCScore.Lowest, IMMCScore.Highest, depth, debug);
            if (bestResult == null)
            {
                return null;
            }
            else
            {
                return bestResult.Action;
            }
        }

        public interface IMMCRequest
        {
            public IMMCGameState GameState { get; }
            /// <summary>
            /// The remaining actions to be considered
            /// </summary>
            public IMMCAction[] SortedRemainingActions { get; }
            public IMMCScore LowerBound { get; }
            public IMMCScore UpperBound { get; }
            public Int32 Depth { get; }
            public MMCDebug? Debug { get; }

            internal IMMCRequest CreateChild(IMMCGameState iMMCGameState, IMMCScore lowerBound);
        }

        public class IMMCRequest_Basic : IMMCRequest
        {
            public IMMCRequest_Basic(IMMCGameState gameState, Int32 depth = Int32.MaxValue, MMCDebug? debug = null)
            {
                this.GameState = gameState;
                this.LowerBound = IMMCScore.Lowest;
                this.UpperBound = IMMCScore.Highest;
                this.Depth = depth;
                this.Debug = debug;
            }

            private IMMCRequest_Basic(IMMCGameState gameState, IMMCRequest parent, IMMCScore upperBound)
            {
                this.GameState = gameState;
                this.LowerBound = parent.UpperBound;
                this.UpperBound = upperBound;
                this.Depth = parent.Depth - 1;
                this.Debug = parent.Debug;
            }

            public IMMCGameState GameState { get; }

            public IMMCScore LowerBound { get; }

            public IMMCScore UpperBound { get; }

            public int Depth { get; }

            public MMCDebug? Debug { get; }

            public IMMCAction[] SortedRemainingActions {
                get { return GameState.SortedMoves; }
            }

            IMMCRequest IMMCRequest.CreateChild(IMMCGameState childGameState, IMMCScore lowerBound)
            {
                return new IMMCRequest_Basic(childGameState, this, lowerBound);
            }
        }

        public class IMMCResponse
        {
            public IMMCAction? BestAction { get; }
            public IMMCScore Score { get; }

            private IMMCResponse(IMMCAction? action, IMMCScore score)
            {
                BestAction = action;
                Score = score;
            }

            private IMMCResponse()
            { }

            internal static IMMCResponse CreateScoreResponse(IMMCScore score) { return new(null, score); }
            internal static IMMCResponse CreateFullResponse(IMMCAction action, IMMCScore score) { return new(action, score); }
            internal static IMMCResponse CreateEmptyResponse() { return new(); }
        }

        public static IMMCAction? GetBestAction(IMMCRequest request)
        {
            IMMCResponse response = GetBestAction_Internal(request);
            return response.BestAction;
        }

        private static IMMCResponse GetBestAction_Internal(IMMCRequest request)
        {
            if (request.Depth == 0)
            {
                request.Debug?.AddActions(request.GameState, Array.Empty<IMMCAction>());

                // Action is a terminal (leaf) action, so there are no counters to it
                return IMMCResponse.CreateScoreResponse(request.GameState.Score);
            }

            IMMCAction[] actions = request.SortedRemainingActions;
            request.Debug?.AddActions(request.GameState, actions);
            if (actions.Any() == false)
            {
                // Action is a terminal (leaf) action, so there are no counters to it
                return IMMCResponse.CreateScoreResponse(request.GameState.Score);
            }

            var lowerBound = request.LowerBound;
            IMMCResponse? best = null;
            foreach (IMMCAction action in actions)
            {
                IMMCRequest postActionRequest = request.CreateChild(request.GameState.ApplyAction(action), lowerBound);
                IMMCResponse response = GetBestAction_Internal(postActionRequest);
                request.Debug?.AddResult(request.GameState, action, postActionRequest.GameState, response.Score);

                if (best == null)
                {
                    best = IMMCResponse.CreateFullResponse(action, response.Score);
                }
                else if (IMMCScore.IsLeftGreaterThanRight(request.GameState.Player, response.Score, best.Score))
                {
                    best = IMMCResponse.CreateFullResponse(action, response.Score);
                }

                if (IMMCScore.IsLeftGreaterThanRight(request.GameState.Player, best.Score, lowerBound))
                {
                    lowerBound = response.Score;
                }

                // #TODO: Check this?
                if (IMMCScore.IsLeftGreaterThanRight(request.GameState.Player, lowerBound, request.UpperBound))
                {
                    return best;
                }
            }

            if (best != null)
            {
                return best;
            }
            else
            {
                return IMMCResponse.CreateEmptyResponse();
            }
        }

        private static MMCResult? GetBestAction_Internal(IMMCGameState gameState, IMMCScore lowerBound, IMMCScore upperBound, Int32 depth, MMCDebug? debug)
        {
            if (depth == 0)
            {
                if (debug != null)
                {
                    debug.AddActions(gameState, Array.Empty<IMMCAction>());
                }

                // Action is a terminal (leaf) action, so there are no counters to it
                return MMCResult.CreateScoreOnly(gameState.Score);
            }

            IMMCAction[] actions = gameState.SortedMoves;
            if (debug != null)
            {
                debug.AddActions(gameState, actions);
            }
            if (actions.Any() == false)
            {
                // Action is a terminal (leaf) action, so there are no counters to it
                return MMCResult.CreateScoreOnly(gameState.Score);
            }

            MMCResult? best = null;
            foreach (IMMCAction action in actions)
            {
                IMMCGameState postActionState = gameState.ApplyAction(action);

                MMCResult? result = GetBestAction_Internal(postActionState, upperBound, lowerBound, depth - 1, debug);
                if (result == null)
                {
                    throw new Exception($"Code failure: GetBestAction_Internal returned null for applying {postActionState} to {gameState}");
                }

                IMMCScore result_score = result.Score;
                if (debug != null)
                {
                    debug.AddResult(gameState, action, postActionState, result_score);
                }

                if (best == null)
                {
                    best = MMCResult.Create(action, result_score);
                }
                else if (IMMCScore.IsLeftGreaterThanRight(gameState.Player, result_score, best.Score))
                {
                    best = MMCResult.Create(action, result_score);
                }

                if (IMMCScore.IsLeftGreaterThanRight(gameState.Player, best.Score, lowerBound))
                {
                    lowerBound = result_score;
                }

                // #TODO: Check this?
                if (IMMCScore.IsLeftGreaterThanRight(gameState.Player, lowerBound, upperBound))
                {
                    return best;
                }
            }

            return best;
        }
    }
}
