using System;
using System.Collections.Generic;
using System.Linq;

namespace ZhuoHeiChaCore
{
    public class Game : IGame
    {
        private int _capacity;
        private readonly List<Card> _remainingCards = new List<Card>();
        private readonly Dictionary<int, List<Card>> _cardsInHandByPlayerId = new Dictionary<int, List<Card>>();
        private readonly List<PlayerType> _playerTypeList = new List<PlayerType>();      // 0 not Ace; 1 is Ace not public; 2 public Ace
        private readonly List<int> _remainingPlayers = new List<int>();
        private readonly List<(int, int)> _tributePairs = new List<(int, int)>();
        
        private readonly List<int> _finishOrder = new List<int>();
        
        private int _currentPlayer = 0;
        private int _lastValidPlayer = 0;        
        private Hand _lastValidHand = HandFactory.EMPTY_HAND;
        private bool _didBlackAceWin = false;

        // Key: source player id
        // Value: a dictionary with
        //      Key: target player id
        //      Value: number of cards to send
        private readonly Dictionary<int, Dictionary<int, int>> _returnTributeDependencyGraph = new Dictionary<int, Dictionary<int, int>>();

        // ((payer, receiver), list of cards)
        private readonly List<((int, int), List<Card>)> _returnTributeCardsBuffer = new List<((int, int), List<Card>)>();

        private readonly ICardFactory _cardFactory;
        private readonly ICardHelper _cardHelper;
        private readonly IGameHelper _gameHelper;

