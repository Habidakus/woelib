
namespace Test
{
    [TestClass]
    public class NegaMaxTest
    {
        [TestMethod]
        public void TestSmall()
        {
            const int moves = 14;
            for (int johnMoves = 1; johnMoves < moves; johnMoves += 2)
            {
                for (int donMoves = 1; donMoves < moves; donMoves += 2)
                {
                    RunCalculations(johnMoves, donMoves);
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

        public struct NMAction
        {
            public Card Card { get; }
            public NMAction(Card card)
            {
                Card = card;
            }
            public override string ToString()
            {
                return $"{Card}";
            }
        }

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

        public class CurrentBoard
        {
            private List<Card> _primaryHand = new List<Card>();
            private List<Card> _oppositionHand = new List<Card>();
            private List<Card> _playedCards = new List<Card>();
            private int _primaryScore { get; }
            private int _oppositionScore { get; }

            public override string ToString()
            {
                if (HasMoves)
                {
                    return $"Primary Score: {_primaryScore} vs Opposition Score: {_oppositionScore} Played Cards: {string.Join(", ", _playedCards)}";
                }
                else
                {
                    return $"Final Score: {_primaryScore - _oppositionScore}";
                }
            }

            public bool HasMoves
            {
                get
                {
                    return _primaryHand.Any();
                }
            }

            public CurrentBoard()
            {
                for (int value = 2; value < 5; ++value)
                {
                    for (int suit = 0; suit < 4; ++suit)
                    {
                        _primaryHand.Add(new Card(value, suit));
                        _oppositionHand.Add(new Card(value, suit));
                    }
                }

                _primaryScore = 0;
                _oppositionScore = 0;
            }

            private CurrentBoard(List<Card> playedCards, List<Card> primaryHand, int primaryScore, List<Card> oppositionHand, int oppositionScore, Card justPlayedCard)
            {
                _primaryHand = [.. primaryHand];
                _primaryScore = primaryScore;
                _oppositionHand = [.. oppositionHand];
                _oppositionScore = oppositionScore;
                _playedCards = [.. playedCards, justPlayedCard];
                _oppositionScore += CalculateScore.GetMoveScore(_playedCards);
                _oppositionHand.Remove(justPlayedCard);
            }

            public NMScore Score { get { return new CurrentBoardScore(_oppositionScore, _oppositionHand, _primaryScore, _primaryHand); } }
            public int ScoreAsInt { get { return _oppositionScore -  _primaryScore; } }

            public IEnumerable<NMAction> SortedMoves 
            {
                get
                {
                    foreach (Card card in _primaryHand)
                    {
                        yield return new NMAction(card);
                    }

                    yield break;
                }
            }

            internal CurrentBoard CreateChild(NMAction action)
            {
                return new(_playedCards, _oppositionHand, _oppositionScore, _primaryHand, _primaryScore, action.Card);
            }

            private class CurrentBoardScore : NMScore
            {
                private int _oppositionScore;
                private int _oppositionPotential;
                private int _primaryScore;
                private int _primaryPotential;

                public override string ToString()
                {
                    return $"<score={Score} potential={Potential}/>";
                }

                protected int Score { get { return _oppositionScore - _primaryScore; } }
                protected int Potential { get { return _oppositionPotential - _primaryPotential; } }

                public CurrentBoardScore(int oppositionScore, List<Card> oppositionHand, int primaryScore, List<Card> primaryHand)
                {
                    _oppositionScore = oppositionScore;
                    _oppositionPotential = SumValue(oppositionHand);
                    _primaryScore = primaryScore;
                    _primaryPotential = SumValue(primaryHand);
                }

                private static int SumValue(List<Card> hand)
                {
                    return hand.Sum(a => a.Value);
                }

                public CurrentBoardScore(int oppositionScore, int oppositionPotential, int primaryScore, int primaryPotential)
                {
                    _oppositionScore = oppositionScore;
                    _oppositionPotential = oppositionPotential;
                    _primaryScore = primaryScore;
                    _primaryPotential = primaryPotential;
                }

                protected override NMScore CreateReverse()
                {
                    return new CurrentBoardScore(_primaryScore, _primaryPotential, _oppositionScore, _oppositionPotential);
                }

                protected override bool IsGreaterThan(NMScore other)
                {
                    if (other is CurrentBoardScore otherScore)
                    {
                        if (Score != otherScore.Score)
                        {
                            return Score > otherScore.Score;
                        }
                        else
                        {
                            return Potential > otherScore.Potential;
                        }
                    }
                    else
                    {
                        throw new Exception($"NMScore.IsGreaterThan({other}) called with incompatible type");
                    }
                }
            }

            internal bool OnlyPlayedCardIs(int value, int suit)
            {
                if (_playedCards.Count != 1)
                    return false;
                var card = _playedCards[0];
                return card.Value == value && card.Suit == suit;
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

        private void RunCalculations(int johnMoves, int donMoves)
        {
            CurrentBoard board = new();
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
                    return;
                }
            }

            Console.WriteLine($"John: {johnMoves}  Don: {donMoves}  Board: {board}");
        }

        private int SumCalculations(int johnMoves, int donMoves)
        {
            CurrentBoard board = new();
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
                }
            }

            return board.ScoreAsInt;
        }
    }
}