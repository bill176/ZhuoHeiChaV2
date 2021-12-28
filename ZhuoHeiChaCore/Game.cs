using System;
using System.Collections.Generic;
using System.Linq;

namespace ZhuoHeiChaCore
{
    public class Game : IGame
    {
        private readonly List<Card> _remainingCards = new List<Card>();
        private readonly Dictionary<int, List<Card>> _cardsInHandByPlayerId = new Dictionary<int, List<Card>>();
        private readonly List<int> _remainingPlayers = new List<int>();
        //private readonly List<int> _remainingPlayers = new List<int>()  {  0,1,2};
        private int _currentPlayer;
        private int _capacity;
        private readonly List<int> _finishOrder = new List<int>();
        //private readonly List<int> _finishOrder = new List<int>{ 0, 2, 1};
        private readonly List<int> _blackAceList = new List<int>();      // 0 not Ace; 1 is Ace not public; 2 public Ace
        //private readonly List<int> _blackAceList = new List<int>() {1, 0, 0 };
        private readonly List<List<int>> tributeGroups = new List<List<int>>();
        private readonly List<(int, int)> tributePairs = new List<(int, int)>();

        // return player id
        public int addPlayer()      // controller will call this function
        {
            int id = _remainingPlayers.Count;
            _remainingPlayers.Add(id);
            return id;
        }

        // distribute cards, check tribute list, notify frontend, pay tribute
        private Dictionary<int, (IEnumerable<Card>, IEnumerable<Card>)> InitGame()
        {
            // distribute cards
            int numOfDecks = 1;   // TODO: need get some data from frontend ###
            int numOfPlayers = 3;       // int numOfPlayers = _remainingPlayers.Count() ###

            var cards = _cardFactory.GetFullDeckShuffled(numOfDecks);

            var deckForUser = cards.Select((s, i) => new { s, i })
                    .GroupBy(x => x.i % numOfPlayers)
                    .Select(g => g.Select(x => x.s).ToList())
                    .ToList();
            for (int i = 0; i < numOfPlayers; i++)
            {
                _cardsInHandByPlayerId.Add(i, deckForUser[i]);
                _cardsInHandByPlayerId[i].Sort(Card.ReverseComparator);

            }

            var cardsBeforeTributeByPlayerId = new Dictionary<int, IEnumerable<Card>>();
            foreach (var playerId in _remainingPlayers)
            {
                var cardsBeforeTribute = _cardsInHandByPlayerId[playerId].ToList();
                cardsBeforeTribute.Sort(Card.Comparator);
                cardsBeforeTributeByPlayerId[playerId] = cardsBeforeTribute;
            }

            // check tribute list
            SetTributeList();

            // send tribute
            PayTribute();

            // save card list after paying tribute
            var cardsPairByPlayerId = new Dictionary<int, (IEnumerable<Card>, IEnumerable<Card>)>();
            foreach (var playerId in _remainingPlayers)
            {
                // go back to the default ascending order; this is the list of cards after tribute
                var cardsAfterTribute = _cardsInHandByPlayerId[playerId].ToList();
                cardsAfterTribute.Sort(Card.Comparator);
                cardsPairByPlayerId[playerId] = (cardsBeforeTributeByPlayerId[playerId], cardsAfterTribute);
            }

            return cardsPairByPlayerId;
        }


