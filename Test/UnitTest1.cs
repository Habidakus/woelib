using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using woelib;

namespace Test
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    [TestClass]
    public class CorrelationTest
    {
        [TestMethod]
        public void CalculatePhiTest()
        {
            List<bool> listA = [true, true, false, true, true, false, true, true];
            List<bool> listB = [false, false, true, false, false, true, false, false];
            var cor_AA = Correlation.CalculatePhi(listA, listA, out double cor_AA_coverage);
            Assert.IsTrue(cor_AA == 1);
            Assert.IsTrue(cor_AA_coverage == 0);
            var cor_AB = Correlation.CalculatePhi(listA, listB, out double cor_AB_coverage);
            Assert.IsTrue(cor_AB == -1);
            Assert.IsTrue(cor_AB_coverage == 0);
            List<bool> listC = [true, true, true, false, true, false, true, true];
            var cor_AC = Correlation.CalculatePhi(listA, listC, out double cor_AC_coverage);
            Assert.IsTrue(cor_AC > 0.1);
            Assert.IsTrue(cor_AC_coverage > 0.1);
            List<bool> listD = [false, false, true, false, false, true, false, true];
            var cor_AD = Correlation.CalculatePhi(listA, listD, out double cor_AD_coverage);
            Assert.IsTrue(cor_AD < 0.5);
            Assert.IsTrue(cor_AD_coverage == 0);
        }

        [TestMethod]
        public void CalculatePearsonTest()
        {
            List<double> listA = [];
            List<double> listB = [];
            List<double> listC = [];
            List<double> listD = [];
            for (double i = -2.5; i < 10; i += 0.1)
            {
                listA.Add(i);
                listB.Add(0 - i);
                listC.Add(i * i);
                listD.Add(i * (1.0 - i));
            }

            var cor_AA = Correlation.CalculatePearson(listA, listA);
            Assert.IsTrue(cor_AA == 1.0);
            var cor_AB = Correlation.CalculatePearson(listA, listB);
            Assert.IsTrue(cor_AB == -1.0);
            var cor_AC = Correlation.CalculatePearson(listA, listC);
            Assert.IsTrue(cor_AC > 0.9);
            var cor_AD = Correlation.CalculatePearson(listA, listD);
            Assert.IsTrue(cor_AD < -0.85);
            var cor_CD = Correlation.CalculatePearson(listC, listD);
            Assert.IsTrue(cor_CD < -0.95);
        }

        internal class ComparableColor : IComparable<ComparableColor>
        {
            private readonly Color _color;
            float Brightness { get { return _color.R + _color.G + _color.B; } }

            internal ComparableColor(string hexCode)
            {
                _color = ColorTranslator.FromHtml(hexCode);
            }
            int IComparable<ComparableColor>.CompareTo(ComparableColor? other)
            {
                if (other == null)
                {
                    return -1;
                }

                return Brightness.CompareTo(other.Brightness);
            }
        }

        // Name, kg, meters, max age, heart rate
        internal static List<Tuple<string, double, double, double, double, ComparableColor>> _animals = [
            new Tuple<string, double, double, double, double, ComparableColor>("Elephant", 6800, 6.5, 74, 30, new ComparableColor("#123447")),
            new Tuple<string, double, double, double, double, ComparableColor>("Human", 62, 1.7, 122, 69, new ComparableColor("#e0ac69")),
            new Tuple<string, double, double, double, double, ComparableColor>("Tiger", 300, 3.3, 20, (56 + 97) / 2, new ComparableColor("#C88141")),
            new Tuple<string, double, double, double, double, ComparableColor>("Ant Eater", 45, 2.4, 26, 135, new ComparableColor("#aa6600")),
            new Tuple<string, double, double, double, double, ComparableColor>("Humming Bird", 0.011, 0.13, 10, 1200, new ComparableColor("#007F8B")),
            new Tuple<string, double, double, double, double, ComparableColor>("Rat", 0.34, 0.25, 7, 325, new ComparableColor("#6D7B8D")),
            new Tuple<string, double, double, double, double, ComparableColor>("Dolphin", 200, 2.59, 60, 96, new ComparableColor("#646077")),
            new Tuple<string, double, double, double, double, ComparableColor>("Sperm Whale", 50000, 18, 70, 15, new ComparableColor("#190140")),
            new Tuple<string, double, double, double, double, ComparableColor>("Hippo", 1480, 3.5, 61, 60, new ComparableColor("#b3b8c7")),
            new Tuple<string, double, double, double, double, ComparableColor>("Gorrila", 180, 1.7, 50, 73.8, new ComparableColor("#575b5e")),
            new Tuple<string, double, double, double, double, ComparableColor>("Lion", 190, 2, 16, (42 + 76) / 2, new ComparableColor("#DECC9C")),
            new Tuple<string, double, double, double, double, ComparableColor>("Bear", 315, 2.7, 44, 85, new ComparableColor("#5F483B")),
            new Tuple<string, double, double, double, double, ComparableColor>("Possum", 2.8, 0.406, 4, 210, new ComparableColor("#A18880")),
            new Tuple<string, double, double, double, double, ComparableColor>("Great White Shark", 771, 4.9, 70, 10, new ComparableColor("#a79d9d")),
            new Tuple<string, double, double, double, double, ComparableColor>("Giant Tortoise", 417, 1.3, 255, 10, new ComparableColor("#438D80")),
            new Tuple<string, double, double, double, double, ComparableColor>("Ostrich", 136, 2.8, 70, 90.52, new ComparableColor("#C9BAA4")),
            new Tuple<string, double, double, double, double, ComparableColor>("Red Panda", 7.7, 0.6, 23, 100, new ComparableColor("#96512a")),
            new Tuple<string, double, double, double, double, ComparableColor>("Zebra", 430.4, 2.3, 30, 67, new ComparableColor("#2D2F33")),
            new Tuple<string, double, double, double, double, ComparableColor>("Ardvark", 82, 1.5, 29.8, 58, new ComparableColor("#d7c1b3")),
            new Tuple<string, double, double, double, double, ComparableColor>("Narwhal", 1600, 5.5, 125, 60, new ComparableColor("#515E58")),
        ];

        internal static List<string> _nameList = _animals.Select(a => a.Item1).ToList();
        internal static List<double> _kgList = _animals.Select(a => a.Item2).ToList();
        internal static List<double> _lengthList = _animals.Select(a => a.Item3).ToList();
        internal static List<double> _ageList = _animals.Select(a => a.Item4).ToList();
        internal static List<double> _heartList = _animals.Select(a => a.Item5).ToList();
        internal static List<ComparableColor> _colorList = _animals.Select(a => a.Item6).ToList();

        [TestMethod]
        public void CalculateSpearmanTest()
        {
            var cor_nameKg = Correlation.CalculateSpearman(_nameList, _kgList);
            Assert.IsTrue(Math.Abs(cor_nameKg) < 0.15, "Name and KG should not correlate");
            var cor_nameMeters = Correlation.CalculateSpearman(_nameList, _lengthList);
            Assert.IsTrue(Math.Abs(cor_nameMeters) < 0.15, "Name and length should not correlate");
            var cor_nameAge = Correlation.CalculateSpearman(_nameList, _ageList);
            Assert.IsTrue(Math.Abs(cor_nameAge) < 0.3, $"Name and max age should not correlate ({cor_nameAge})");
            var cor_nameHeart = Correlation.CalculateSpearman(_nameList, _heartList);
            Assert.IsTrue(Math.Abs(cor_nameHeart) < 0.33, $"Name and heart rate should not correlate ({cor_nameHeart})");
            var cor_nameColor = Correlation.CalculateSpearman(_nameList, _colorList);
            Assert.IsTrue(Math.Abs(cor_nameColor) < 0.15, "Name and Color should not correlate");

            var cor_KgMeters = Correlation.CalculateSpearman(_kgList, _lengthList);
            Assert.IsTrue(cor_KgMeters > 0.85, $"Weight and Length should correlate ({cor_KgMeters})");
            var cor_KgAge = Correlation.CalculateSpearman(_kgList, _ageList);
            Assert.IsTrue(cor_KgAge > 0.666);
            var cor_KgHeart = Correlation.CalculateSpearman(_kgList, _heartList);
            Assert.IsTrue(cor_KgHeart < 0.5);
            var cor_KgColor = Correlation.CalculateSpearman(_kgList, _colorList);
            Assert.IsTrue(Math.Abs(cor_KgColor) < 0.33, $"Weight and Color should not correlate ({cor_KgColor})");

            var cor_MetersAge = Correlation.CalculateSpearman(_lengthList, _ageList);
            Assert.IsTrue(cor_MetersAge > 0.6, $"Length and age should correlate ({cor_MetersAge})");
            var cor_MetersHeart = Correlation.CalculateSpearman(_lengthList, _heartList);
            Assert.IsTrue(cor_MetersHeart < -0.45, $"Length and heart rate should inversely correlate ({cor_MetersHeart})");
            var cor_MetersColor = Correlation.CalculateSpearman(_lengthList, _colorList);
            Assert.IsTrue(Math.Abs(cor_MetersColor) < 0.25, $"Length and Color should not correlate ({cor_MetersColor})");

            var cor_AgeHeart = Correlation.CalculateSpearman(_ageList, _heartList);
            Assert.IsTrue(cor_AgeHeart < -0.5);
            var cor_AgeColor = Correlation.CalculateSpearman(_ageList, _colorList);
            Assert.IsTrue(Math.Abs(cor_AgeColor) < 0.2, $"Max age and Color should not correlate ({cor_AgeColor})");

            var cor_HeartColor = Correlation.CalculateSpearman(_heartList, _colorList);
            Assert.IsTrue(Math.Abs(cor_HeartColor) < 0.15, "heart rate and color should not correlate");
        }

        [TestMethod]
        public void CalculateGraphs()
        {
            var graph = Correlation.CalculateGraphAndTrendLine(_nameList, _ageList, out double slope, out double offset);
            var cor = Correlation.CalculateSpearman(_nameList, _ageList);

            const int WIDTH = 1024;
            const int HEIGHT = 1024;
            using Bitmap b = new(WIDTH, HEIGHT);
            int yellow = (int)Math.Round((1.0 - Math.Abs(cor)) * 255.4);
            Color trendColor = Color.FromArgb(255, yellow, 0);
            for (int x = 0; x < WIDTH; ++x)
            {
                double dx = x / (double)WIDTH;
                double dy = (dx * slope) + offset;

                if (dy < 0)
                    continue;

                int px = (int)Math.Round((WIDTH - 1) * dx);
                int py = (HEIGHT - 1) - (int)Math.Round((HEIGHT - 1) * dy);

                if (px >= 0 && py >= 0 && px < WIDTH && py < HEIGHT)
                    b.SetPixel(px, py, trendColor);
            }

            for (int o = 0; o < 10; ++o)
            {
                b.SetPixel(o, 0, Color.Green);
                b.SetPixel(0, o, Color.Green);
                b.SetPixel(o, HEIGHT - 1, Color.Green);
                b.SetPixel(WIDTH - 1, o, Color.Green);
                b.SetPixel((WIDTH - 1) - o, 0, Color.Green);
                b.SetPixel(0, (HEIGHT - 1) - o, Color.Green);
                b.SetPixel((WIDTH - 1) - o, HEIGHT - 1, Color.Green);
                b.SetPixel(WIDTH - 1, (HEIGHT - 1) - o, Color.Green);
            }

            HashSet<Tuple<int, int>> used = [];
            for (int i = 0; i < graph.Count; ++i)
            {
                double scaledX = graph[i].Item1;
                double scaledY = graph[i].Item2;
                int x = (int)Math.Round((WIDTH - 1) * scaledX);
                int y = (HEIGHT - 1) - (int)Math.Round((HEIGHT - 1) * scaledY);
                if (used.Contains(Tuple.Create(x, y)))
                {
                    for (int o = 0; o < 8; ++o)
                    {
                        int oo = o < 4 ? o : o + 1;
                        int od = (oo + x + (WIDTH * y)) % 9;
                        int dx = x + (od % 3) - 1;
                        int dy = y + (od / 3) - 1;
                        if (dx < 0 || dy < 0)
                            continue;
                        if (dx > (WIDTH - 1) || dy > (HEIGHT - 1))
                            continue;
                        if (b.GetPixel(dx, dy) != Color.White)
                        {
                            b.SetPixel(dx, dy, Color.White);
                            used.Add(new Tuple<int, int>(x, y));
                            break;
                        }
                    }
                }
                else
                {
                    b.SetPixel(x, y, Color.White);
                    used.Add(new Tuple<int, int>(x, y));
                }

                Graphics g = Graphics.FromImage(b);

                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                RectangleF rectf = new(Math.Min(x, WIDTH - 40), Math.Min(y, HEIGHT - 20), 90, 50);
                g.DrawString(_nameList[i], new Font("Tahoma", 8), Brushes.White, rectf);

                g.Flush();
            }

            string filename = $"graph.png";
            b.Save(filename.Trim().Replace(' ', '_'), ImageFormat.Png);
        }
    }
}