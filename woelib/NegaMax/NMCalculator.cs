namespace woelib.NegaMax
{
    public class NMCalculator
    {
        public static (NMScore score, INMAction? action) Calculate(INMCurrentBoard board, int depth)
        {
            return Calculate(board, depth, NMScore.MinValue, NMScore.MaxValue, 1);
        }

        public static (NMScore score, INMAction? action) Calculate(INMCurrentBoard board, int depth, NMScore alpha, NMScore beta, int color)
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
                INMCurrentBoard child = board.CreateChild(action);
                (NMScore score, INMAction? action) childResult = Calculate(child, depth - 1, beta.Reversed, alpha.Reversed, 0 - color);
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