        private void SetTributeList()
        {
            if (_finishOrder.Any(x => HasFourTwo(x)) && !HasFourTwo(_finishOrder[0]))
                return;
            // p p a a p [playerlist]
            // 1 2 3 4 5
            // (3,1) (3,2) (4,1) (4,2)
            // foreach pair in listofpair
            //      blackAceInfo[]
            // 3, p   start = 2, end = 1, type = a
            // 1, p
            // 2, a
            // 0, a
            // 4, p

            //{ {  3,1 }, {  2,0}, {  4} }

            // p a p a p
            int start = 0;
            int current = 0;
            while (current < _finishOrder.Count - 1)
            {
                if (_blackAceList[current] == _blackAceList[current + 1])
                {
                    current++;
                }
                else 
                {
                    tributeGroups.Add(Enumerable.Range(start, current - start + 1).Select(idx => _finishOrder[idx]).ToList());
                    start = current + 1;
                    current = start;

                }
            }
            tributeGroups.Add(Enumerable.Range(start, current - start + 1).Select(idx => _finishOrder[idx]).ToList());

            var tributeGroupArray = tributeGroups.ToArray();
            for (var i = 0; i < tributeGroupArray.Length - 1; ++i)
            {
                var thisGroup = tributeGroupArray[i];
                var nextGroup = tributeGroupArray[i + 1];

                foreach (var receiving in thisGroup)
                {
                    foreach (var paying in nextGroup)
                    {
                        if (!HasTwoCats(paying))
                            tributePairs.Add((paying, receiving));
                    }
                }
            }

        }

        private bool HasFourTwo(int player_id)
        {
            var cardTypeList = _cardsInHandByPlayerId[player_id].Select(x => x.CardType);
            return cardTypeList.Contains(CardType.SPADE_TWO) && cardTypeList.Contains(CardType.HEART_TWO)
                && cardTypeList.Contains(CardType.DIAMOND_TWO) && cardTypeList.Contains(CardType.CLUBS_TWO);
        }

        private bool HasTwoCats(int player_id)
        {
            var cardTypeList = _cardsInHandByPlayerId[player_id].Select(x => x.CardType);
            return cardTypeList.Contains(CardType.JOKER_SMALL) && cardTypeList.Contains(CardType.JOKER_BIG);
        }

        /// <summary>
        /// group tributeList according to Black ACEs
        /// call tribute in order
        /// refer to public ace list
        /// </summary>
        private void PayTribute()
        {

            foreach (var (paying, receiving) in tributePairs)
            {
                int round = Math.Max(_blackAceList[paying], _blackAceList[receiving]);
                for (int i = 0; i < round; i++)
                    PayTribute(paying, receiving);
            }
        }


        // exclude Black Ace for now
        public void PayTribute(int payingPlayerId, int receivingPlayerId)
        {
            var tribute = _cardsInHandByPlayerId[payingPlayerId].First(x=>x.CardType != CardType.SPADE_ACE);

            _cardsInHandByPlayerId[payingPlayerId].Remove(tribute);
            _cardsInHandByPlayerId[receivingPlayerId].Add(tribute);
            _cardsInHandByPlayerId[receivingPlayerId].Sort(Card.ReverseComparator);
        }

        // Key: source player id
        // Value: a dictionary with
        //      Key: target player id
        //      Value: number of cards to send
        private readonly Dictionary<int, Dictionary<int, int>> _returnTributeDependencyGraph = new Dictionary<int, Dictionary<int, int>>();

        // Key: source player id
        // Value: a dictionary with
        //      Key: target player id
        //      Value: list of cards to send
        private readonly Dictionary<int, Dictionary<int, List<Card>>> _returnTributeCardsBuffer = new Dictionary<int, Dictionary<int, List<Card>>>();

        private readonly ICardFactory _cardFactory;
        private readonly ICardHelper _cardHelper;

        public Game(ICardFactory cardFactory, ICardHelper cardHelper, int capacity)
        {
            _cardFactory = cardFactory;
            _cardHelper = cardHelper;

            _capacity = capacity;
        }

