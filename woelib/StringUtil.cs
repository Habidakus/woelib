namespace woelib
{
    public class StringUtil
    {
        // see https://en.wikipedia.org/wiki/Levenshtein_distance

        /// <summary>
        /// Measures the number of insertions, deletions, or substitutions it would take to change one sequence
        /// into another. The most common use case is trying to guess what keyword a person was trying to type
        /// when they mistype something on a command line and you want to suggest "did you mean X?"
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static int StrDist(string s, string t)
        {
            var m = s.Length;
            var n = t.Length;
            if (m == 0)
            {
                return n;
            }
            if (n == 0)
            {
                return m;
            }

            Span<int> d = stackalloc int[(m + 1) * (n + 1)];
            Func<int, int, int> GetIndex = (x, y) => x * (n + 1) + y;
            for (int i = 1; i <= m; ++i)
                d[GetIndex(i, 0)] = i;
            for (int j = 1; j <= n; ++j)
                d[GetIndex(0, j)] = j;

            for (int j = 1; j <= n; ++j)
            {
                for (int i = 1; i <= m; ++i)
                {
                    int substitutionCost = d[GetIndex(i - 1, j - 1)];
                    if (s[i-1] != t[j-1])
                        substitutionCost += 1;
                    int deletionCost = d[GetIndex(i - 1, j)] + 1;
                    int insertionCost = d[GetIndex(i, j - 1)] + 1;
                    d[GetIndex(i, j)] = Math.Min(Math.Min(deletionCost, insertionCost), substitutionCost);
                }
            }

            return d[GetIndex(m, n)];
        }
    }
}
