namespace woelib.MinMaxCalculator
{
    /// <summary>
    /// Representation of worth that any given [MMCAction] is if committed to a [MMCGameState]
    /// 
    /// You must provide a derived class that extends [MMCScore] and your own implementation of the
    /// [method reversed] and [method is_better_than] functions.
    /// </summary>
    public interface IMMCScore
    {
        /// <summary>
        /// Compare the current score with another one, and return true if the current score is certainly
        /// better than the other one (eg: should return false if they are essentially equal, let alone worse).
        /// </summary>
        public static bool IsLeftGreaterThanRight(int player, IMMCScore left, IMMCScore right)
        {
            if (left == Highest || right == Lowest)
                return true;
            if (left == Lowest || right == Highest)
                return false;
            return left.IsGreaterThan(player, right);
        }

        public bool IsGreaterThan(int player, IMMCScore other);

        public static IMMCScore Lowest => MMCConstScore._lowest;
        public static IMMCScore Highest => MMCConstScore._highest;
    }

    /// <summary>
    /// Helper class for MinMaxCalculator.  There's nothing you need to see here, unless you're just looking around.
    /// </summary>
    internal class MMCConstScore : IMMCScore
    {
        internal static MMCConstScore _lowest = new MMCConstScore();
        internal static MMCConstScore _highest = new MMCConstScore();

        public bool IsGreaterThan(int player, IMMCScore other)
        {
            return (this == _highest);
        }
    }
}