        public Dictionary<int, IEnumerable<Card>> SendCards(int sourcePlayerId, int targetPlayerId, IEnumerable<Card> cards)
        {
            var cardList = cards.ToList();

            // check if both ids are valid
            if (!_returnTributeDependencyGraph.TryGetValue(sourcePlayerId, out var receivingPlayers)
                || !receivingPlayers.TryGetValue(targetPlayerId, out var numOfCardsToSend))
                throw new ArgumentException($"Player {sourcePlayerId} cannot send cards to player {targetPlayerId}!");

            // check if sourcePlayer has the cards
            if (!PlayerHasCards(sourcePlayerId, cardList))
                throw new ArgumentException($"Player {sourcePlayerId} doesn't have all of the following cards: " +
                    $"{_cardHelper.ConvertCardsToString(cardList)}");

            // check if number of cards to be sent is correct
            if (cardList.Count != numOfCardsToSend)
                throw new ArgumentException($"Player {sourcePlayerId} is trying to send {cardList.Count} cards to " +
                    $"{targetPlayerId}. Expecting {numOfCardsToSend} cards.");

            // store the returned cards to the buffer
            if (!_returnTributeCardsBuffer.TryGetValue(sourcePlayerId, out var tributeCardsByTargetPlayerId))
            {
                _returnTributeCardsBuffer[sourcePlayerId] = new Dictionary<int, List<Card>>
                {
                    { targetPlayerId, cardList }
                };
            }
            else
            {
                tributeCardsByTargetPlayerId[targetPlayerId] = cardList;
            }

            // remove the dependency to avoid duplication
            _returnTributeDependencyGraph[sourcePlayerId].Remove(targetPlayerId);
            if (_returnTributeDependencyGraph[sourcePlayerId].Count == 0)
                _returnTributeDependencyGraph.Remove(sourcePlayerId);

            // process the buffer if all tributes have been sent
            var playerCardsDictionary = new Dictionary<int, IEnumerable<Card>>();
            if (_returnTributeDependencyGraph.Count == 0)
                playerCardsDictionary = ProcessTributeBuffer();

            return playerCardsDictionary;
        }

        /// <summary>
        /// Add a new player to the current game. Throws exception if max capacity reached
        /// </summary>
        /// <returns>Id of the newly added player</returns>
        public int AddPlayer()
        {
            if (_remainingPlayers.Count >= _capacity)
                throw new InvalidOperationException($"Cannot add a new player to game! Max capacity {_capacity} reached!");

            var newPlayerId = _remainingPlayers.Count;
            _remainingPlayers.Add(newPlayerId);

            return newPlayerId;
        }

        /// <summary>
        /// Check if all of cards belong to the player
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="cards"></param>
        private bool PlayerHasCards(int playerId, IEnumerable<Card> cards)
        {
            return cards.All(card => _cardsInHandByPlayerId[playerId].Contains(card));
        }

        /// <summary>
        /// Commits the tribute card changes previously stored in the buffer
        /// </summary>
        /// <returns>A dictionary containing the list of players whose cards changed as well 
        /// as their updated cards</returns>
        private Dictionary<int, IEnumerable<Card>> ProcessTributeBuffer()
        {
            var playersWithModifiedCards = new HashSet<int>();
            foreach (var srcPlayerId in _returnTributeCardsBuffer.Keys)
            {
                playersWithModifiedCards.Add(srcPlayerId);
                foreach (var tgtPlayerId in _returnTributeCardsBuffer[srcPlayerId].Keys)
                {
                    playersWithModifiedCards.Add(tgtPlayerId);
                    foreach (var card in _returnTributeCardsBuffer[srcPlayerId][tgtPlayerId])
                    {
                        _cardsInHandByPlayerId[srcPlayerId].Remove(card);
                        _cardsInHandByPlayerId[tgtPlayerId].Add(card);
                    }
                }
            }

            var playerCardsDictionary = new Dictionary<int, IEnumerable<Card>>();

            // TODO: only sort players with added cards
            foreach (var player in playersWithModifiedCards)
            {
                _cardsInHandByPlayerId[player].Sort(Card.Comparator);
                playerCardsDictionary[player] = _cardsInHandByPlayerId[player];
            }

            return playerCardsDictionary;
        }
    }

    public interface IGame
    {
        Dictionary<int, IEnumerable<Card>> SendCards(int sourcePlayerId, int targetPlayerId, IEnumerable<Card> cards);
        int AddPlayer();
    }

}