        public Game(ICardFactory cardFactory, ICardHelper cardHelper, IGameHelper gameHelper, int capacity)
        {
            _cardFactory = cardFactory;
            _cardHelper = cardHelper;
            _gameHelper = gameHelper;

            _capacity = capacity;
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


        // distribute cards, check tribute list, notify frontend, pay tribute
        private Dictionary<int, (IEnumerable<Card>, IEnumerable<Card>)> InitGame(int numOfDecks = 1)
        {
            // distribute cards
            int numOfPlayers = 3;       // int numOfPlayers = _cardsInHandByPlayerId.Count() ###

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

            // return cards for player before paying tribute
            var cardsBeforeTributeByPlayerId = new Dictionary<int, IEnumerable<Card>>();
            foreach (var playerId in _remainingPlayers)
            {
                var cardsBeforeTribute = _cardsInHandByPlayerId[playerId].ToList();
                cardsBeforeTribute.Sort(Card.Comparator);
                cardsBeforeTributeByPlayerId[playerId] = cardsBeforeTribute;
            }

            // generate tribute list
            var tributePairs = GetTributePairs();
            _tributePairs.Clear();
            _tributePairs.AddRange(tributePairs);

            // send tribute
            PayTribute();

            _finishOrder.Clear();
            // initialize player type list list based on cards in hand
            foreach (var kvp in _cardsInHandByPlayerId)
                _playerTypeList[kvp.Key] = _gameHelper.GetPlayerType(kvp.Value);

            _currentPlayer = 0;
            _lastValidHand = HandFactory.EMPTY_HAND;
            _lastValidPlayer = 0;
            _didBlackAceWin = false;

            // save card list after paying tribute
            var cardsPairByPlayerId = new Dictionary<int, (IEnumerable<Card>, IEnumerable<Card>)>();
            foreach (var playerId in _remainingPlayers)
            {
                // go back to the default ascending order; this is the list of cards after tribute
                var cardsAfterTribute = _cardsInHandByPlayerId[playerId].ToList();
                cardsAfterTribute.Sort(Card.Comparator);
                cardsPairByPlayerId[playerId] = (cardsBeforeTributeByPlayerId[playerId], cardsAfterTribute);
            }
            
            // TODO: start return tribute
            return cardsPairByPlayerId;
        }

        private IEnumerable<(int, int)> GetTributePairs()
        {
            if (_finishOrder.Any(x => _gameHelper.HasFourTwo(_cardsInHandByPlayerId[x]))
                && !_gameHelper.HasFourTwo(_cardsInHandByPlayerId[_finishOrder[0]]))
                return Enumerable.Empty<(int, int)>();

            var finishGroupsSequence = _gameHelper.GroupConsecutivePlayersOfSameType(_finishOrder, idx => _playerTypeList[idx]);
            var payerReceiverPairs = _gameHelper.GeneratePayerReceiverPairsForConsecutiveGroups(finishGroupsSequence);

            return payerReceiverPairs.Where(pair => !_gameHelper.HasTwoCats(_cardsInHandByPlayerId[pair.payer]));
        }

        /// <summary>
        /// group tributeList according to Black ACEs
        /// call tribute in order
        /// refer to public ace list
        /// </summary>
        private void PayTribute()
        {
            foreach (var (paying, receiving) in _tributePairs)
            {
                var numOfTributeCards = GetNumOfTributeCards(paying, receiving);
                for (int i = 0; i < numOfTributeCards; i++)
                    PayTribute(paying, receiving);
            }
        }

        private int GetNumOfTributeCards(int payer, int receiver)
        {
            return Math.Max((int)_playerTypeList[payer], (int)_playerTypeList[receiver]);
        }


        public void PayTribute(int payingPlayerId, int receivingPlayerId)
        {
            var tribute = _cardsInHandByPlayerId[payingPlayerId].First(x  =>  x.CardType != CardType.SPADE_ACE);

            _cardsInHandByPlayerId[payingPlayerId].Remove(tribute);
            _cardsInHandByPlayerId[receivingPlayerId].Add(tribute);
            _cardsInHandByPlayerId[receivingPlayerId].Sort(Card.ReverseComparator);
        }


        public Dictionary<int, IEnumerable<Card>> ReturnTribute(int sourcePlayerId, int targetPlayerId, IEnumerable<Card> cards)
        {
            var cardList = cards.ToList();

            // check if both ids are valid
            var pairIndex = _tributePairs.FindIndex(p => p.Item1 == sourcePlayerId && p.Item2 == targetPlayerId);
            if (pairIndex == -1)
                throw new ArgumentException($"Player {sourcePlayerId} cannot send cards to player {targetPlayerId}!");

            // check if sourcePlayer has the cards
            if (!PlayerHasCards(sourcePlayerId, cardList))
                throw new ArgumentException($"Player {sourcePlayerId} doesn't have all of the following cards: " +
                    $"{_cardHelper.ConvertCardsToString(cardList)}");

            // check if number of cards to be sent is correct
            var numOfCardsToSend = GetNumOfTributeCards(sourcePlayerId, targetPlayerId);
            if (cardList.Count != numOfCardsToSend)
                throw new ArgumentException($"Player {sourcePlayerId} is trying to send {cardList.Count} cards to " +
                    $"{targetPlayerId}. Expecting {numOfCardsToSend} cards.");

            // store the returned cards to the buffer
            _returnTributeCardsBuffer.Add(((_tributePairs[pairIndex].Item1, _tributePairs[pairIndex].Item2), cardList));

            // remove the dependency to avoid duplication
            _tributePairs.RemoveAt(pairIndex);

            // process the buffer if all tributes have been sent
            var playerCardsDictionary = new Dictionary<int, IEnumerable<Card>>();
            if (_tributePairs.Count == 0)
                playerCardsDictionary = ProcessTributeBuffer();

            return playerCardsDictionary;
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
            foreach (var ((payer, receiver), cards) in _returnTributeCardsBuffer)
            {
                foreach (var card in cards)
                {
                    _cardsInHandByPlayerId[payer].Remove(card);
                    _cardsInHandByPlayerId[receiver].Add(card);
                }
            }

            var playerCardsDictionary = new Dictionary<int, IEnumerable<Card>>();

            // TODO: only sort players with added cards
            foreach (var player in _remainingPlayers)
            {
                _cardsInHandByPlayerId[player].Sort(Card.Comparator);
                playerCardsDictionary[player] = _cardsInHandByPlayerId[player];
            }

            return playerCardsDictionary;
        }

        public GameActionResult AceGoPublic(int goPublicPlayerId)
        {
            // check valid or not
            if (!_remainingPlayers.Contains(goPublicPlayerId))
                return new GameActionResult(GameReturnType.Error, " player id is not valid");
            else if (_playerTypeList[goPublicPlayerId] != PlayerType.Ace)
                return new GameActionResult(GameReturnType.Error, " this player is not Black Ace, can not go public");
            else 
            {
                _playerTypeList[goPublicPlayerId] = PlayerType.PublicAce;
                return new GameActionResult(GameReturnType.NoAction);
            }
        }

        /// <summary>
        /// input: playerId: of whom want to play; UserCard: the cards it wants to play
        /// check whether the cards is valid, and wether it is greater than the lastHand. return true if valid and greater than the lastHand or skip.
        /// return false, iff need user to resubmit
        /// </summary>
        public GameActionResult PlayHand(int playerId, List<Card> UserCard)     // use CardFactory to create UserCard
        {
            int possible_next_player = (_currentPlayer + 1) % _cardsInHandByPlayerId.Count;

            if(_currentPlayer != playerId)
            {
                throw new ArgumentException($"Player {playerId} cannot play hand now, because he/she is not the current player!");
            }

            Hand userHand = HandFactory.EMPTY_HAND;
            try
            {
                userHand = HandFactory.CreateHand(UserCard);    // throw an exception if not valid
            }
            catch
            {
                return new GameActionResult(GameReturnType.Resubmit, " Hand is not valid ");
            }

            if (userHand.Group == HandFactory.EMPTY_HAND.Group && _lastValidPlayer != playerId)    // dealer cannot skip
            {
                // change current player to the next one.
                while (!_remainingPlayers.Contains(possible_next_player))
                {
                    possible_next_player = (possible_next_player + 1) % _cardsInHandByPlayerId.Count;
                }
                _currentPlayer = possible_next_player;

                return new GameActionResult(GameReturnType.PlayHandSuccess);
            }

            if (_lastValidPlayer == playerId)
                if (userHand.Group == HandFactory.EMPTY_HAND.Group)      // dealer cannot skip
                    return new GameActionResult(GameReturnType.Resubmit, " dealer cannot skip ");
                else
                { 
                    _lastValidHand = HandFactory.EMPTY_HAND;
                }

            if (!userHand.CompareValue(_lastValidHand))
                return new GameActionResult(GameReturnType.Resubmit, " your hand is smaller then the last hand ");

            foreach (Card c in UserCard)
                _cardsInHandByPlayerId[playerId].Remove(c);

            // change current player to the next one.
            while (!_remainingPlayers.Contains(possible_next_player))
            {
                possible_next_player = (possible_next_player + 1) % _cardsInHandByPlayerId.Count;
            }
            _currentPlayer = possible_next_player;

            _lastValidPlayer = playerId;
            CheckPlayerFinished(playerId);
            if (CheckGameEnded())
                return new GameActionResult(GameReturnType.GameEnded);

            // TODO: update UserCard to be the remaining cards of the player
            return new GameActionResult(GameReturnType.PlayHandSuccess, UserCard, _currentPlayer);     // send back the update cards
        }

        private void CheckPlayerFinished(int playerId)
        {
            if (_cardsInHandByPlayerId[playerId].Count == 0) 
            {
                _finishOrder.Add(playerId);
                _remainingPlayers.Remove(playerId);
                _lastValidHand = HandFactory.EMPTY_HAND;
                // change next valid player to the next one.
                int possible_next_valid_player = (_lastValidPlayer + 1) % _cardsInHandByPlayerId.Count;
                while (!_remainingPlayers.Contains(possible_next_valid_player))
                {
                    possible_next_valid_player = (possible_next_valid_player + 1) % _cardsInHandByPlayerId.Count;
                }
                _lastValidPlayer = possible_next_valid_player;
            }

        }

        private bool CheckGameEnded()
        {
            if (_remainingPlayers.All(x => _playerTypeList[x] == PlayerType.Normal))
            {
                _didBlackAceWin = false; // set who wins
                _finishOrder.AddRange(_remainingPlayers);
                _remainingPlayers.Clear();
                return true;
            }
            else if (_remainingPlayers.All(x => _playerTypeList[x] != PlayerType.Normal))
            {
                _didBlackAceWin = true;
                _finishOrder.AddRange(_remainingPlayers);
                _remainingPlayers.Clear();
                _cardsInHandByPlayerId.Clear();
                return true;
            }
            else
                return false;
        }
    }

    public interface IGame
    {
        Dictionary<int, IEnumerable<Card>> ReturnTribute(int sourcePlayerId, int targetPlayerId, IEnumerable<Card> cards);
        int AddPlayer();
    }

    public enum PlayerType
    {
        Normal,
        Ace,
        PublicAce
    }
}
