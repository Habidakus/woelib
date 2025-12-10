using System;

namespace woelib.NegaMax
{
    public class Request
    {
        public INMGameState GameState { get; }
        public Int32 Depth { get; }
        public TimeSpan Timeout { get; }
        public DateTime ExpirationTime { get; }
        private NMScore AlphaScore { get; set; } = NMScore.MinValue;
        private NMScore BetaScore { get; set; } = NMScore.MaxValue;
        internal float FractionCompleted { get; } = 0.0f;
        internal enum TurnColorEnum
        {
            Invoker = 1,
            Opponent = -1,
        }
        private TurnColorEnum TurnColor { get; }

        /// <summary>
        /// Passing this object to the <see cref="Calculator.GetBestAction"/> method will cause the engine to consult a <see cref="depth"/> deep alpha-beta pruning tree to determine the optimal next move from the given <see cref="INMGameState"/>.
        /// </summary>
        public Request(INMGameState gameState, Int32 depth = Int32.MaxValue)
        {
            GameState = gameState;
            Depth = depth;
            Timeout = TimeSpan.MaxValue;
            ExpirationTime = DateTime.MaxValue;
            TurnColor = TurnColorEnum.Invoker;
        }

        /// <summary>
        /// Passing this object to the <see cref="Calculator.GetBestAction"/> method will cause the engine to
        /// begin consulting an alpha-beta pruning tree to determine the optimal next move
        /// from the given <see cref="INMGameState"/>.
        /// </summary>
        /// <param name="depth">
        /// How many moves ahead the engine should consider when calculating the best move.
        /// </param>
        /// <param name="timeout">
        /// Limit the amount of time the engine is allowed to spend calculating the best move. If the timeout
        /// is reached before the calculation is complete, a <see cref="PausedResponse"/> will be returned which can be
        /// used to resume the calculation later.
        /// </param>
        public Request(INMGameState gameState, Int32 depth, TimeSpan timeout)
        {
            GameState = gameState;
            Depth = depth;
            Timeout = timeout;
            ExpirationTime = DateTime.Now + timeout;
            TurnColor = TurnColorEnum.Invoker;
        }

        internal bool UpdateAlphaAndCheck(NMScore newScore)
        {
            AlphaScore = NMScore.Max(AlphaScore, newScore);
            return NMScore.GreaterOrEqualTo(AlphaScore, BetaScore);
        }

        private Request(Request parent, INMAction action)
        {
            GameState = parent.GameState.CreateChild(action);
            Depth = parent.Depth - 1;
            Timeout = parent.Timeout;
            ExpirationTime = parent.ExpirationTime;
            AlphaScore = parent.BetaScore.Reversed;
            BetaScore = parent.AlphaScore.Reversed;
            TurnColor = (parent.TurnColor == TurnColorEnum.Invoker)
                ? TurnColorEnum.Opponent
                : TurnColorEnum.Invoker;
        }

        private INMAction[]? ContinuationActions { get; } = null;
        private Request? FirstChildRequest { get; } = null;
        internal INMAction? InitialBestAction { get; } = null;
        internal NMScore InitialBestScore { get; } = NMScore.MinValue;

        internal Request(INMGameState  gameState,
                         float         fractionCompleted,
                         int           depth,
                         TimeSpan      timeout,
                         INMAction?    bestAction,
                         NMScore       bestScore,
                         NMScore       alphaScore,
                         NMScore       betaScore,
                         TurnColorEnum turnColorEnum,
                         INMAction[]   remainingAction,
                         Request?      childRequest)
            : this(gameState, depth, timeout)
        {
            AlphaScore = alphaScore;
            BetaScore = betaScore;
            TurnColor = turnColorEnum;
            InitialBestScore = bestScore;
            InitialBestAction = bestAction;
            ContinuationActions = remainingAction;
            FirstChildRequest = childRequest;
            FractionCompleted = fractionCompleted;
        }

        internal Request CreateChild(bool firstChild, INMAction action)
        {
            if (firstChild && FirstChildRequest != null)
            {
                return FirstChildRequest;
            }

            return new(this, action);
        }

        internal INMAction[] SortedActions
        {
            get
            {
                if (ContinuationActions != null)
                {
                    return ContinuationActions;
                }
                else
                {
                    return GameState.SortedMoves.ToArray();
                }
            }
        }


        internal PausedResponse CreatePauseResponse(INMAction? bestAction, NMScore bestScore, float remainingFractionCompleted, Span<INMAction> remainingActions, PausedResponse? childPauseResponse = null)
        {
            return new PausedResponse(this, remainingFractionCompleted, bestAction, bestScore, AlphaScore, BetaScore, TurnColor, remainingActions.ToArray(), childPauseResponse);
        }

        internal ResolvedResponse ExaustResponse
        {
            get
            {
                switch (TurnColor)
                {
                    case TurnColorEnum.Invoker:
                        return new ResolvedResponse(null, GameState.Score);
                    case TurnColorEnum.Opponent:
                        return new ResolvedResponse(null, GameState.Score.Reversed);
                    default:
                        throw new InvalidOperationException($"Unrecognized TurnColorEnum:{TurnColor}");
                }
            }
        }
    }
}
