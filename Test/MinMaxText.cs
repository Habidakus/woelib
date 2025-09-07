using woelib.MinMaxCalculator;

namespace Test
{
    public enum Turn { John, Don };

    //public class Card
    //{
    //    public int Value;
    //    public int Suit;

    //    public override string ToString()
    //    {
    //        return $"{Value} of {Suit}";
    //    }

    //    public Card(int value, int suit)
    //    {
    //        Value = value;
    //        Suit = suit;
    //    }

    //    public static Card NoCard => new Card(-1, -1);
    //}

    //public class CalculateScore
    //{
    //    public static int GetMoveScore(List<Card> playedCards)
    //    {
    //        int last = playedCards.Count - 1;
    //        if (last > 2)
    //        {
    //            return GetValue(playedCards[last], playedCards[last - 1], playedCards[last - 2], playedCards[last - 3]);
    //        }
    //        else if (last == 2)
    //        {
    //            return GetValue(playedCards[last], playedCards[last - 1], playedCards[last - 2], Card.NoCard);
    //        }
    //        else if (last == 1)
    //        {
    //            return GetValue(playedCards[last], playedCards[last - 1], Card.NoCard, Card.NoCard);
    //        }
    //        else if (last == 0)
    //        {
    //            return GetValue(playedCards[last], Card.NoCard, Card.NoCard, Card.NoCard);
    //        }
    //        else
    //        {
    //            throw new Exception("Why are no cards in the played pile?");
    //        }
    //    }

    //    private static int GetSuitCound(Card played, Card p1, Card p2, Card p3)
    //    {
    //        HashSet<int> suits = [played.Suit, p1.Suit, p2.Suit, p3.Suit];
    //        suits.Remove(-1);
    //        return suits.Count;
    //    }

    //    private static int GetValue(Card played, Card p1, Card p2, Card p3)
    //    {
    //        int multiple = 1;
    //        int suitCount = GetSuitCound(played, p1, p2, p3);
    //        if (suitCount == 4)
    //        {
    //            multiple = -1;
    //        }
    //        else if (suitCount == 1)
    //        {
    //            multiple = 1;
    //        }
    //        else if (suitCount == 2)
    //        {
    //            multiple = 2;
    //        }

    //        int previousValue = p1.Suit == -1 ? 0 : p1.Value;

    //        return multiple * (played.Value * 2 + previousValue);
    //    }
    //}

    //public class Score : IMMCScore
    //{
    //    private int _john_score { get; }
    //    private int _don_score { get; }

    //    public override string ToString()
    //    {
    //        return $"<Score value={_don_score - _john_score} john={_john_score} don={_don_score}>";
    //    }

    //    public Score(Turn whoJustWent, Score previousScore, List<Card> playedCards)
    //    {
    //        _john_score = previousScore._john_score;
    //        _don_score = previousScore._don_score;

    //        int value = CalculateScore.GetMoveScore(playedCards);
    //        if (whoJustWent == Turn.John)
    //            _john_score += value;
    //        else
    //            _don_score += value;
    //    }

    //    //private static int GetSuitCound(Card played, Card p1, Card p2, Card p3)
    //    //{
    //    //    HashSet<int> suits = [played.Suit, p1.Suit, p2.Suit, p3.Suit];
    //    //    suits.Remove(-1);
    //    //    return suits.Count;
    //    //}

    //    //private static int GetValue(Card played, Card p1, Card p2, Card p3)
    //    //{
    //    //    int multiple = 1;
    //    //    int suitCount = GetSuitCound(played, p1, p2, p3);
    //    //    if (suitCount == 4)
    //    //    {
    //    //        multiple = 4;
    //    //    }
    //    //    else if (suitCount == 1)
    //    //    {
    //    //        multiple = -1;
    //    //    }
    //    //    else if (suitCount == 3)
    //    //    {
    //    //        multiple = 2;
    //    //    }

    //    //    return multiple * played.Value;
    //    //}

    //    public Score(int v)
    //    {
    //        _john_score = v;
    //        _don_score = -v;
    //    }

    //    public bool IsGreaterThan(int player, IMMCScore other)
    //    {
    //        if (other is Score otherScore)
    //        {

    //            if (player == (int)Turn.John)
    //                return _john_score - _don_score > otherScore._john_score - otherScore._don_score;
    //            else if (player == (int)Turn.Don)
    //                return _don_score - _john_score > otherScore._don_score - otherScore._john_score;
    //            else
    //                throw new Exception($"Invalid player={player} in IsGreaterThan({this} > {other})");
    //        }
    //        else
    //        {
    //            throw new InvalidOperationException($"Other score {other} is not of type Score.");
    //        }
    //    }
    //}

    //public class PlayAction : IMMCAction
    //{
    //    public override string ToString()
    //    {
    //        return $"{Player} plays {Card.Value} of {Card.Suit}";
    //    }

