using woelib.NegaMax;

namespace Test
{
    [TestClass]
    public class NegaMaxTest
    {
        public enum Turn { JohnsTurn, DonsTurn };

        [TestMethod]
        public void TestInnerWorkings()
        {
            GameBoardNode johnWin = new(1);
            Assert.IsTrue(johnWin.IsTerminal);
            Assert.IsTrue(johnWin.Score.Value == 1.0);
            Assert.IsTrue(NMScore.GreaterThan(johnWin.Score, johnWin.Score.Reversed));

            GameBoardNode donWin = new(-1);
            Assert.IsTrue(donWin.IsTerminal);
            Assert.IsTrue(donWin.Score.Value == -1.0);
            Assert.IsTrue(NMScore.GreaterThan(donWin.Score.Reversed, donWin.Score));

            CurrentBoard johnBoard = new(Turn.JohnsTurn, johnWin);
            Assert.IsTrue(johnBoard.Won(Turn.JohnsTurn));
            Assert.IsFalse(johnBoard.Won(Turn.DonsTurn));

            CurrentBoard donBoard = new(Turn.DonsTurn, donWin);
            Assert.IsTrue(donBoard.Won(Turn.DonsTurn));
            Assert.IsFalse(donBoard.Won(Turn.JohnsTurn));
        }

        [TestMethod]
        public void TestSmall()
        {
            Dictionary<Tuple<int, int>, int> johnWinTracker = new();
            const int advance = 1;
            const int moves = 6;

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

                    johnWinTracker[new Tuple<int, int>(johnMoves, donMoves)] = win;

                    Console.WriteLine($"John:{johnMoves} Don:{donMoves}  wins:{win} losses:{lost}");

                    if (johnMoves > 2)
                    {
                        int johnLesserWinCount = johnWinTracker[new Tuple<int, int>(johnMoves - 2, donMoves)];
                        Assert.IsTrue(johnLesserWinCount < win, $"Why did john get less wins with {johnMoves} look ahead than he did with {johnMoves - 1} (when don had {donMoves})?!");
                    }

                    if (donMoves > 1)
                    {
                        int donLesserWinCount = johnWinTracker[new Tuple<int, int>(johnMoves, donMoves - 1)];
                        Assert.IsTrue(donLesserWinCount > win, $"Why did john get more moves with {johnMoves} look ahead and don had {donMoves} than when don had {donMoves - 1} look aheads?!");
                    }
                }

                Console.WriteLine("-");
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

        public class Move : INMAction
        {
            internal enum MoveDirection
            {
                Left,
                Right,
            }
            internal MoveDirection Direction { get; }
            private Move(MoveDirection direction)
            {
                Direction = direction;
            }

            public static readonly Move left = new(MoveDirection.Left);
            public static readonly Move right = new(MoveDirection.Right);
        }

        public class GameBoardNode
        {
            public float Value { get; }
            public NodeScore Score { get { return new NodeScore(Value); } }

            public bool IsTerminal { get { return Value == -1 || Value == 1; } }
            private GameBoardNode? _left { get; }
            public GameBoardNode Left { get { Assert.IsNotNull(_left); return _left; } }
            private GameBoardNode? _right { get; }
            public GameBoardNode Right { get { Assert.IsNotNull(_right); return _right; } }
            public bool HasChildren { get { return _left != null; } }

            internal GameBoardNode CreateReverse()
            {
                if (IsTerminal)
                {
                    return new GameBoardNode(0.0f - Value);
                }
                else
                {
                    return new GameBoardNode(0.0f - Value, Left.CreateReverse(), Right.CreateReverse());
                }
            }

            public GameBoardNode GetChild(INMAction action)
            {
                if (action is Move move)
                {
                    if (move.Direction == Move.MoveDirection.Left)
                    {
                        Assert.IsNotNull(_left);
                        return _left;
                    }
                    else
                    {
                        Assert.IsNotNull(_right);
                        return _right;
                    }
                }
                else
                {
                    throw new Exception($"Bad GetChild({action}) argument");
                }
            }

            private GameBoardNode(Random rnd)
            {
                _left = null;
                _right = null;
                Value = (rnd.NextDouble() > 0.5) ? 1 : -1;
            }

