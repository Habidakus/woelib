using static woelib.NegaMax.Request;

namespace woelib.NegaMax
{
    public interface IResponse
    {
    }

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

    public class PausedResponse : IResponse
    {
        public Request ContinuationRequest
        {
            get
            {
                return new Request(GameState, Depth, Timeout, BestAction, BestScore, AlphaScore, BetaScore, TurnColorEnum, RemainingAction, ChildResponse == null ? null : ChildResponse.ContinuationRequest);
            }
        }

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

        internal PausedResponse(Request request, INMAction? bestAction, NMScore bestScore, NMScore alphaScore, NMScore betaScore, Request.TurnColorEnum turnColor, INMAction[] iNMActions, PausedResponse? childPauseResponse)
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
        }
    }
}
