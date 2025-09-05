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
