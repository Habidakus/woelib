namespace woelib.NegaMax
{
    /// <summary>
    /// Base class that represents a game state for the [NegaMax.Calculator]
    /// 
    /// When using the [NegaMax.Calculator] you should create your own class that derives from this class
    /// and provides it's own implementation of the four functions listed below.
    /// </summary>
    public interface INMGameState
    {
        /// <summary>
        /// Returns true if there are any moves that the current player could perform in the current game state.
        /// </summary>
        bool HasMoves { get; }

        /// <summary>
        /// Returns the current worth of the game state from the point of view of the Computer player (and thus
        /// [method NMScore.Reversed] would return the worth of the current game state from the point of view of the
        /// human opponent).
        /// </summary>
        NMScore Score { get; }

        /// <summary>
        /// Returns the list of all legal moves that the current player could make in the current game state.
        /// </summary>
        IEnumerable<INMAction> SortedMoves { get; }

        /// <summary>
        /// Returns a new game state object that represents what will happen, deterministically, to the current game
        /// state object if the given [INMAction] is applied to it.
        /// </summary>
        INMGameState CreateChild(INMAction action);
    }
}
