namespace woelib.NegaMax
{
    public interface INMCurrentBoard
    {
        public bool HasMoves { get; }
        public NMScore Score { get; }
        public IEnumerable<INMAction> SortedMoves { get; }

        public INMCurrentBoard CreateChild(INMAction action);
    }
}
