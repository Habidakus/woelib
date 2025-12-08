namespace woelib.NegaMax
{
    public class NMScore
    {
        private enum ValueType
        {
            Regular,
            Sentinel_Min,
            Sentinel_Max,
        }
        private ValueType _type { get; }


        public override string ToString()
        {
            return $"<score={_type}/>";
        }

        protected virtual NMScore CreateReverse()
        {
            throw new NotImplementedException("You must extend NMScore and implement CreateReverse()");
        }
        protected virtual bool IsGreaterThan(NMScore other)
        {
            throw new NotImplementedException("You must extend NMScore and implement IsGreaterThan()");
        }

        public NMScore Reversed
        {
            get
            {
                if (_type == ValueType.Sentinel_Min)
                    return MaxValue;
                else if (_type == ValueType.Sentinel_Max)
                    return MinValue;
                else
                    return CreateReverse();
            }
        }

        private static NMScore _minValue = new(ValueType.Sentinel_Min);
        public static NMScore MinValue => _minValue;
        private static NMScore _maxValue = new(ValueType.Sentinel_Max);
        public static NMScore MaxValue => _maxValue;

        protected NMScore()
        {
            _type = ValueType.Regular;
        }

        private NMScore(ValueType type)
        {
            _type = type;
        }

        public static bool GreaterThan(NMScore left, NMScore right)
        {
            if (left._type == ValueType.Regular && right._type == ValueType.Regular)
                return left.IsGreaterThan(right);
            else if (left._type == right._type)
                return false;
            else if (left._type == ValueType.Sentinel_Min || right._type == ValueType.Sentinel_Max)
                return false;
            else if (left._type == ValueType.Sentinel_Max || right._type == ValueType.Sentinel_Min)
                return true;
            else
                return false;
        }

        public static bool GreaterOrEqualTo(NMScore alpha, NMScore beta)
        {
            return GreaterThan(alpha, beta) || !GreaterThan(beta, alpha);
        }

        public static NMScore Max(NMScore alpha, NMScore bestScore)
        {
            if (GreaterThan(alpha, bestScore))
                return alpha;
            else
                return bestScore;
        }
    }
}
