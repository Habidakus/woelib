namespace woelib.MinMaxCalculator
{
    /// <summary>
    /// Base class that represents a game state for the [MinMaxCalculator]
    /// 
    /// When using the [MinMaxCalculator] you should create your own class that derives from this class
    /// and provides it's own implementation of the three functions listed below.
    /// 
    /// <example>
    /// <code>
    /// class_name MyGameState extends MMCGameState
    /// 
    /// func apply_action(action : MMCAction) -> MMCGameState:
    ///     var ret_val : MyGameState = MyGameState.new()
    ///     # ... code that applys MyAction to the current game state
    ///     return ret_val
    /// </code>
    /// </example>
    /// </summary>
    public interface IMMCGameState
    {
        /// <summary>
        /// Returns the current worth of the game state from the point of view of the player who provided
        /// the action that created this game state instance. For instance, if in chess and black has
        /// just taken the opponent's queen, then this score would be very high as seen from the point of
        /// view of the black pieces.
        ///</summary>
        public IMMCScore Score { get; }

        /// <summary>
        /// Returns the list of all legal moves that the current player could make in the current game state.
        /// </summary>
        public IMMCAction[] SortedMoves { get; }

        /// <summary>
        /// The current player (which all the moves returned by the [property Moves] property are for).
        /// </summary>
        public int Player { get; }

        /// <summary>
        /// Returns a new game state object that represents what will happen, deterministically, to the current game
        /// state object if the given [MMCAction] is applied to it.
        /// </summary>
        public IMMCGameState ApplyAction(IMMCAction action);
    }
}
