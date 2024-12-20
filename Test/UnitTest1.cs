using System.Drawing;
using System.Drawing.Imaging;
using woelib;

namespace Test
{
    [TestClass]
    public class ETATest
    {
        [TestMethod]
        public void SimpleTest()
        {
            const int goal = 150;
            const int totalSeconds = 3;
            AdaptiveETA eta = new(goal);
            eta.Add(0, DateTime.Now);
            DateTime plannedFinish = DateTime.Now + TimeSpan.FromSeconds(totalSeconds);
            double[] values = new double[goal];
            DateTime[] etas = new DateTime[goal];
            for (int i = 1; i < goal; ++i)
            {
                Thread.Sleep(TimeSpan.FromSeconds(totalSeconds / (double) goal));
                (values[i], etas[i], double ratePerSecond) = eta.GetEstimate(DateTime.Now);
                
                eta.Add(i, DateTime.Now);
            }
            Thread.Sleep(TimeSpan.FromSeconds(totalSeconds / (double)goal));
            DateTime finish = DateTime.Now;

            int worstEtaIndex = -1;
            long worstEta = long.MinValue;
            int worstValueIndex = -1;
            double worstValue = double.MinValue;
            for (int i = 7; i<goal; ++i)
            {
                double deltaValue = Math.Abs(i - values[i]);
                if (deltaValue > worstValue) {
                    worstValue = deltaValue;
                    worstValueIndex = i;
                }

                long deltaTime = Math.Abs((finish - etas[i]).Ticks);
                if (deltaTime > worstEta)
                {
                    worstEta = deltaTime;
                    worstEtaIndex = i;
                }
            }

            Assert.IsTrue(worstValue < 0.5, $"Unexpectidly miss estimated value at #{worstValueIndex} of {goal} queries.");
            Assert.IsTrue(TimeSpan.FromTicks(worstEta).TotalSeconds < 0.5, $"Unexpectidly miss estimated eta at #{worstEtaIndex} of {goal} queries.");
        }

        [TestMethod]
        public void GettingFasterTest()
        {
            const double initialSecondsPerImpulse = 60;
            const double finalSecondsPerImpulse = 0.5;
            AdaptiveETA eta = new AdaptiveETA(1000);
            DateTime dateTime = DateTime.Now;
            eta.Add(0, dateTime);
            double rate = double.MinValue;
            DateTime currentEta = DateTime.MaxValue;
            for (int i = 1; i <= 1000; ++i)
            {
                double f = (1000.0 - i) / 1000.0;
                double secondsThisImpulse = (initialSecondsPerImpulse - finalSecondsPerImpulse) * f + finalSecondsPerImpulse;
                dateTime = dateTime.AddSeconds(secondsThisImpulse);
                (double currentAmount, DateTime when, double amountPerSecond) = eta.GetEstimate(dateTime);
                if (amountPerSecond != rate && !double.IsNaN(currentAmount) && !double.IsNaN(amountPerSecond) && i > 7)
                {
                    Assert.IsTrue(amountPerSecond >= rate, $"Our rate should always be faster... why isn't {amountPerSecond} > {rate} ?");
                    rate = amountPerSecond;
                    Assert.IsTrue(currentEta >= when);
                    currentEta = when;
                }

                eta.Add(i, dateTime);
            }
        }
    }

