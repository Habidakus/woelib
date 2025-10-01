namespace woelib.NegaMax
{
    /// <summary>
    /// Representation of one valid move that a player could take from a given [MMCGameState]
    ///
    /// You must provide a derived class that extends [MMCAction] such that it could be provided to the
    /// [method INMGameState.CreateChild] function and produce a new valid state.
    /// </summary>
    public interface INMAction
    {
    }
}
