namespace woelib
{
    public class Correlation
    {
        /// <summary>
        /// Calculate the Phi correlation between two lists of booleans.
        /// https://en.wikipedia.org/wiki/Phi_coefficient
        /// </summary>
        /// <param name="listA">The first set of data which we will correlate against list B.</param>
        /// <param name="listB">The first set of data which we will correlate against list A.</param>
        /// <param name="coverage">How reliable the correlation is; measured from 0 (some cases not seen) to 1 (all cases seen robustly).</param>
        /// <returns></returns>
        public static double CalculatePhi(List<bool> listA, List<bool> listB, out double coverage)
        {
            if (listA.Count != listB.Count)
                throw new Exception("Both lists must be of equal length");

            int nTT = 0;
            int nTF = 0;
            int nFT = 0;
            int nFF = 0;

            int ncT = 0;
            int ncF = 0;
            int nTc = 0;
            int nFc = 0;

            for (int i = 0; i < listA.Count; ++i)
            {
                if (listA[i])
                {
                    if (listB[i])
                    {
                        ++nTT;
                        ++ncT;
                    }
                    else
                    {
                        ++nTF;
                        ++ncF;
                    }

                    ++nTc;
                }
                else
                {
                    if (listB[i])
                    {
                        ++nFT;
                        ++ncT;
                    }
                    else
                    {
                        ++nFF;
                        ++ncF;
                    }

                    ++nFc;
                }
            }

            int divisor = ncT * ncF * nFc * nTc;
            if (divisor == 0)
            {
                coverage = 0;
                return Double.NaN;
            }
            else
            {
                coverage = (double)Math.Min(Math.Min(nTT, nTF), Math.Min(nFT, nFF)) / (double)listA.Count;
                return (nTT * nFF - nTF * nFT) / Math.Sqrt(divisor);
            }
        }

        /// <summary>
        /// How much of a correlation is there between list A and list B.
        /// https://en.wikipedia.org/wiki/Pearson_correlation_coefficient
        /// </summary>
        /// <param name="listA">The first set of data which we will correlate against list B.</param>
        /// <param name="listB">The first set of data which we will correlate against list A.</param>
        /// <returns>How strongly the two values correlate; where 0 means no correlation at all, and 1 is a strong possitive correlation (and -1 is a strong negative correlation).</returns>
        public static double CalculatePearson(List<double> listA, List<double> listB)
        {
            if (listA.Count != listB.Count)
                throw new Exception("Both lists must be of equal length");

            double leftSum = 0;
            double rightSum = 0;
            double leftSquareSum = 0;
            double rightSquareSum = 0;
            double leftRightSum = 0;
            for (int i = 0; i<listA.Count; ++i)
            {
                leftSum += listA[i];
                rightSum += listB[i];
                leftRightSum += (listA[i] * listB[i]);
                leftSquareSum += (listA[i] * listA[i]);
                rightSquareSum += (listB[i] * listB[i]);
            }

            int count = listA.Count;
            return (count * leftRightSum - leftSum * rightSum) /
                Math.Sqrt(
                    (count * leftSquareSum - leftSum * leftSum) *
                    (count * rightSquareSum - rightSum * rightSum)
                    );
        }

        private static List<double> Rankify<T>(List<T> list) where T : IComparable<T>
        {
            List<Tuple<int, T>> sortedRanks = [];
            for (int i = 0; i < list.Count; ++i)
            {
                sortedRanks.Add(new Tuple<int, T>(i, list[i]));
            }

            int totalDiffValues = 0;
            sortedRanks.Sort((a, b) => a.Item2.CompareTo(b.Item2));
            for (int i = 1; i < sortedRanks.Count; ++i)
            {
                if (sortedRanks[i - 1].Item2.CompareTo(sortedRanks[i].Item2) != 0)
                ++totalDiffValues;
            }

            List<Tuple<int, double>> unList = [new Tuple<int, double>(sortedRanks[0].Item1, 0.0)];

            int index = 0;
            for (int i = 1; i < sortedRanks.Count; ++i)
            {
                if (sortedRanks[i - 1].Item2.CompareTo(sortedRanks[i].Item2) != 0)
                    ++index;

                unList.Add(new Tuple<int, double>(sortedRanks[i].Item1, (double)index / totalDiffValues));
            }

            unList.Sort((a,b) => a.Item1.CompareTo(b.Item1));
            return unList.Select(a => a.Item2).ToList();
        }