    //    public Turn Player { get; }
    //    public Card Card { get; }
    //    private GameState _parentGameState { get; }
    //    private GameState? _ourGameState = null;
    //    private IMMCGameState GameState
    //    {
    //        get
    //        {
    //            if (_ourGameState == null)
    //            {
    //                _ourGameState = _parentGameState.ApplyAction(this) as GameState;
    //            }

    //            return _ourGameState!;
    //        }
    //    }
    //    private IMMCScore? _score = null;
    //    public IMMCScore Score 
    //    {
    //        get 
    //        {
    //            if (_score == null)
    //            {
    //                _score = GameState.Score;
    //            }

    //            return _score;
    //        }
    //    }

    //    internal PlayAction(Turn player, Card card, GameState gameState)
    //    {
    //        Player = player;
    //        Card = card;
    //        _parentGameState = gameState;
    //    }
    //}

    //public class GameState : IMMCGameState
    //{
    //    internal Turn Turn { get; }
    //    private List<Card> _johnHand = new List<Card>();
    //    private List<Card> _donHand = new List<Card>();
    //    private List<Card> _playedCards = new List<Card>();

    //    public override string ToString()
    //    {
    //        return $"{Turn}'s turn to play: {_score}";
    //    }

    //    public int Player { get { return (int)Turn; } }

    //    /// <summary>
    //    /// Returns the current worth of the game state from the point of view of the player who provided
    //    /// the action that created this game state instance. For instance, if in chess and black has
    //    /// just taken the opponent's queen, then this score would be very high as seen from the point of
    //    /// view of the black pieces.
    //    ///</summary>
    //    public IMMCScore Score { get { return _score; } }
    //    private Score _score { get; }

    //    public IMMCAction[] SortedMoves
    //    {
    //        get
    //        {
    //            List<IMMCAction> actions = new List<IMMCAction>();
    //            if (Turn == Turn.John)
    //            {
    //                foreach (Card card in _johnHand)
    //                {
    //                    actions.Add(new PlayAction(Turn.John, card, this));
    //                }
    //            }
    //            else
    //            {
    //                foreach (Card card in _donHand)
    //                {
    //                    actions.Add(new PlayAction(Turn.Don, card, this));
    //                }
    //            }

    //            IMMCAction[] actionArray = actions.ToArray();
    //            Array.Sort(actionArray, (a, b) => IMMCScore.IsLeftGreaterThanRight((int)Turn, a.Score, b.Score) ? -1 : 1);
    //            return actionArray;
    //        }
    //    }

    //    public IMMCGameState ApplyAction(IMMCAction action)
    //    {
    //        return new GameState(this, action);
    //    }

    //    private GameState(GameState gameState, IMMCAction action)
    //    {
    //        _johnHand = new List<Card>(gameState._johnHand);
    //        _donHand = new List<Card>(gameState._donHand);
    //        _playedCards = new List<Card>(gameState._playedCards);
    //        if (action is PlayAction playAction)
    //        {
    //            _playedCards.Add(playAction.Card);
    //            _score = new Score(gameState.Turn, gameState._score, _playedCards);

    //            bool removed = false;
    //            if (playAction.Player == Turn.John)
    //            {
    //                removed = _johnHand.Remove(playAction.Card);
    //                Turn = Turn.Don;
    //            }
    //            else
    //            {
    //                removed = _donHand.Remove(playAction.Card);
    //                Turn = Turn.John;
    //            }

    //            Assert.IsTrue(removed);
    //        }
    //        else
    //        {
    //            throw new Exception("Invalid action type");
    //        }
    //    }

    //    public GameState()
    //    {
    //        for (int value = 2; value < 5; ++value)
    //        {
    //            for (int suit = 0; suit < 4; ++suit)
    //            {
    //                _johnHand.Add(new Card(value, suit));
    //                _donHand.Add(new Card(value, suit));
    //            }
    //        }

    //        _score = new Score(0);
    //        Turn = Turn.John;
    //    }
    //}

    //[TestClass]
    //public class MinMaxText
    //{
    //    [TestMethod]
    //    public void TestSmall()
    //    {
    //        for (int johnMoves = 1; johnMoves < 9; ++johnMoves)
    //        {
    //            for (int donMoves = 1; donMoves < 9; ++donMoves)
    //            {
    //                RunCalculations(johnMoves, donMoves);
    //            }
    //            Console.WriteLine("-");
    //        }
    //    }

    //    // ---------------------------------------------

    //    //[TestMethod]
    //    //public void Test22()
    //    //{
    //    //    RunCalculations(johnMoves: 2, donMoves: 2);
    //    //}

    //    // ---------------------------------------------

    //    //[TestMethod]
    //    //public void Test82()
    //    //{
    //    //    RunCalculations(johnMoves: 8, donMoves: 2);
    //    //}

    //    //[TestMethod]
    //    //public void Test28()
    //    //{
    //    //    RunCalculations(johnMoves: 2, donMoves: 8);
    //    //}

    //    //[TestMethod]
    //    //public void Test83()
    //    //{
    //    //    RunCalculations(johnMoves: 8, donMoves: 3);
    //    //}

