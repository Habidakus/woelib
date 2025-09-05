namespace woelib.MinMaxCalculator
{
    /// <summary>
    /// Helper class for MinMaxCalculator. There's nothing you need to see here, unless you're just looking around.
    /// </summary>
    internal class MMCResult
    {
        public IMMCScore Score { get; }
        public IMMCAction? Action { get; }

        internal static MMCResult CreateScoreOnly(IMMCScore score)
        {
            return new MMCResult(score, null);
        }

        internal static MMCResult Create(IMMCAction action, IMMCScore score)
        {
            return new MMCResult(score, action);
        }

        private MMCResult(IMMCScore score, IMMCAction? action)
        {
            Score = score;
            Action = action;
        }
    }
}
