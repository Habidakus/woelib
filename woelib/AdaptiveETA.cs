namespace woelib
{
    public class AdaptiveETA
    {
        readonly double _goal;
        int _acquired = 0;
        double[] _values = new double[7];
        DateTime[] _dates = new DateTime[7];

        private int GetLeastValuableIndex(DateTime date)
        {
            if (_acquired < 7)
                throw new Exception("LeastValuableIndex cannot be referenced before full");

            double smallestSpanLength = double.MaxValue;
            int smallestSpan = -1;
            for (int i = 1; i < 6; ++i)
            {
                double logOfSpan = Math.Log((_dates[i + 1] - _dates[i - 1]).Ticks);
                if (logOfSpan < smallestSpanLength)
                {
                    smallestSpan = i;
                    smallestSpanLength = logOfSpan;
                }
            }

            if (smallestSpanLength < Math.Log((date - _dates[6]).Ticks))
            {
                return smallestSpan;
            }
            else
            {
                return 6;
            }
        }

        public AdaptiveETA(double goal)
        {
            _goal = goal;
        }

        public void Add(double value, DateTime date)
        {
            if (_acquired < _values.Count())
            {
                _values[_acquired] = value;
                _dates[_acquired] = date;
                ++_acquired;
                return;
            }

            int index = GetLeastValuableIndex(date);
            int lastIndex = _values.Count() - 1;
            if (index < lastIndex)
            {
                for (int i = index; i < lastIndex; ++i)
                {
                    _values[i] = _values[i + 1];
                    _dates[i] = _dates[i + 1];
                }
            }

            _values[lastIndex] = value;
            _dates[lastIndex] = date;
        }

        public void Add(double value)
        {
            Add(value, DateTime.Now);
        }

        public (double currentAmount, DateTime eta) GetEstimate(DateTime date)
        {
            if (_acquired == 0)
                return (double.NaN, DateTime.MaxValue);
            if (_acquired == 1)
                return (_values[0], DateTime.MaxValue);

            int lastAcquired = _acquired - 1;
            double workRemaining = _goal - _values[lastAcquired];

            // Perform simple estimate
            if (_acquired < 4)
            {
                double completed = _values[lastAcquired] - _values[0];
                double totalSeconds = (_dates[lastAcquired] - _dates[0]).TotalSeconds;
                if (totalSeconds == 0)
                {
                    return (_goal, DateTime.Now);
                }

                double amountPerSecond = (_values[lastAcquired] - _values[0]) / totalSeconds;
                if (amountPerSecond == 0)
                {
                    return (_values[lastAcquired], DateTime.MaxValue);
                }

                double secondsRemaining = workRemaining / amountPerSecond;
                double currentAmount = _values[lastAcquired] + (date - _dates[lastAcquired]).TotalSeconds * amountPerSecond;
                try
                {
                    return (currentAmount, _dates[lastAcquired] + TimeSpan.FromSeconds(secondsRemaining));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            int lastIndex = _values.Count() - 1;
            if (_acquired < lastIndex)
            {
                double max = double.MinValue;
                double min = double.MaxValue;
                double totalAmountsPerSecond = 0;
                for (int i = 0; i < _acquired - 1; ++i)
                {
                    double aps = (_values[i + 1] - _values[i]) / (_dates[i + 1] - _dates[i]).TotalSeconds;
                    if (aps > max)
                        max = aps;
                    if (aps < min)
                        min = aps;

                    totalAmountsPerSecond += aps;
                }

                // Remove the two most extreme values
                totalAmountsPerSecond -= (min + max);

                // Now compute average amount per second
                double amountPerSecond = totalAmountsPerSecond / (_acquired - 3);

                double secondsRemaining = workRemaining / amountPerSecond;
                double currentAmount = _values[lastAcquired] + (date - _dates[lastAcquired]).TotalSeconds * amountPerSecond;
                return (currentAmount, _dates[lastAcquired] + TimeSpan.FromSeconds(secondsRemaining));
            }
            else
            {
                Span<double> amountsPerSecond = stackalloc double[6];
                for (int i = 0; i < 6; ++i)
                {
                    double aps = (_values[i + 1] - _values[i]) / (_dates[i + 1] - _dates[i]).TotalSeconds;
                    amountsPerSecond[i] = aps;
                }

                // Remove the four most extreme values
                MathUtil.NthElement(amountsPerSecond, 1);
                Span<double> subSlice = amountsPerSecond.Slice(2);
                MathUtil.NthElement(subSlice, 1);
                double amountPerSecond = (subSlice[0] + subSlice[1]) / 2.0;
                double secondsRemaining = workRemaining / amountPerSecond;
                double currentAmount = _values[lastAcquired] + (date - _dates[lastAcquired]).TotalSeconds * amountPerSecond;
                return (currentAmount, _dates[lastAcquired] + TimeSpan.FromSeconds(secondsRemaining));
            }
        }
    }
}