    //    //[TestMethod]
    //    //public void Test38()
    //    //{
    //    //    RunCalculations(johnMoves: 3, donMoves: 8);
    //    //}

    //    //[TestMethod]
    //    //public void Test84()
    //    //{
    //    //    RunCalculations(johnMoves: 8, donMoves: 4);
    //    //}

    //    //[TestMethod]
    //    //public void Test48()
    //    //{
    //    //    RunCalculations(johnMoves: 4, donMoves: 8);
    //    //}

    //    //[TestMethod]
    //    //public void Test85()
    //    //{
    //    //    RunCalculations(johnMoves: 8, donMoves: 5);
    //    //}

    //    //[TestMethod]
    //    //public void Test58()
    //    //{
    //    //    RunCalculations(johnMoves: 5, donMoves: 8);
    //    //}

    //    //[TestMethod]
    //    //public void Test86()
    //    //{
    //    //    RunCalculations(johnMoves: 8, donMoves: 6);
    //    //}

    //    //[TestMethod]
    //    //public void Test68()
    //    //{
    //    //    RunCalculations(johnMoves: 6, donMoves: 8);
    //    //}

    //    //[TestMethod]
    //    //public void Test87()
    //    //{
    //    //    RunCalculations(johnMoves: 8, donMoves: 7);
    //    //}

    //    //[TestMethod]
    //    //public void Test78()
    //    //{
    //    //    RunCalculations(johnMoves: 7, donMoves: 8);
    //    //}

    //    //[TestMethod]
    //    //public void Test88()
    //    //{
    //    //    RunCalculations(johnMoves: 8, donMoves: 8);
    //    //}

    //    //// --------------------------

    //    //[TestMethod]
    //    //public void Test92()
    //    //{
    //    //    RunCalculations(johnMoves: 9, donMoves: 2);
    //    //}

    //    //[TestMethod]
    //    //public void Test29()
    //    //{
    //    //    RunCalculations(johnMoves: 2, donMoves: 9);
    //    //}

    //    //[TestMethod]
    //    //public void Test93()
    //    //{
    //    //    RunCalculations(johnMoves: 9, donMoves: 3);
    //    //}

    //    //[TestMethod]
    //    //public void Test39()
    //    //{
    //    //    RunCalculations(johnMoves: 3, donMoves: 9);
    //    //}

    //    //[TestMethod]
    //    //public void Test94()
    //    //{
    //    //    RunCalculations(johnMoves: 9, donMoves: 4);
    //    //}

    //    //[TestMethod]
    //    //public void Test49()
    //    //{
    //    //    RunCalculations(johnMoves: 4, donMoves: 9);
    //    //}

    //    //[TestMethod]
    //    //public void Test95()
    //    //{
    //    //    RunCalculations(johnMoves: 9, donMoves: 5);
    //    //}

    //    //[TestMethod]
    //    //public void Test59()
    //    //{
    //    //    RunCalculations(johnMoves: 5, donMoves: 9);
    //    //}

    //    //[TestMethod]
    //    //public void Test96()
    //    //{
    //    //    RunCalculations(johnMoves: 9, donMoves: 6);
    //    //}

    //    //[TestMethod]
    //    //public void Test69()
    //    //{
    //    //    RunCalculations(johnMoves: 6, donMoves: 9);
    //    //}

    //    //[TestMethod]
    //    //public void Test97()
    //    //{
    //    //    RunCalculations(johnMoves: 9, donMoves: 7);
    //    //}

    //    //[TestMethod]
    //    //public void Test79()
    //    //{
    //    //    RunCalculations(johnMoves: 7, donMoves: 9);
    //    //}

    //    //[TestMethod]
    //    //public void Test89()
    //    //{
    //    //    RunCalculations(johnMoves: 8, donMoves: 9);
    //    //}

    //    //[TestMethod]
    //    //public void Test98()
    //    //{
    //    //    RunCalculations(johnMoves: 9, donMoves: 8);
    //    //}

    //    //[TestMethod]
    //    //public void Test99()
    //    //{
    //    //    RunCalculations(johnMoves: 9, donMoves: 9);
    //    //}

    //    // ---------------------------------------------

    //    private static void RunCalculations(int johnMoves, int donMoves)
    //    {
    //        GameState initialState = new GameState();
    //        //MMCDebug debug = new MMCDebug();

    //        while (initialState.SortedMoves.Length > 0)
    //        {
    //            int lookAhead = initialState.Turn == Turn.John ? johnMoves : donMoves;
    //            IMMCAction? bestAction = MinMaxCalculator.GetBestAction(initialState, lookAhead/*, debug*/);
    //            //debug.Dump(initialState);
    //            initialState = initialState.ApplyAction(bestAction!) as GameState;
    //            //Console.WriteLine($"Best Action: {bestAction} leads to {initialState}");
    //        }
    //        Console.WriteLine($"john={johnMoves} don={donMoves} : {initialState}");
    //    }
    //}
}