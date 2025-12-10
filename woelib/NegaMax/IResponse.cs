using static woelib.NegaMax.Request;

namespace woelib.NegaMax
{
    public interface IResponse
    {
    }

    /// <summary>
    /// The object returned by <see cref="Calculator.GetBestAction"/> when an optimal move has been determined. 
    /// Consult the <see cref="Action"/> property to retrieve the optimal move.
    /// </summary>
    public class ResolvedResponse : IResponse
    {
        public INMAction? Action { get; }
        public NMScore Score { get; }

        internal ResolvedResponse(INMAction? action, NMScore score)
        {
            Action = action;
            Score = score;
        }
    }

    /// <summary>
    /// An object indicating that an invocation to <see cref="Calculator.GetBestAction"/> has been paused and can be resumed later.
    /// </summary>
    /// <remarks>Use this class to obtain the necessary information to continue a previously paused operation.
    /// The response includes the current progress and a continuation request that can be used to resume processing from
    /// the paused state.</remarks>
    public class PausedResponse : IResponse
    {
        /// <summary>
        /// Gets a new request representing the continuation of the current operation, including updated state and
        /// progress information.
        /// </summary>
        /// <remarks>Use this property to obtain a request that reflects the current state and can be used
        /// to continue processing a previous call to <see cref="Calculator.GetBestAction"/>.</remarks>
        public Request ContinuationRequest
        {
            get
            {
                return new Request(GameState, FractionCompleted, Depth, Timeout, BestAction, BestScore, AlphaScore, BetaScore, TurnColorEnum, RemainingAction, ChildResponse == null ? null : ChildResponse.ContinuationRequest);
            }
        }

        /// <summary>
        /// Gets the fraction of the operation that has been completed, which can be helpful if you need to fill a progress bar or ETA clock.
        /// </summary>
        public float FractionCompleted { get; }

        private INMGameState GameState { get; }
        private Int32 Depth { get; }
        private TimeSpan Timeout { get; }
        private INMAction? BestAction { get; }
        private NMScore BestScore { get; }
        private NMScore AlphaScore { get; }
        private NMScore BetaScore { get; }
        private TurnColorEnum TurnColorEnum { get; }
        private INMAction[] RemainingAction { get; }
        private PausedResponse? ChildResponse { get; }

        internal PausedResponse(Request               request,
                                float                 remainingFractionCompleted,
                                INMAction?            bestAction,
                                NMScore               bestScore,
                                NMScore               alphaScore,
                                NMScore               betaScore,
                                Request.TurnColorEnum turnColor,
                                INMAction[]           iNMActions,
                                PausedResponse?       childPauseResponse)
        {
            GameState = request.GameState;
            Depth = request.Depth;
            Timeout = request.Timeout;
            BestAction = bestAction;
            BestScore = bestScore;
            AlphaScore = alphaScore;
            BetaScore = betaScore;
            TurnColorEnum = turnColor;
            RemainingAction = iNMActions;
            ChildResponse = childPauseResponse;
            FractionCompleted = request.FractionCompleted + remainingFractionCompleted * (1.0f - request.FractionCompleted);
        }
    }
}
