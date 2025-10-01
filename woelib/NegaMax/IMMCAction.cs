namespace woelib.NegaMax
{
    /// <summary>
    /// Representation of one valid move that the current player could take from a given [INMGameState].
    ///
    /// You must provide a derived class that extends [INMAction] such that it could be provided to the
    /// [method INMGameState.CreateChild] function and produce a new valid game state.
    /// </summary>
    public interface INMAction
    {
    }
}
