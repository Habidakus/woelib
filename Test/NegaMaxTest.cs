
using woelib.NegaMax;

namespace Test
{
    [TestClass]
    public class NegaMaxTest
    {

        [TestMethod]
        public void TestInnerWorkings()
        {
            NMNode johnWin = new(1);
            Assert.IsTrue(johnWin.IsTerminal);
            Assert.IsTrue(johnWin.Value == 1.0);
            JohnScore johnScore = new(johnWin);
            Assert.IsTrue(NMScore.GreaterThan(johnScore, johnScore.Reversed));

            NMNode donWin = new(-1);
            Assert.IsTrue(donWin.IsTerminal);
            Assert.IsTrue(donWin.Value == -1.0);
            DonScore donScore = new(donWin);
            Assert.IsTrue(NMScore.GreaterThan(donScore, donScore.Reversed));

            JohnsGameState johnBoard = new(Turn.John, johnWin);
            Assert.IsTrue(johnBoard.Won());
            DonsGameState donsBoard = new(Turn.John, johnWin);
            Assert.IsFalse(donsBoard.Won());

            johnBoard = new(Turn.Don, donWin);
            Assert.IsFalse(johnBoard.Won());
            donsBoard = new(Turn.Don, donWin);
            Assert.IsTrue(donsBoard.Won());
        }

        [TestMethod]
        public void TestSmall()
        {
            const int advance = 1;
            const int moves = 9;

            for (int johnMoves = 1; johnMoves < moves; johnMoves += advance)
            {
                for (int donMoves = 1; donMoves < moves; donMoves += advance)
                {
                    int win = 0;
                    int lost = 0;
                    for (int seed = 0; seed < 5000; ++seed)
                    {
                        int status = RunCalculations(johnMoves, donMoves, 10 + seed);
                        if (status > 0)
                            win += status;
                        else if (status < 0)
                            lost -= status;
                        else
                        {
                            win += 1;
                            lost += 1;
                        }
                    }

                    Console.WriteLine($"John:{johnMoves} Don:{donMoves}  wins:{win} losses:{lost}");
                }

                Console.WriteLine("-");
            }
        }
        
        //[TestMethod]
        //public void TestBothSides()
        //{
        //    const int moves = 9;
        //    for (int johnMoves = 1; johnMoves < moves; johnMoves += 2)
        //    {
        //        for (int donMoves = johnMoves; donMoves < moves; donMoves += 2)
        //        {
        //            int johnScore = SumCalculations(johnMoves, donMoves);
        //            int donScore = SumCalculations(donMoves, johnMoves);
        //            Console.WriteLine($"J{johnMoves} vs {donMoves}  = {johnScore + donScore}");
        //        }

        //        Console.WriteLine("-");
        //    }
        //}

        //[TestMethod]
        //public void TestSimple()
        //{
        //    RunCalculations(johnMoves: 7, donMoves: 7);
        //}

        public class NMAction : INMAction
        {
            private static NMAction? _left;
            private static NMAction? _right;
            internal static NMAction Left
            {
                get
                {
                    if (_left == null)
                        _left = new NMAction();
                    return _left;
                }
            }
            internal static NMAction Right
            {
                get
                {
                    if (_right == null)
                        _right = new NMAction();
                    return _right;
                }
            }
        }

        public class JohnScore : NMScore
        {
            internal double Value { get; }

            private JohnScore(double value)
            {
                Value = value;
            }
            public JohnScore(NMNode node)
            {
                Value = node.Value;
            }

            public override string ToString()
            {
                if (Value == 1)
                    return "John Won";
                else if (Value == -1)
                    return "John Lost";
                else
                    return $"{Value}";
            }

            protected override NMScore CreateReverse()
            {
                return new JohnScore(0.0 - Value);
            }

            protected override bool IsGreaterThan(NMScore other)
            {
                if (other is JohnScore nscore)
                {
                    return Value > nscore.Value;
                }
                else
                {
                    throw new Exception($"Bad IsGreaterThan({other}) argument");
                }
            }
        }

        public class DonScore : NMScore
        {
            internal double Value { get; }

