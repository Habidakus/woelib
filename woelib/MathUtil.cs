using System.Numerics;

namespace woelib
{
    public class MathUtil
    {
        const float EPSILON = 0.00000001f;

        /// <summary>
        /// Given two line segments, find the point at which they intersect, if there is any single point. Note that two colinear lines will return null here.
        /// </summary>
        /// <returns>Null if there is no single intersection point, or the point if there is one.</returns>
        public static Vector2? FindIntersectPoint(Vector2 a_start, Vector2 a_end, Vector2 b_start, Vector2 b_end)
        {
            float CrossProduct(Vector2 a, Vector2 b)
            {
                return a.X * b.Y - a.Y * b.X;
            }

            Vector2 a_segment = a_end - a_start;
            Vector2 b_segment = b_end - b_start;
            float cross_product = CrossProduct(a_segment, b_segment);
            if (Math.Abs(cross_product) <= EPSILON)
                return null;

            Vector2 start_segment = b_start - a_start;
            float t = CrossProduct(start_segment, b_segment) / cross_product;
            if (t >= 0 && t <= 1)
            {
                float u = CrossProduct(start_segment, a_segment) / cross_product;
                if (u >= 0 && u <= 1)
                {
                    return a_start + t * a_segment;
                }
            }
            
            return null;
        }

        /// <summary>
        /// Given two line segments, determine if they intersect.
        /// </summary>
        /// <returns>True if the two lines intersect.</returns>
        public static bool Do2DLinesIntersect(Vector2 a_start, Vector2 a_end, Vector2 b_start, Vector2 b_end)
        {
            bool IsCloseToZero(float val)
            {
                return Math.Abs(val) <= EPSILON;
            }

            float Orientation(Vector2 a, Vector2 b, Vector2 c)
            {
                return (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
            }

            bool OnSegment(Vector2 start, Vector2 point, Vector2 end)
            {
                return point.X <= Math.Max(start.X, end.X) && point.X >= Math.Min(start.X, end.X) &&
                       point.Y <= Math.Max(start.Y, end.Y) && point.Y >= Math.Min(start.Y, end.Y);
            }

            float o1 = Orientation(a_start, a_end, b_start);
            float o2 = Orientation(a_start, a_end, b_end);
            float o3 = Orientation(b_start, b_end, a_start);
            float o4 = Orientation(b_start, b_end, a_end);

            if (o1 * o2 < 0 && o3 * o4 < 0)
                return true;

            if (IsCloseToZero(o1) && OnSegment(a_start, b_start, a_end))
                return true;
            if (IsCloseToZero(o2) && OnSegment(a_start, b_end, a_end))
                return true;
            if (IsCloseToZero(o3) && OnSegment(b_start, a_start, b_end))
                return true;
            if (IsCloseToZero(o4) && OnSegment(b_start, a_end, b_end))
                return true;

            return false;
        }

        /// <summary>
        /// Given a set of comparable items, order the set such that all elements before the Nth element are "less than"
        /// the Nth element, and all elements after the Nth element are greater in value - but unlike sort routines, the
        /// order of the elements in their sub-sets before and after the Nth position are not sorted.
        /// 
        /// This is useful if you need, say, the 5 largest cookies but don't care which order they're in, just that they
        /// are the 5 largest, and it's important to find them with fewer comparisons than traditional sort algorithms.
        /// </summary>
        /// <param name="span">The set of all elements - this set will be re-arranged by the algorthim.</param>
        /// <param name="nth">The index within the set at which all elements before it will be less than it's value,
        /// and all elements after it will be greater.</param>
        /// <param name="comp">The comparison implementation used to determine the ordering of any two elements.</param>
        public static void NthElement<T>(Span<T> span, int nth, IComparer<T> comp)
        {
            if (span.Length < 2)
                return;

            int pivot = Partition(span, comp);
            if (pivot == nth)
                return;

            if (pivot < nth)
            {
                Span<T> rightSlice = span.Slice(pivot + 1);
                NthElement(rightSlice, nth - (pivot + 1), comp);
            }
            else // pivot > nth
            {
                Span<T> leftSlice = span.Slice(0, pivot);
                NthElement(leftSlice, nth, comp);
            }
        }

        /// <summary>
        /// Given a set of comparable items, order the set such that all elements before the Nth element are "less than"
        /// the Nth element, and all elements after the Nth element are greater in value - but unlike sort routines, the
        /// order of the elements in their sub-sets before and after the Nth position are not sorted.
        /// 
        /// This is useful if you need, say, the 5 largest cookies but don't care which order they're in, just that they
        /// are the 5 largest, and it's important to find them with fewer comparisons than traditional sort algorithms.
        /// </summary>
        /// <param name="span">The set of all elements - this set will be re-arranged by the algorthim.</param>
        /// <param name="nth">The index within the set at which all elements before it will be less than it's value,
        /// and all elements after it will be greater.</param>
        public static void NthElement<T>(Span<T> span, int nth) where T : IComparable<T>
        {
            if (span.Length < 2)
                return;

            int pivot = Partition(span);
            if (pivot == nth)
                return;

            if (pivot < nth)
            {
                Span<T> rightSlice = span.Slice(pivot + 1);
                NthElement(rightSlice, nth - (pivot + 1));
            }
            else // pivot > nth
            {
                Span<T> leftSlice = span.Slice(0, pivot);
                NthElement(leftSlice, nth);
            }
        }

        private static int Partition<T>(Span<T> span, IComparer<T> comp)
        {
            int i = 0;
            int j = span.Length - 1;
            while (i < j)
            {
                while (i <= (span.Length - 2) && comp.Compare(span[i], span[0]) <= 0)
                {
                    ++i;
                }

                while (j >= 1 && comp.Compare(span[0], span[j]) <= 0)
                {
                    --j;
                }

                if (i < j)
                {
                    T tj = span[j];
                    span[j] = span[i];
                    span[i] = tj;
                }
            }

            T te = span[0];
            span[0] = span[j];
            span[j] = te;

            return j;
        }

        private static int Partition<T>(Span<T> span) where T : IComparable<T>
        {
            int i = 0;
            int j = span.Length - 1;
            while (i < j)
            {
                while (i <= (span.Length - 2) && span[i].CompareTo(span[0]) <= 0)
                {
                    ++i;
                }

                while (j >= 1 && span[0].CompareTo(span[j]) <= 0)
                {
                    --j;
                }

                if (i < j)
                {
                    T tj = span[j];
                    span[j] = span[i];
                    span[i] = tj;
                }
            }

            T te = span[0];
            span[0] = span[j];
            span[j] = te;

            return j;
        }

        /// <summary>
        /// Calculate the Phi correlation between two lists of booleans.
        /// https://en.wikipedia.org/wiki/Phi_coefficient
        /// </summary>
        /// <param name="listA">The first set of data which we will correlate against list B.</param>
        /// <param name="listB">The first set of data which we will correlate against list A.</param>
        /// <param name="coverage">How reliable the correlation is; measured from 0 (some cases not seen) to 1 (all cases seen robustly).</param>
        /// <returns></returns>
        public static double CalculatePhiCorrelation(List<bool> listA, List<bool> listB, out double coverage)
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
        public static double CalculatePearsonCorrelation(List<double> listA, List<double> listB)
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
        public static double CalculateSpearmanCorrelation<T,B>(List<T> listA, List<B> listB) where T : IComparable<T> where B : IComparable<B>
        {
            if (listA.Count != listB.Count)
                throw new Exception("Both lists must be of equal length");

            if (listA.Count < 2)
                return double.NaN;

            return CalculatePearsonCorrelation(Rankify(listA), Rankify(listB));
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
