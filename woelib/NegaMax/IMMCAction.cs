namespace woelib.NegaMax
{
    /// <summary>
    /// Representation of one valid move that the current player could take from a given <see cref="INMGameState"/>.
    ///
    /// You must provide a derived class that extends <see cref="INMAction"/> such that it could be provided to the
    /// <see cref="INMGameState.CreateChild"/> function and produce a new valid game state.
    /// </summary>
    public interface INMAction
    {
    }
}