            public DonScore(NMNode node)
            {
                Value = node.Value;
            }

            private DonScore(double value)
            {
                Value = value;
            }

            public override string ToString()
            {
                if (Value == -1)
                    return "Don Won";
                else if (Value == 1)
                    return "Don Lost";
                else
                    return $"{Value}";
            }

            protected override NMScore CreateReverse()
            {
                return new DonScore(0.0 - Value);
            }

            protected override bool IsGreaterThan(NMScore other)
            {
                if (other is DonScore nscore)
                {
                    return Value < nscore.Value;
                }
                else
                {
                    throw new Exception($"Bad IsGreaterThan({other}) argument");
                }
            }
        }

        public class NMNode
        {
            public float Value { get; }
            public bool IsTerminal { get { return Value == -1 || Value == 1; } }
            private NMNode? _left { get; }
            public NMNode Left { get { Assert.IsNotNull(_left); return _left; } }
            private NMNode? _right { get; }
            public NMNode Right { get { Assert.IsNotNull(_right); return _right; } }
            public bool HasChildren { get { return _left != null; } }

            public NMNode GetChild(INMAction action)
            {
                if (action == NMAction.Left)
                {
                    Assert.IsNotNull(_left); return _left;
                }
                else
                {
                    Assert.IsNotNull(_right); return _right;
                }
            }

            private NMNode(Random rnd)
            {
                _left = null;
                _right = null;
                Value = (rnd.NextDouble() > 0.5) ? 1 : -1;
            }

            internal NMNode(float value)
            {
                _left = null;
                _right = null;
                Value = value;
            }

            private NMNode(Random rnd, NMNode left, NMNode right)
            {
                _left = left;
                _right = right;
                float range = left.Value - right.Value;
                Value = right.Value + (float)(rnd.NextDouble() * range);
            }

            public static NMNode CreateTree(Random rnd, int depth)
            {
                if (depth == 0)
                {
                    return new NMNode(rnd);
                }
                else
                {
                    NMNode right = CreateTree(rnd, depth - 1);
                    NMNode left = CreateTree(rnd, depth - 1);
                    if (right.IsTerminal && left.IsTerminal && right.Value == left.Value)
                    {
                        return right;
                    }
                    else
                    {
                        return new NMNode(rnd, left, right);
                    }
                }
            }
        }

        public class JohnsGameState : INMCurrentBoard
        {
            public Turn Turn { get; }
            public NMNode Node { get; }

            public override string ToString()
            {
                if (HasMoves)
                {
                    return $"Working Score: {Score}";
                }
                else
                {
                    return $"Final Score: {Score}";
                }
            }

            public bool HasMoves { get { return Node.HasChildren; } }

            public JohnsGameState(Random rnd, int depth)
            {
                Node = NMNode.CreateTree(rnd, depth);
            }

            internal JohnsGameState(Turn turn, NMNode node)
            {
                Turn = turn;
                Node = node;
            }

            internal JohnsGameState(JohnsGameState parent, INMAction action)
            {
                Turn = (parent.Turn == Turn.Don) ? Turn.John : Turn.Don;
                Node = parent.Node.GetChild(action);
            }

            internal JohnsGameState(DonsGameState parent, INMAction action)
            {
                Turn = (parent.Turn == Turn.Don) ? Turn.John : Turn.Don;
                Node = parent.Node.GetChild(action);
            }

            public bool Won()
            {
                Assert.IsTrue(Node.IsTerminal);
                return (Node.Value == 1);
            }

            public NMScore Score { get { return new JohnScore(Node); } }

            public IEnumerable<INMAction> SortedMoves
            {
                get
                {
                    if (HasMoves)
                    {
                        bool johnsTurn = Turn == Turn.John;
                        NMScore leftScore = new JohnScore(Node.Left);
                        NMScore rightScore = new JohnScore(Node.Right);
                        bool leftIsHigher = NMScore.GreaterThan(leftScore, rightScore);
                        if (leftIsHigher == johnsTurn)
                        {
                            yield return NMAction.Left;
                            yield return NMAction.Right;
                        }
                        else
                        {
                            yield return NMAction.Right;
                            yield return NMAction.Left;
                        }
                    }

                    yield break;
                }
            }