        /// <summary>
        /// Generate a Spearman's correlation for any two lists of comparable data.
        /// https://en.wikipedia.org/wiki/Spearman%27s_rank_correlation_coefficient
        /// </summary>
        /// <param name="listA">The first set of data which we will correlate against list B. Note that listA only has to be IComparable against other items in list A, there does not necessarily have to be any comparison between an item on list A and an item on list B.</param>
        /// <param name="listB">The first set of data which we will correlate against list A. Note that listB only has to be IComparable against other items in list B, there does not necessarily have to be any comparison between an item on list A and an item on list B.</param>
        /// <returns>How strongly the two values correlate; where 0 means no correlation at all, and 1 is a strong possitive correlation (and -1 is a strong negative correlation).</returns>
        /// <exception cref="Exception"></exception>
        public static double CalculateSpearman<T,B>(List<T> listA, List<B> listB) where T : IComparable<T> where B : IComparable<B>
        {
            if (listA.Count != listB.Count)
                throw new Exception("Both lists must be of equal length");

            if (listA.Count < 2)
                return double.NaN;

            return CalculatePearson(Rankify(listA), Rankify(listB));
        }

        /// <summary>
        /// Given any two lists of doubles of any range, generate where they would be placed on a 2d plot and where the trendline should be placed.
        /// </summary>
        /// <param name="listX">The list of data that is to be plotted against the X axis</param>
        /// <param name="listY">The list of data that is to be plotted against the Y axis</param>
        /// <param name="slope">Describes the slope of the data's trendline such that y = x * slope + offset</param>
        /// <param name="offset">Describes the offset of the data's trendline such that y = x * slope + offset</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static List<Tuple<double, double>> CalculateGraphAndTrendLine(
            List<double> listX,
            List<double> listY,
            out double slope,
            out double offset)
        {
            if (listX.Count != listY.Count)
                throw new Exception("Both lists must be of equal length");

            double min_x = double.MaxValue;
            double min_y = double.MaxValue;
            double max_x = double.MinValue;
            double max_y = double.MinValue;
            for (int i = 0; i < listX.Count; i++)
            {
                min_x = Math.Min(min_x, listX[i]);
                min_y = Math.Min(min_y, listY[i]);
                max_x = Math.Max(max_x, listX[i]);
                max_y = Math.Max(max_y, listY[i]);
            }

            double range_x = max_x - min_x;
            double range_y = max_y - min_y;

            double xSum = 0;
            double xxSum = 0;
            double ySum = 0;
            double xySum = 0;

            List<Tuple<double, double>> graph = [];

            for (int z = 0; z < listX.Count; ++z)
            {
                double x = (listX[z] - min_x) / range_x;
                double y = (listY[z] - min_y) / range_y;
                graph.Add(new Tuple<double, double>(x, y));
                xSum += x;
                ySum += y;
                xxSum += (x * x);
                xySum += (x * y);
            }

            slope = ((listX.Count * xySum) - (xSum * ySum)) / ((listX.Count * xxSum) - (xSum * xSum));
            offset = (ySum - slope * xSum) / listX.Count;

            return graph;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="B"></typeparam>
        /// <param name="listX"></param>
        /// <param name="listY"></param>
        /// <param name="slope"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static List<Tuple<double, double>> CalculateGraphAndTrendLine<T, B>(
            List<T> listX,
            List<B> listY,
            out double slope,
            out double offset) where T : IComparable<T> where B : IComparable<B>
        {
            return CalculateGraphAndTrendLine(Rankify(listX), Rankify(listY), out slope, out offset);
        }
    }
}
