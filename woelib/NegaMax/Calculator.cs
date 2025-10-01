namespace woelib.NegaMax
{
    ///<summary>
    /// An generic alpha beta pruning tree to implement deterministic best move calculations.
    ///
    /// Every time you wish to have the artificial opponent take a turn, you'll need to supply the [method Calculator.GetBestAction]
    /// function with a representation of the current game state. To do this first create an extension
    /// of the [INMGameState] interface which can hold within it a flyweight representation of the current state of the
    /// game - your class will need to implement your own version of the three base functions of that class;
    /// [method INMGameState.Score], [method INMGameState.SortedMoves], and [method INMGameState.CreateChild].
    /// Next you'll need to implement the [INMAction] interface and extend the [NMScore] class. Once you have all three,
    /// you will be able to ask the [method Calculator.GetBestAction] for an optimal move for the artificial opponent to take.
    ///
    /// This class is an implementation of https://en.wikipedia.org/wiki/Negamax
    ///</summary>
    public class Calculator
    {
        ///<summary>
        /// Given a game state, calculate which potential action is the optimal next move. For very simple
        /// game states with limited moves, the depth value can be omitted and the engine will attempt to
        /// calculate the entire game tree. However for most interesting games (eg, not tic-tac-toe) you will
        /// want to specify a maximum depth for the engine to consider, otherwise the search space will be
        /// far too large and the game will appear to stop.
        ///</summary>
        public static INMAction? GetBestAction(INMGameState gameState, Int32 depth = Int32.MaxValue)
        {
            return GetBestAction_Internal(gameState, depth, NMScore.MinValue, NMScore.MaxValue, color: 1).action;
        }

        private static (NMScore score, INMAction? action) GetBestAction_Internal(INMGameState board, int depth, NMScore alpha, NMScore beta, int color)
        {
            if (depth == 0 || !board.HasMoves)
            {
                if (color > 0)
                    return (board.Score, null);
                else
                    return (board.Score.Reversed, null);
            }

            NMScore bestScore = NMScore.MinValue;
            INMAction? bestAction = null;

            foreach (INMAction action in board.SortedMoves)
            {
                var child = board.CreateChild(action);

                var childResult = GetBestAction_Internal(child, depth - 1, beta.Reversed, alpha.Reversed, 0 - color);
                NMScore reversedScore = childResult.score.Reversed;
                if (bestAction == null || NMScore.GreaterThan(reversedScore, bestScore))
                {
                    bestScore = reversedScore;
                    bestAction = action;
                }

                alpha = NMScore.Max(alpha, bestScore);
                if (NMScore.GreaterOrEqualTo(alpha, beta))
                {
                    break;
                }
            }

            return (bestScore, bestAction);
        }
    }
}