            public INMCurrentBoard CreateChild(INMAction action)
            {
                return new JohnsGameState(this, action);
            }

            public INMCurrentBoard CreateOpponentsBoard(INMAction action)
            {
                return new DonsGameState(this, action);
            }
        }

        public class DonsGameState : INMCurrentBoard
        {
            public Turn Turn { get; }
            public NMNode Node { get; }

            public override string ToString()
            {
                if (HasMoves)
                {
                    return $"Working Score: {Score}";
                }
                else
                {
                    return $"Final Score: {Score}";
                }
            }

            public bool HasMoves { get { return Node.HasChildren; } }

            public DonsGameState(Random rnd, int depth)
            {
                Node = NMNode.CreateTree(rnd, depth);
            }

            internal DonsGameState(Turn turn, NMNode node)
            {
                Turn = turn;
                Node = node;
            }

            internal DonsGameState(DonsGameState parent, INMAction action)
            {
                Turn = (parent.Turn == Turn.Don) ? Turn.John : Turn.Don;
                Node = parent.Node.GetChild(action);
            }

            internal DonsGameState(JohnsGameState parent, INMAction action)
            {
                Turn = (parent.Turn == Turn.Don) ? Turn.John : Turn.Don;
                Node = parent.Node.GetChild(action);
            }

            public bool Won()
            {
                Assert.IsTrue(Node.IsTerminal);
                return (Node.Value == -1);
            }

            public NMScore Score { get { return new DonScore(Node); } }

            public IEnumerable<INMAction> SortedMoves 
            {
                get
                {
                    if (HasMoves)
                    {
                        bool johnsTurn = Turn == Turn.John;
                        NMScore leftScore = new DonScore(Node.Left);
                        NMScore rightScore = new DonScore(Node.Right);
                        bool leftIsHigher = NMScore.GreaterThan(leftScore, rightScore);
                        if (leftIsHigher == johnsTurn)
                        {
                            yield return NMAction.Left;
                            yield return NMAction.Right;
                        }
                        else
                        {
                            yield return NMAction.Right;
                            yield return NMAction.Left;
                        }
                    }

                    yield break;
                }
            }

            public INMCurrentBoard CreateChild(INMAction action)
            {
                return new DonsGameState(this, action);
            }

            public INMCurrentBoard CreateOpponentsBoard(INMAction action)
            {
                return new JohnsGameState(this, action);
            }
        }

        private int RunCalculations(int johnMoves, int donMoves, int seed)
        {
            Random rnd = new Random(seed);
            const int boardDepth = 10;
            INMCurrentBoard[] boards = [new JohnsGameState(rnd, boardDepth), new DonsGameState(rnd, boardDepth)];
            int wins = 0;
            foreach (INMCurrentBoard turnBoard in boards)
            {
                INMCurrentBoard board = turnBoard;
                while (board.HasMoves)
                {
                    int moves = 0;
                    if (board is JohnsGameState)
                    {
                        moves = johnMoves;
                    }
                    else if (board is DonsGameState)
                    {
                        moves = donMoves;
                    }
                    
                    (NMScore score, INMAction? action) = NMCalculator.Calculate(board, moves);
                    if (action != null)
                    {
                        if (board is JohnsGameState jgs)
                        {
                            board = jgs.CreateOpponentsBoard(action);
                        }
                        else if (board is DonsGameState dgs)
                        {
                            board = dgs.CreateOpponentsBoard(action);
                        }
                        else
                        {
                            throw new Exception("Bade board type");
                        }
                    }
                    else
                    {
                        Assert.IsNotNull(action);
                        return 0;
                    }
                }

                //Console.WriteLine($"John: {johnMoves}  Don: {donMoves}  Board: {board}");
                if (board is JohnsGameState jgsa)
                {
                    wins += jgsa.Won() ? 1 : -1;
                }
                else if (board is DonsGameState dgsa)
                {
                    wins += dgsa.Won() ? -1 : 1;
                }
                else
                {
                    throw new Exception("Bade board type");
                }
            }

            return wins;
        }
    }
}