    [TestClass]
    public class StringUtilTest
    {
        [TestMethod]
        public void StrDistTest()
        {
            Assert.IsTrue(StringUtil.StrDist("money", "money") == 0);
            Assert.IsTrue(StringUtil.StrDist("money", "monkey") == 1);
            Assert.IsTrue(StringUtil.StrDist("monkey", "money") == 1);
            Assert.IsTrue(StringUtil.StrDist("money", "Money") == 1);
            Assert.IsTrue(StringUtil.StrDist("Money", "money") == 1);
            Assert.IsTrue(StringUtil.StrDist("money", "") == 5);
            Assert.IsTrue(StringUtil.StrDist("", "money") == 5);
            Assert.IsTrue(StringUtil.StrDist("Saturday", "Sunday") == 3);
            Assert.IsTrue(StringUtil.StrDist("Sunday", "Saturday") == 3);
            Assert.IsTrue(StringUtil.StrDist("kitten", "sitting") == 3);
            Assert.IsTrue(StringUtil.StrDist("sitting", "kitten") == 3);
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    [TestClass]
    public class MathUtilTest
    {
        class IntComparer : IComparer<int>
        {
            public int CompareCount = 0;
            public int Compare(int x, int y)
            {
                CompareCount += 1;

                bool xTwo = (x % 2) == 0;
                bool yTwo = (y % 2) == 0;
                if (xTwo != yTwo)
                    return xTwo ? -1 : 1;

                bool xThree = (x % 3) == 0;
                bool yThree = (y % 3) == 0;
                if (xThree != yThree)
                    return xThree ? -1 : 1;

                bool xFive = (x % 5) == 0;
                bool yFive = (y % 5) == 0;
                if (xFive != yFive)
                    return xFive ? -1 : 1;

                bool xSeven = (x % 7) == 0;
                bool ySeven = (y % 7) == 0;
                if (xSeven != ySeven)
                    return xSeven ? -1 : 1;

                bool xEleven = (x % 11) == 0;
                bool yEleven = (y % 11) == 0;
                if (xEleven != yEleven)
                    return xEleven ? -1 : 1;

                bool xThirteen = (x % 13) == 0;
                bool yThirteen = (y % 13) == 0;
                if (xThirteen != yThirteen)
                    return xThirteen ? -1 : 1;

                return x.CompareTo(y);
            }
        }

        [TestMethod]
        public void NthElementComparerTest()
        {
            Random rnd = new Random();
            int[] intSort = new int[102400];
            int[] intNth = new int[102400];
            for (int i = 0; i < intNth.Length; i++)
            {
                intSort[i] = intNth[i] = rnd.Next();
            }

            IntComparer nthComp = new();
            IntComparer sortComp = new();

            Span<int> nthSpan = intNth.AsSpan();
            MathUtil.NthElement(nthSpan, 1024, nthComp);

            Span<int> intSpan = intSort.AsSpan();
            intSpan.Sort(sortComp);

            int nthTotalCompares = nthComp.CompareCount;
            int sortTotalCompares = sortComp.CompareCount;

            Assert.IsTrue(nthTotalCompares < sortTotalCompares);
        }

        [TestMethod]
        public void NthElementSpeedTest()
        {
            for (int j = 0; j < 5000; j++)
            {
                Random rnd = new Random();
                int[] intNth = new int[10240];
                for (int i = 0; i < intNth.Length; i++)
                {
                    intNth[i] = rnd.Next();
                }

                Span<int> nthSpan = intNth.AsSpan();
                MathUtil.NthElement(nthSpan, 1024);

                int highestBeforeN = nthSpan.Slice(0, 1023).ToArray().Max();
                int lowestAfterN = nthSpan.Slice(1025).ToArray().Min();
                Assert.IsTrue(highestBeforeN <= nthSpan[1024]);
                Assert.IsTrue(lowestAfterN >= nthSpan[1024], $"lowestAfterN={lowestAfterN} but nthSpan[1022]={nthSpan[1022]}, nthSpan[1023]={nthSpan[1023]}, nthSpan[1024]={nthSpan[1024]}, nthSpan[1025]={nthSpan[1025]}");
            }
        }

        [TestMethod]
        public void NthElementTest()
        {
            string origin = "bathysphere";
            for (int i = 0; i < 11; ++i)
            {
                Span<char> charList = origin.ToArray().AsSpan();
                MathUtil.NthElement(charList, i);

                if (i > 1)
                {
                    char[] prefix = charList.ToArray().AsSpan(0, i - 1).ToArray();
                    char[] suffix = charList.ToArray().AsSpan(i + 1).ToArray();
                    if (suffix.Length > 0)
                    {
                        Assert.IsTrue(prefix.Max() <= charList[i]);
                        Assert.IsTrue(charList[i] <= suffix.Min());
                    }
                }
            }

            int[] intList = { 6, 1, 2, 3, 4, 5 };
            MathUtil.NthElement(intList.AsSpan(), 2);
            Assert.IsTrue(intList.AsSpan(0, 1).ToArray().Max() <= intList.AsSpan(2).ToArray().Min());
        }

        [TestMethod]
        public void CalculatePhiTest()
        {
            List<bool> listA = [true, true, false, true, true, false, true, true];
            List<bool> listB = [false, false, true, false, false, true, false, false];
            var cor_AA = MathUtil.CalculatePhiCorrelation(listA, listA, out double cor_AA_coverage);
            Assert.IsTrue(cor_AA == 1);
            Assert.IsTrue(cor_AA_coverage == 0);
            var cor_AB = MathUtil.CalculatePhiCorrelation(listA, listB, out double cor_AB_coverage);
            Assert.IsTrue(cor_AB == -1);
            Assert.IsTrue(cor_AB_coverage == 0);
            List<bool> listC = [true, true, true, false, true, false, true, true];
            var cor_AC = MathUtil.CalculatePhiCorrelation(listA, listC, out double cor_AC_coverage);
            Assert.IsTrue(cor_AC > 0.1);
            Assert.IsTrue(cor_AC_coverage > 0.1);
            List<bool> listD = [false, false, true, false, false, true, false, true];
            var cor_AD = MathUtil.CalculatePhiCorrelation(listA, listD, out double cor_AD_coverage);
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

            var cor_AA = MathUtil.CalculatePearsonCorrelation(listA, listA);
            Assert.IsTrue(cor_AA == 1.0);
            var cor_AB = MathUtil.CalculatePearsonCorrelation(listA, listB);
            Assert.IsTrue(cor_AB == -1.0);
            var cor_AC = MathUtil.CalculatePearsonCorrelation(listA, listC);
            Assert.IsTrue(cor_AC > 0.9);
            var cor_AD = MathUtil.CalculatePearsonCorrelation(listA, listD);
            Assert.IsTrue(cor_AD < -0.85);
            var cor_CD = MathUtil.CalculatePearsonCorrelation(listC, listD);
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
            var cor_nameKg = MathUtil.CalculateSpearmanCorrelation(_nameList, _kgList);
            Assert.IsTrue(Math.Abs(cor_nameKg) < 0.15, "Name and KG should not correlate");
            var cor_nameMeters = MathUtil.CalculateSpearmanCorrelation(_nameList, _lengthList);
            Assert.IsTrue(Math.Abs(cor_nameMeters) < 0.15, "Name and length should not correlate");
            var cor_nameAge = MathUtil.CalculateSpearmanCorrelation(_nameList, _ageList);
            Assert.IsTrue(Math.Abs(cor_nameAge) < 0.3, $"Name and max age should not correlate ({cor_nameAge})");
            var cor_nameHeart = MathUtil.CalculateSpearmanCorrelation(_nameList, _heartList);
            Assert.IsTrue(Math.Abs(cor_nameHeart) < 0.33, $"Name and heart rate should not correlate ({cor_nameHeart})");
            var cor_nameColor = MathUtil.CalculateSpearmanCorrelation(_nameList, _colorList);
            Assert.IsTrue(Math.Abs(cor_nameColor) < 0.15, "Name and Color should not correlate");

            var cor_KgMeters = MathUtil.CalculateSpearmanCorrelation(_kgList, _lengthList);
            Assert.IsTrue(cor_KgMeters > 0.85, $"Weight and Length should correlate ({cor_KgMeters})");
            var cor_KgAge = MathUtil.CalculateSpearmanCorrelation(_kgList, _ageList);
            Assert.IsTrue(cor_KgAge > 0.666);
            var cor_KgHeart = MathUtil.CalculateSpearmanCorrelation(_kgList, _heartList);
            Assert.IsTrue(cor_KgHeart < 0.5);
            var cor_KgColor = MathUtil.CalculateSpearmanCorrelation(_kgList, _colorList);
            Assert.IsTrue(Math.Abs(cor_KgColor) < 0.33, $"Weight and Color should not correlate ({cor_KgColor})");

            var cor_MetersAge = MathUtil.CalculateSpearmanCorrelation(_lengthList, _ageList);
            Assert.IsTrue(cor_MetersAge > 0.6, $"Length and age should correlate ({cor_MetersAge})");
            var cor_MetersHeart = MathUtil.CalculateSpearmanCorrelation(_lengthList, _heartList);
            Assert.IsTrue(cor_MetersHeart < -0.45, $"Length and heart rate should inversely correlate ({cor_MetersHeart})");
            var cor_MetersColor = MathUtil.CalculateSpearmanCorrelation(_lengthList, _colorList);
            Assert.IsTrue(Math.Abs(cor_MetersColor) < 0.25, $"Length and Color should not correlate ({cor_MetersColor})");

            var cor_AgeHeart = MathUtil.CalculateSpearmanCorrelation(_ageList, _heartList);
            Assert.IsTrue(cor_AgeHeart < -0.5);
            var cor_AgeColor = MathUtil.CalculateSpearmanCorrelation(_ageList, _colorList);
            Assert.IsTrue(Math.Abs(cor_AgeColor) < 0.2, $"Max age and Color should not correlate ({cor_AgeColor})");

            var cor_HeartColor = MathUtil.CalculateSpearmanCorrelation(_heartList, _colorList);
            Assert.IsTrue(Math.Abs(cor_HeartColor) < 0.15, "heart rate and color should not correlate");
        }

        [TestMethod]
        public void CalculateGraphs()
        {
            var graph = MathUtil.CalculateGraphAndTrendLine(_nameList, _ageList, out double slope, out double offset);
            var cor = MathUtil.CalculateSpearmanCorrelation(_nameList, _ageList);

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

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    [TestClass]
    public class DependancyEngineTest
    {
        enum TestOneDependancies
        {
            GetSeedMoney,
            ConcreteGathered,
            SteelGathered,
            FuelGathered,
            AccountantHired,
            SupervisorHired,
            ScientistsHired,
            PowerPlantBuilt,
            PowerWiredUp,
            PowerPlantStarted,
            MegaLaserInvented,
            MegaLaserBuilt,
            MegaLaserFired,
        }

        class MadScientistLair : IDependancyEngineDataHandler
        {
            public bool IsFinished { get; private set; } = false;
            private readonly Random _random = new();

            internal void CanHookUpPower(DependancyEngine<TestOneDependancies> dependancyEngine, int actionId)
            {
                Thread thread = new Thread(new ThreadStart(() =>
                {
                    Console.WriteLine("Can wire up base");
                    Thread.Sleep(_random.Next(100, 500));
                    Console.WriteLine("Power wired up");
                    dependancyEngine.Resolve(TestOneDependancies.PowerWiredUp);
                    dependancyEngine.MarkCompleted(actionId);
                }));
                thread.Start();
            }

            internal void CanStartPowerPlant(DependancyEngine<TestOneDependancies> dependancyEngine, int actionId)
            {
                Console.WriteLine("Power switch thrown");
                dependancyEngine.Resolve(TestOneDependancies.PowerPlantStarted);
                dependancyEngine.MarkCompleted(actionId);
            }

            internal void CanBuildPowerPlant(DependancyEngine<TestOneDependancies> dependancyEngine, int actionId)
            {
                Thread thread = new Thread(new ThreadStart(() =>
                {
                    Console.WriteLine("Can build power plant");
                    Thread.Sleep(_random.Next(1000, 1500));
                    Console.WriteLine("Power plant built");
                    dependancyEngine.Resolve(TestOneDependancies.PowerPlantBuilt);
                    dependancyEngine.MarkCompleted(actionId);
                }));
                thread.Start();
            }

            internal void CanGatherConcrete(DependancyEngine<TestOneDependancies> dependancyEngine, int actionId)
            {
                Thread thread = new Thread(new ThreadStart(() =>
                {
                    Console.WriteLine("Can gather concrete");
                    Thread.Sleep(_random.Next(0, 1000));
                    Console.WriteLine("Concrete gathered");
                    dependancyEngine.Resolve(TestOneDependancies.ConcreteGathered);
                    dependancyEngine.MarkCompleted(actionId);
                }));
                thread.Start();
            }

            internal void CanGatherSteel(DependancyEngine<TestOneDependancies> dependancyEngine, int actionId)
            {
                Thread thread = new Thread(new ThreadStart(() =>
                {
                    Console.WriteLine("Can gather steel");
                    Thread.Sleep(_random.Next(0, 1000));
                    Console.WriteLine("Steel gathered");
                    dependancyEngine.Resolve(TestOneDependancies.SteelGathered);
                    dependancyEngine.MarkCompleted(actionId);
                }));
                thread.Start();
            }

            internal void CanGatherFuel(DependancyEngine<TestOneDependancies> dependancyEngine, int actionId)
            {
                Thread thread = new Thread(new ThreadStart(() =>
                {
                    Console.WriteLine("Can gather fuel");
                    Thread.Sleep(_random.Next(0, 1000));
                    Console.WriteLine("Fuel gathered");
                    dependancyEngine.Resolve(TestOneDependancies.FuelGathered);
                    dependancyEngine.MarkCompleted(actionId);
                }));
                thread.Start();
            }

            internal void CanHireAccountant(DependancyEngine<TestOneDependancies> dependancyEngine, int actionId)
            {
                Console.WriteLine("Accountant hired");
                dependancyEngine.Resolve(TestOneDependancies.AccountantHired);
                dependancyEngine.MarkCompleted(actionId);
            }

            internal void CanHireSupervisor(DependancyEngine<TestOneDependancies> dependancyEngine, int actionId)
            {
                Console.WriteLine("Supervisor hired");
                dependancyEngine.Resolve(TestOneDependancies.SupervisorHired);
                dependancyEngine.MarkCompleted(actionId);
            }

            internal void CanHireScientists(DependancyEngine<TestOneDependancies> dependancyEngine, int actionId)
            {
                Console.WriteLine("Scientist hired");
                dependancyEngine.Resolve(TestOneDependancies.ScientistsHired);
                dependancyEngine.MarkCompleted(actionId);
            }

            internal void CanInventMegaLaser(DependancyEngine<TestOneDependancies> dependancyEngine, int actionId)
            {
                Thread thread = new Thread(new ThreadStart(() =>
                {
                    Console.WriteLine("Can invent Mega Laser");
                    Thread.Sleep(_random.Next(10, 1100));
                    Console.WriteLine("Mega Laser Invented");
                    dependancyEngine.Resolve(TestOneDependancies.MegaLaserInvented);
                    dependancyEngine.MarkCompleted(actionId);
                }));
                thread.Start();
            }

            internal void CanBuildMegaLaser(DependancyEngine<TestOneDependancies> dependancyEngine, int actionId)
            {
                Console.WriteLine("Mega Laser Built");
                dependancyEngine.Resolve(TestOneDependancies.MegaLaserBuilt);
                dependancyEngine.MarkCompleted(actionId);
            }

            internal void CanFireMegaLaser(DependancyEngine<TestOneDependancies> dependancyEngine, int actionId)
            {
                Console.WriteLine("Mega Laser Fired");
                dependancyEngine.Resolve(TestOneDependancies.MegaLaserFired);
                dependancyEngine.MarkCompleted(actionId);
            }

            internal void OnFinished(DependancyEngine<TestOneDependancies> dependancyEngine)
            {
                Console.WriteLine("MUHAHAHAH!!!");
                IsFinished = true;
            }
        }

        [TestMethod]
        public void TestOne()
        {
            try
            {
                MadScientistLair dataHandler = new MadScientistLair();
                DependancyEngine<TestOneDependancies> de = new(dataHandler);
                de.Add(dataHandler.CanBuildPowerPlant, TestOneDependancies.SupervisorHired, TestOneDependancies.ConcreteGathered, TestOneDependancies.SteelGathered);
                de.Add(dataHandler.CanHookUpPower, TestOneDependancies.SupervisorHired, TestOneDependancies.PowerPlantBuilt);
                de.Add(dataHandler.CanStartPowerPlant, TestOneDependancies.PowerWiredUp, TestOneDependancies.FuelGathered).Name = "Start Up Power Plant";
                de.Add(dataHandler.CanGatherConcrete, TestOneDependancies.AccountantHired).Name = "Gather Concrete";
                de.Add(dataHandler.CanGatherSteel, TestOneDependancies.AccountantHired).Name = "Gather Steel";
                de.Add(dataHandler.CanGatherFuel, TestOneDependancies.AccountantHired).Name = "Gather Fuel";
                de.Add(dataHandler.CanHireAccountant, TestOneDependancies.GetSeedMoney).Name = "Hire Accountant";
                de.Add(dataHandler.CanHireSupervisor, TestOneDependancies.GetSeedMoney).Name = "Hire Supervisor";
                de.Add(dataHandler.CanHireScientists, TestOneDependancies.GetSeedMoney).Name = "Hire Scientists";
                de.Add(dataHandler.CanInventMegaLaser, TestOneDependancies.ScientistsHired).Name = "Invent Mega Laser";
                de.Add(dataHandler.CanBuildMegaLaser, TestOneDependancies.SupervisorHired, TestOneDependancies.ConcreteGathered, TestOneDependancies.SteelGathered, TestOneDependancies.MegaLaserInvented).Name = "Build Mega Laser";
                de.Add(dataHandler.CanFireMegaLaser, TestOneDependancies.PowerPlantStarted, TestOneDependancies.MegaLaserBuilt).Name = "Fire Laser";
                de.OnFinished += dataHandler.OnFinished;

                de.Resolve(TestOneDependancies.GetSeedMoney);
                de.Start();

                Console.WriteLine("\nStart State:");
                Console.WriteLine(de.GetState());

                DateTime start = DateTime.Now;
                while (dataHandler.IsFinished == false && (DateTime.Now - start).TotalSeconds < 1)
                    ;

                Console.WriteLine("\nMid State:");
                Console.WriteLine(de.GetState());

                while (dataHandler.IsFinished == false && (DateTime.Now - start).TotalSeconds < 5)
                    ;

                Console.WriteLine("\nEnd State:");
                Console.WriteLine(de.GetState());

                Assert.IsFalse(de.IsRunning);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        enum UserInfoDeps
        {
            STARTED,
            AdminPermissionLevel,
            TroubleTicketId,
            UserEmailAddr,
            UserId,
        }

        class UserInfoCache : IDependancyEngineDataHandler
        {
            private bool DoesAdminHaveSupervisorPermissions() { return false; }
            private long GetUserIdFromEmailAddress(string userEmail) { return userEmail.GetHashCode(); }
            private long GetUserIdFromTroubleTicketId(long troubleTicketId) { return long.MaxValue - troubleTicketId; }
            private string GetAddressFromUserId(long userId) { return $"{Math.Abs(userId / 100 + 13) % 100} West Ave, Somewhere, CA"; }
            private string GetEmailFromUserId(long userId) { return $"user{userId % 100}@gmail.com"; }

            public bool? Supervisor { get; set; } = false;
            public long? TroubleTicketId { get; set; } = null;
            public long? UserId { get; set; } = null;
            public string UserEmail { get; set; } = string.Empty;
            public string UserAddress { get; set; } = string.Empty;

            internal void FetchAdminPermissionLevel(DependancyEngine<UserInfoDeps> dependancyEngine, int actionId)
            {
                if (Supervisor == null)
                {
                    Supervisor = DoesAdminHaveSupervisorPermissions();
                }

                dependancyEngine.Resolve(UserInfoDeps.AdminPermissionLevel);
                dependancyEngine.MarkCompleted(actionId);
            }

            internal void FetchUserId(DependancyEngine<UserInfoDeps> dependancyEngine, int actionId)
            {
                if (UserId == null)
                {
                    if (TroubleTicketId != null)
                    {
                        if (TroubleTicketId != null)
                        {
                            // Query the DB's trouble ticket table for the user id associated with this trouble ticket
                            UserId = GetUserIdFromTroubleTicketId((long)TroubleTicketId);
                            dependancyEngine.Resolve(UserInfoDeps.UserId);
                        }
                        else
                        {
                            throw new Exception($"Can not get email with null User ID");
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(UserEmail))
                    {
                        // Query the UserId via the Email table
                        UserId = GetUserIdFromEmailAddress(UserEmail);
                        dependancyEngine.Resolve(UserInfoDeps.UserId);
                    }
                    else
                    {
                        throw new Exception("Both trouble ticket Id and user email are unknown!");
                    }
                }

                dependancyEngine.MarkCompleted(actionId);
            }

            internal void FetchUserEmail(DependancyEngine<UserInfoDeps> dependancyEngine, int actionId)
            {
                if (string.IsNullOrWhiteSpace(UserEmail))
                {
                    if (Supervisor == true)
                    {
                        if (UserId != null)
                        {
                            UserEmail = GetEmailFromUserId((long)UserId);
                            dependancyEngine.Resolve(UserInfoDeps.UserEmailAddr);
                        }
                        else
                        {
                            throw new Exception($"Can not get email with null User ID");
                        }
                    }
                    else
                    {
                        UserEmail = "(redacted)";
                    }
                }

                dependancyEngine.MarkCompleted(actionId);
            }

            internal void FetchUserAddress(DependancyEngine<UserInfoDeps> dependancyEngine, int actionId)
            {
                if (string.IsNullOrWhiteSpace(UserAddress))
                {
                    if (Supervisor == true)
                    {
                        if (UserId != null)
                        {
                            UserAddress = GetAddressFromUserId((long)UserId);
                        }
                        else
                        {
                            throw new Exception($"Can not get email with null User ID");
                        }
                    }
                    else
                    {
                        UserAddress = "(redacted)";
                    }
                }

                dependancyEngine.MarkCompleted(actionId);
            }

            internal void FetchCompleted(DependancyEngine<UserInfoDeps> dependancyEngine)
            {
                //DumpInfo();
            }

            internal void DumpInfo()
            {
                Console.WriteLine($"Id={UserId} Email={UserEmail} Address={UserAddress} TroubleTicketId={TroubleTicketId}");
            }
        }

        [TestMethod]
        public void TestTwo()
        {
            foreach (bool? supe in new List<bool?>([true, false, null]))
            {
                foreach (long? troubleTicketId in new List<long?>([591, null]))
                {
                    foreach (string? email in new List<string?>([null, "woe@full.city"]))
                    {
                        DependancyEngine<UserInfoDeps> infoDepEngine = GenerateUserInfoDependancyEngine();
                        if (infoDepEngine.DataHandler is UserInfoCache uic)
                        {
                            if (supe != null)
                            {
                                uic.Supervisor = supe;
                                infoDepEngine.Resolve(UserInfoDeps.AdminPermissionLevel);
                            }

                            if (troubleTicketId != null)
                            {
                                uic.TroubleTicketId = troubleTicketId;
                                infoDepEngine.Resolve(UserInfoDeps.TroubleTicketId);
                            }

                            if (email != null)
                            {
                                uic.UserEmail = email;
                                infoDepEngine.Resolve(UserInfoDeps.UserEmailAddr);
                            }

                            infoDepEngine.Resolve(UserInfoDeps.STARTED);
                            infoDepEngine.Start();
                            Assert.IsFalse(infoDepEngine.IsRunning);
                            if (supe != true)
                            {
                                Assert.IsFalse(uic.UserAddress.Contains("West"));
                                if (string.IsNullOrWhiteSpace(email))
                                {
                                    Assert.IsFalse(uic.UserEmail.Contains("@"));
                                }
                            }

                            if (email != null || troubleTicketId != null)
                            {
                                Assert.IsTrue(uic.UserId != null);
                            }
                        }
                    }
                }
            }
        }

        private static DependancyEngine<UserInfoDeps> GenerateUserInfoDependancyEngine()
        {
            // #TODO: I don't like how we can double the amount of work if we have both email and trouble ticket we'll 
            // be fetching the User ID in two different ways. We should have a simple syntax to reduce it to just one
            // method or the other if both are present.
            UserInfoCache userInfoCache = new UserInfoCache();
            DependancyEngine<UserInfoDeps> infoDepEngine = new(userInfoCache);
            infoDepEngine.Add(userInfoCache.FetchAdminPermissionLevel, UserInfoDeps.STARTED);
            infoDepEngine.Add(userInfoCache.FetchUserId, UserInfoDeps.UserEmailAddr).Name = "Fetch User Id";
            infoDepEngine.Add(userInfoCache.FetchUserId, UserInfoDeps.TroubleTicketId).Name = "Fetch User Id";
            infoDepEngine.Add(userInfoCache.FetchUserEmail, UserInfoDeps.UserId, UserInfoDeps.AdminPermissionLevel).Name = "Fetch Email";
            infoDepEngine.Add(userInfoCache.FetchUserAddress, UserInfoDeps.UserId, UserInfoDeps.AdminPermissionLevel).Name = "Fetch Addr";
            infoDepEngine.OnFinished += userInfoCache.FetchCompleted;
            return infoDepEngine;
        }
    }
}