            internal GameBoardNode(float value)
            {
                _left = null;
                _right = null;
                Value = value;
            }

            private GameBoardNode(float value, GameBoardNode left, GameBoardNode right)
            {
                _left = left;
                _right = right;
                Value = value;
            }

            private GameBoardNode(Random rnd, GameBoardNode left, GameBoardNode right)
            {
                _left = left;
                _right = right;
                float range = left.Value - right.Value;
                Value = right.Value + (float)(rnd.NextDouble() * range);
            }

            public static GameBoardNode CreateTree(Random rnd, int depth)
            {
                if (depth == 0)
                {
                    return new GameBoardNode(rnd);
                }
                else
                {
                    GameBoardNode right = CreateTree(rnd, depth - 1);
                    GameBoardNode left = CreateTree(rnd, depth - 1);
                    if (right.IsTerminal && left.IsTerminal && right.Value == left.Value)
                    {
                        return right;
                    }
                    else
                    {
                        return new GameBoardNode(rnd, left, right);
                    }
                }
            }
        }

        public class CurrentBoard : INMGameState
        {
            private Turn Turn { get; }
            private GameBoardNode Node { get; }

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

            internal CurrentBoard(Random rnd, int depth)
            {
                Node = GameBoardNode.CreateTree(rnd, depth);
            }

            internal CurrentBoard(Turn turn, GameBoardNode node)
            {
                Turn = turn;
                Node = node;
            }

            private CurrentBoard(CurrentBoard parent, INMAction action)
            {
                Turn = (parent.Turn == Turn.DonsTurn) ? Turn.JohnsTurn : Turn.DonsTurn;
                Node = parent.Node.GetChild(action);
            }

            // Create the inverse of the given board, a game state from the opposing player's point of view
            internal CurrentBoard(CurrentBoard opponentsGameState)
            {
                Turn = opponentsGameState.Turn;
                Node = opponentsGameState.Node.CreateReverse();
            }

            internal bool Won(Turn who)
            {
                Assert.IsTrue(Node.IsTerminal);
                double value = Node.Score.Value;
                return value == ((who == Turn.JohnsTurn) ? 1 : -1);
            }

            public NMScore Score { get { return Node.Score; } }

            public IEnumerable<INMAction> SortedMoves 
            {
                get
                {
                    if (HasMoves)
                    {
                        bool johnsTurn = Turn == Turn.JohnsTurn;
                        NMScore leftScore = Node.Left.Score;
                        NMScore rightScore = Node.Right.Score;
                        bool leftIsHigher = NMScore.GreaterThan(leftScore, rightScore);
                        if (leftIsHigher == johnsTurn)
                        {
                            yield return Move.left;
                            yield return Move.right;
                        }
                        else
                        {
                            yield return Move.right;
                            yield return Move.left;
                        }
                    }

                    yield break;
                }
            }

            public INMGameState CreateChild(INMAction action)
            {
                return new CurrentBoard(this, action);
            }
        }

        private bool RunCalculations(int johnMoves, int donMoves, int seed)
        {
            Random rnd = new Random(seed);
            const int boardDepth = 10;
            CurrentBoard johnGameState = new CurrentBoard(rnd, boardDepth);
            CurrentBoard donGameState = new CurrentBoard(johnGameState);
            bool isJohnsTurn = true;
            while (johnGameState.HasMoves)
            {
                INMAction? action = null;
                if ( isJohnsTurn)
                {
                    action = Calculator.GetBestAction(johnGameState, johnMoves);
                }
                else
                {
                    action = Calculator.GetBestAction(donGameState, donMoves);
                }

                if (action != null)
                {
                    johnGameState = johnGameState.CreateChild(action) as CurrentBoard;
                    donGameState = donGameState.CreateChild(action) as CurrentBoard;
                    isJohnsTurn = !isJohnsTurn;
                }
                else
                {
                    Assert.IsNotNull(action);
                    return false;
                }
            }

            if (johnGameState is CurrentBoard currentBoard)
            {
                return currentBoard.Won(Turn.JohnsTurn);
            }
            else
            {
                throw new Exception("Bad board type");
            }
        }
    }
}