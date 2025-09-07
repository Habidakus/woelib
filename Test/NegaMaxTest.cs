
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
            Assert.IsTrue(johnWin.Score.Value == 1.0);
            Assert.IsTrue(NMScore.GreaterThan(johnWin.Score, johnWin.Score.Reversed));

            NMNode donWin = new(-1);
            Assert.IsTrue(donWin.IsTerminal);
            Assert.IsTrue(donWin.Score.Value == -1.0);
            Assert.IsTrue(NMScore.GreaterThan(donWin.Score.Reversed, donWin.Score));

            CurrentBoard johnBoard = new(Turn.John, johnWin);
            Assert.IsTrue(johnBoard.Won(Turn.John));
            Assert.IsFalse(johnBoard.Won(Turn.Don));

            CurrentBoard donBoard = new(Turn.Don, donWin);
            Assert.IsTrue(donBoard.Won(Turn.Don));
            Assert.IsFalse(donBoard.Won(Turn.John));
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
                        if (RunCalculations(johnMoves, donMoves, 10 + seed))
                        {
                            win += 1;
                        }
                        else
                        {
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

        public enum NMAction { left, right };

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

            internal static bool GreaterThan(NMScore left, NMScore right)
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

            internal static NMScore Max(NMScore alpha, NMScore bestScore)
            {
                if (GreaterThan(alpha, bestScore))
                    return alpha;
                else
                    return bestScore;
            }

            internal static bool GreaterOrEqualTo(NMScore alpha, NMScore beta)
            {
                return GreaterThan(alpha, beta) || !GreaterThan(beta, alpha);
            }
        }

        public class NodeScore : NMScore
        {
            internal double Value { get; }

            internal NodeScore(double value)
            {
                Value = value;
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
                return new NodeScore(0.0 - Value);
            }

            protected override bool IsGreaterThan(NMScore other)
            {
                if (other is NodeScore nscore)
                {
                    return Value > nscore.Value;
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
            public NodeScore Score { get { return new NodeScore(Value); } }

            //internal NodeScore GetScore(Turn turn)
            //{
            //    return new NodeScore(turn == Turn.John ? Value : 0.0 - Value);
            //}

            public bool IsTerminal { get { return Value == -1 || Value == 1; } }
            private NMNode? _left { get; }
            public NMNode Left { get { Assert.IsNotNull(_left); return _left; } }
            private NMNode? _right { get; }
            public NMNode Right { get { Assert.IsNotNull(_right); return _right; } }
            public bool HasChildren { get { return _left != null; } }

            public NMNode GetChild(NMAction action)
            {
                if (action == NMAction.left)
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

        public class CurrentBoard
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

            public CurrentBoard(Random rnd, int depth)
            {
                Node = NMNode.CreateTree(rnd, depth);
            }

            internal CurrentBoard(Turn turn, NMNode node)
            {
                Turn = turn;
                Node = node;
            }

            private CurrentBoard(CurrentBoard parent, NMAction action)
            {
                Turn = (parent.Turn == Turn.Don) ? Turn.John : Turn.Don;
                Node = parent.Node.GetChild(action);
            }

            public bool Won(Turn who)
            {
                Assert.IsTrue(Node.IsTerminal);
                double value = Node.Score.Value;
                return value == ((who == Turn.John) ? 1 : -1);
            }

            public NMScore Score { get { return Node.Score; } }

            public IEnumerable<NMAction> SortedMoves 
            {
                get
                {
                    if (HasMoves)
                    {
                        bool johnsTurn = Turn == Turn.John;
                        NMScore leftScore = Node.Left.Score;
                        NMScore rightScore = Node.Right.Score;
                        bool leftIsHigher = NMScore.GreaterThan(leftScore, rightScore);
                        if (leftIsHigher == johnsTurn)
                        {
                            yield return NMAction.left;
                            yield return NMAction.right;
                        }
                        else
                        {
                            yield return NMAction.right;
                            yield return NMAction.left;
                        }
                    }

                    yield break;
                }
            }

            internal CurrentBoard CreateChild(NMAction action)
            {
                return new(this, action);
            }
        }

        private (NMScore score, NMAction? action) negamax(CurrentBoard board, int depth, NMScore alpha, NMScore beta, int color)
        {
            // if depth = 0 or node is a terminal node then
            //     return color * the heuristic value of node
            if (depth == 0 || !board.HasMoves)
            {
                if (color > 0)
                    return (board.Score, null);
                else
                    return (board.Score.Reversed, null);
            }

            // value := -infinity
            NMScore bestScore = NMScore.MinValue;
            NMAction? bestAction = null;

            bool debugOut = false; // board.OnlyPlayedCardIs(2, 3);

            // childNodes := generateMoves(node)
            // childNodes := orderMoves(childNodes)
            // foreach child in childNodes do
            foreach (NMAction action in board.SortedMoves)
            {
                var child = board.CreateChild(action);

                // value := max(value, -negamax(child, depth - 1, -beta, -alpha, -color))
                var childResult = negamax(child, depth - 1, beta.Reversed, alpha.Reversed, 0 - color);
                NMScore reversedScore = childResult.score.Reversed;
                if (debugOut)
                {
                    if (bestAction != null)
                        Console.WriteLine($"    Considering {action} => {reversedScore} (best: {bestScore},  bestAction: {bestAction},  alpha: {alpha},  beta: {beta})");
                    else
                        Console.WriteLine($"    Considering {action} => {reversedScore} (alpha: {alpha},  beta: {beta})");
                }
                if (bestAction == null || NMScore.GreaterThan(reversedScore, bestScore))
                {
                    bestScore = reversedScore;
                    bestAction = action;
                }

                // alpha := max(alpha, value)
                alpha = NMScore.Max(alpha, bestScore);

                // if alpha >= beta then
                //     break (* cut-off *)
                if (NMScore.GreaterOrEqualTo(alpha, beta))
                {
                    if (debugOut)
                    {
                        Console.WriteLine($"    Pruning remaining moves ({alpha} >= {beta})");
                    }
                    break;
                }
            }

            return (bestScore, bestAction);
        }

        private bool RunCalculations(int johnMoves, int donMoves, int seed)
        {
            Random rnd = new Random(seed);
            const int boardDepth = 10;
            CurrentBoard board = new(rnd, boardDepth);
            int color = 1;
            while (board.HasMoves)
            {
                int depth = color > 0 ? johnMoves : donMoves;
                (NMScore score, NMAction? action) = negamax(board, depth, NMScore.MinValue, NMScore.MaxValue, color);
                if (action != null)
                {
                    string player = color > 0 ? "John" : "Don";
                    board = board.CreateChild(action.Value);
                    //Console.WriteLine($"{player} plays {action} => {board}");
                    color *= -1;
                }
                else
                {
                    Assert.IsNotNull(action);
                    return false;
                }
            }

            //Console.WriteLine($"John: {johnMoves}  Don: {donMoves}  Board: {board}");
            return board.Won(Turn.John);
        }
    }
}