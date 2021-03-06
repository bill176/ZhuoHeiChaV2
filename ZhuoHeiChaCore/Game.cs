using System;
using System.Collections.Generic;
using System.Linq;
using ZhuoHeiChaCore.ReturnTypeAndValue;

namespace ZhuoHeiChaCore
{
    public class Game : IGame
    {
        public int Capacity => _capacity;
        public List<int> RemainingPlayers { get; private set; } = new List<int>();
        public int CurrentPlayer { get; private set; } = 0;
        public int LastValidPlayer { get; private set; } = 0;

        protected int _capacity;
        protected readonly Dictionary<int, List<Card>> _cardsInHandByPlayerId = new Dictionary<int, List<Card>>();
        protected readonly List<PlayerType> _playerTypeList = new List<PlayerType>();      // 0 not Ace; 1 is Ace not public; 2 public Ace
        protected readonly List<(int, int)> _tributePairs = new List<(int, int)>();
        protected readonly List<int> _finishOrder = new List<int>();
        protected Hand _lastValidHand = HandFactory.EMPTY_HAND;
        protected bool _isBlackAceWin = false;

        // Key: source player id
        // Value: a dictionary with
        //      Key: target player id
        //      Value: number of cards to send
        protected readonly Dictionary<int, Dictionary<int, int>> _returnTributeDependencyGraph = new Dictionary<int, Dictionary<int, int>>();

        // ((payer, receiver), list of cards)
        protected readonly List<((int, int), List<Card>)> _returnTributeCardsBuffer = new List<((int, int), List<Card>)>();

        protected ICardFactory _cardFactory;
        protected ICardHelper _cardHelper;
        protected IGameHelper _gameHelper;

        public Game(ICardFactory cardFactory, ICardHelper cardHelper, IGameHelper gameHelper, int capacity)
        {
            _cardFactory = cardFactory;
            _cardHelper = cardHelper;
            _gameHelper = gameHelper;

            _capacity = capacity;
        }

        protected Game() { }

        /// <summary>
        /// Add a new player to the current game. Throws exception if max capacity reached
        /// </summary>
        /// <returns>Id of the newly added player</returns>
        public int AddPlayer()
        {
            if (RemainingPlayers.Count >= _capacity)
                throw new InvalidOperationException($"Cannot add a new player to game! Max capacity {_capacity} reached!");

            var newPlayerId = RemainingPlayers.Count;
            RemainingPlayers.Add(newPlayerId);

            return newPlayerId;
        }


        // distribute cards, check tribute list, notify frontend, pay tribute
        public InitGameReturnValue InitGame(int numOfDecks = 1)
        {
            if (RemainingPlayers.Count == 0)
            {
                for (int playerId = 0; playerId < Capacity; playerId++)
                {
                    RemainingPlayers.Add(playerId);
                }
            }


            // distribute cards

            var cards = _cardFactory.GetFullDeckShuffled(numOfDecks);

            var deckForUser = cards.Select((s, i) => new { s, i })
                    .GroupBy(x => x.i % Capacity)
                    .Select(g => g.Select(x => x.s).ToList())
                    .ToList();
            for (int i = 0; i < Capacity; i++)
            {
                _cardsInHandByPlayerId.Add(i, deckForUser[i]);
                _cardsInHandByPlayerId[i].Sort(Card.ReverseComparator);
            }

            ////for test
            //var firstCards = _cardHelper.ConvertIdsToCards(new List<int> { 2, 1, 0 }).ToList();
            //var secondCards = _cardHelper.ConvertIdsToCards(new List<int> { 5, 4, 3 }).ToList();
            //var thirdCards = _cardHelper.ConvertIdsToCards(new List<int> { 12, 11, 10, 9, 8, 7 }).ToList();
            //var fourthCards = _cardHelper.ConvertIdsToCards(new List<int> { 21, 20, 19}).ToList();
            //_cardsInHandByPlayerId[0] = firstCards;
            //_cardsInHandByPlayerId[1] = secondCards;
            //_cardsInHandByPlayerId[2] = thirdCards;
            //_cardsInHandByPlayerId[3] = fourthCards;
            ////////////


            // return cards for player before paying tribute
            var cardsBeforeTributeByPlayerId = new Dictionary<int, IEnumerable<Card>>();


            for (int playerId = 0; playerId < Capacity; playerId++)
            {
                var cardsBeforeTribute = _cardsInHandByPlayerId[playerId].ToList();
                cardsBeforeTribute.Sort(Card.Comparator);
                cardsBeforeTributeByPlayerId[playerId] = cardsBeforeTribute;
            }

            // generate tribute list
            var tributePairs = GetTributePairs();
            _tributePairs.Clear();

            _tributePairs.AddRange(tributePairs);

            var isFirstRound = _tributePairs.Count == 0;

            // send tribute
            PayTribute();

            _finishOrder.Clear();
            if (_playerTypeList.Count == 0)
            {
                foreach (var kvp in _cardsInHandByPlayerId)
                    _playerTypeList.Add(_gameHelper.GetPlayerType(kvp.Value));
            }

            CurrentPlayer = 0;
            _lastValidHand = HandFactory.EMPTY_HAND;
            LastValidPlayer = 0;
            _isBlackAceWin = false;

            // save card list after paying tribute
            var cardsPairByPlayerId = new Dictionary<int, (IEnumerable<Card>, IEnumerable<Card>)>();
            for (int playerId = 0; playerId < Capacity; playerId++)
            {
                // go back to the default ascending order; this is the list of cards after tribute
                var cardsAfterTribute = _cardsInHandByPlayerId[playerId].ToList();
                cardsAfterTribute.Sort(Card.Comparator);
                cardsPairByPlayerId[playerId] = (cardsBeforeTributeByPlayerId[playerId], cardsAfterTribute);
            }


            var returnTributeListByPlayerId = new Dictionary<int, List<int>>();
            var cardsToBeReturnCount = new Dictionary<int, List<int>>();

            for (int playerId = 0; playerId < Capacity; playerId++)
            {
                returnTributeListByPlayerId[playerId] = _tributePairs.Where(p => p.Item2 == playerId).Select(p => p.Item1).ToList();
                cardsToBeReturnCount[playerId] = _tributePairs.Where(p => p.Item2 == playerId).Select(p => GetNumOfTributeCards(p.Item1, p.Item2)).ToList();

            }

            return new InitGameReturnValue
            {
                CardsPairsByPlayerId = cardsPairByPlayerId,
                ReturnTributeListByPlayerId = returnTributeListByPlayerId,
                CardsToBeReturnCount = cardsToBeReturnCount,
                IsFirstRound = isFirstRound

            };
        }
        public ReturnTributeReturnValue ReturnTribute(int payer, int receiver, IEnumerable<Card> cards)
        {
            var cardList = cards.ToList();

            // check if both ids are valid
            var pairIndex = _tributePairs.FindIndex(p => p.Item1 == payer && p.Item2 == receiver);
            if (pairIndex == -1)
                throw new ArgumentException($"Player {receiver} cannot send cards to player {payer}!");

            // check if sourcePlayer has the cards
            if (!PlayerHasCards(receiver, cardList))
                throw new ArgumentException($"Player {receiver} doesn't have all of the following cards: " +
                    $"{_cardHelper.ConvertCardsToString(cardList)}");

            // check if number of cards to be sent is correct
            var numOfCardsToSend = GetNumOfTributeCards(payer, receiver);
            if (cardList.Count != numOfCardsToSend)
                throw new ArgumentException($"Player {receiver} is trying to send {cardList.Count} cards to " +
                    $"{payer}. Expecting {numOfCardsToSend} cards.");


            // store the returned cards to the buffer
            _returnTributeCardsBuffer.Add(((_tributePairs[pairIndex].Item1, _tributePairs[pairIndex].Item2), cardList));

            // remove the dependency to avoid duplication
            _tributePairs.RemoveAt(pairIndex);

            // process the buffer if all tributes have been sent
            var cardsAfterReturnTribute = new Dictionary<int, IEnumerable<Card>>();
            if (_tributePairs.Count == 0)
            {
                cardsAfterReturnTribute = ProcessTributeBuffer();

                _playerTypeList.Clear();
                // initialize player type list list based on cards in hand
                foreach (var kvp in _cardsInHandByPlayerId)
                    _playerTypeList.Add(_gameHelper.GetPlayerType(kvp.Value));
            }

            return new ReturnTributeReturnValue
            {
                cardsAfterReturnTribute = cardsAfterReturnTribute,
                returnTributeValid = true
            };
        }
        public void AceGoPublic(int goPublicPlayerId)
        {
            // check valid or not
            if (!RemainingPlayers.Contains(goPublicPlayerId) || _playerTypeList[goPublicPlayerId] != PlayerType.Ace)
            {
                throw new ArgumentException($"Player {goPublicPlayerId} is not a black ace!");
            }

            _playerTypeList[goPublicPlayerId] = PlayerType.PublicAce;
            CurrentPlayer = goPublicPlayerId;
            LastValidPlayer = goPublicPlayerId;
        }

        /// <summary>
        /// input: playerId: of whom want to play; UserCard: the cards it wants to play
        /// check whether the cards is valid, and wether it is greater than the lastHand. return true if valid and greater than the lastHand or skip.
        /// return false, iff need user to resubmit
        /// </summary>
        public PlayHandReturn PlayHand(int playerId, List<Card> UserCard)     // use CardFactory to create UserCard
        {
            int possible_next_player = (CurrentPlayer + 1) % _cardsInHandByPlayerId.Count;

            if (CurrentPlayer != playerId)
            {
                throw new ArgumentException($"Player {playerId} cannot play hand now, because he/she is not the current player!");
            }

            foreach (Card c in UserCard)
                if (!_cardsInHandByPlayerId[playerId].Contains(c))
                    throw new ArgumentException($"Player {playerId} does not have these cards!");

            Hand userHand = HandFactory.EMPTY_HAND;
            try
            {
                userHand = HandFactory.CreateHand(UserCard);    // throw an exception if not valid
            }
            catch
            {
                return new PlayHandReturn(PlayHandReturnType.Resubmit, " Hand is not valid ");
            }

            if (userHand.Group == HandFactory.EMPTY_HAND.Group && LastValidPlayer != playerId)    // currentPlayer cannot skip
            {
                // change current player to the next one.
                while (!RemainingPlayers.Contains(possible_next_player))
                {
                    possible_next_player = (possible_next_player + 1) % _cardsInHandByPlayerId.Count;
                }
                CurrentPlayer = possible_next_player;

                var a = _lastValidHand.ListOfCards;
                return new PlayHandReturn(PlayHandReturnType.PlayHandSuccess, a, CurrentPlayer);
            }

            if (LastValidPlayer == playerId)
                if (userHand.Group == HandFactory.EMPTY_HAND.Group)      // currentPlayer cannot skip
                    return new PlayHandReturn(PlayHandReturnType.Resubmit, " dealer cannot skip ");
                else
                {
                    _lastValidHand = HandFactory.EMPTY_HAND;
                }

            if (!userHand.CompareValue(_lastValidHand))
                return new PlayHandReturn(PlayHandReturnType.Resubmit, " your hand is smaller then the last hand ");

            _lastValidHand = userHand;
            foreach (Card c in UserCard)
                _cardsInHandByPlayerId[playerId].Remove(c);

            // change current player to the next one.
            while (!RemainingPlayers.Contains(possible_next_player))
            {
                possible_next_player = (possible_next_player + 1) % _cardsInHandByPlayerId.Count;
            }
            CurrentPlayer = possible_next_player;

            LastValidPlayer = playerId;
            CheckPlayerFinished(playerId);
            if (CheckGameEnded())
            {
                return new PlayHandReturn(PlayHandReturnType.GameEnded, _isBlackAceWin);
            }


            return new PlayHandReturn(PlayHandReturnType.PlayHandSuccess, UserCard, CurrentPlayer);     // send back the update cards
        }
        public IEnumerable<bool> IsBlackAceList()
        {
            return _playerTypeList.Select(t => t == PlayerType.Ace || t == PlayerType.PublicAce);
        }

        public Dictionary<int, int> GetOpponentCardsCount()
        {
            var a = _cardsInHandByPlayerId.Select(kvp => new KeyValuePair<int, int>(kvp.Key, kvp.Value.Count));

            return new Dictionary<int, int>(a);
        }


        protected IEnumerable<(int, int)> GetTributePairs()
        {
            // return empty list at first round.
            if (_finishOrder.Count == 0)
                return Enumerable.Empty<(int, int)>();
            if (_finishOrder.Any(x => _gameHelper.HasFourTwo(_cardsInHandByPlayerId[x]))
                && !_gameHelper.HasFourTwo(_cardsInHandByPlayerId[_finishOrder[0]]))
                return Enumerable.Empty<(int, int)>();

            var finishGroupsSequence = _gameHelper.GroupConsecutivePlayersOfSameType(_finishOrder, _playerTypeList);
            var payerReceiverPairs = _gameHelper.GeneratePayerReceiverPairsForConsecutiveGroups(finishGroupsSequence);

            return payerReceiverPairs.Where(pair => !_gameHelper.HasTwoCats(_cardsInHandByPlayerId[pair.payer]));
        }

        /// <summary>
        /// group tributeList according to Black ACEs
        /// call tribute in order
        /// refer to public ace list
        /// </summary>
        protected void PayTribute()
        {
            foreach (var (paying, receiving) in _tributePairs)
            {
                var cardsToBeReturnCount = GetNumOfTributeCards(paying, receiving);
                for (int i = 0; i < cardsToBeReturnCount; i++)
                    PayTribute(paying, receiving);
            }
        }

        protected int GetNumOfTributeCards(int payer, int receiver)
        {
            return Math.Max((int)_playerTypeList[payer], (int)_playerTypeList[receiver]);
        }
        protected void CheckPlayerFinished(int playerId)
        {
            if (_cardsInHandByPlayerId[playerId].Count == 0)
            {
                _finishOrder.Add(playerId);
                RemainingPlayers.Remove(playerId);
                _lastValidHand = HandFactory.EMPTY_HAND;
                // change next valid player to the next one.
                int possible_next_valid_player = (LastValidPlayer + 1) % _cardsInHandByPlayerId.Count;
                while (!RemainingPlayers.Contains(possible_next_valid_player))
                {
                    possible_next_valid_player = (possible_next_valid_player + 1) % _cardsInHandByPlayerId.Count;
                }
                LastValidPlayer = possible_next_valid_player;
            }

        }

        protected bool CheckGameEnded()
        {
            if (RemainingPlayers.All(x => _playerTypeList[x] == PlayerType.Normal))
            {
                _isBlackAceWin = true; // determin who wins
                _finishOrder.AddRange(RemainingPlayers);
                RemainingPlayers.Clear();
                _cardsInHandByPlayerId.Clear();
                return true;
            }
            else if (RemainingPlayers.All(x => _playerTypeList[x] != PlayerType.Normal))
            {
                _isBlackAceWin = false;
                _finishOrder.AddRange(RemainingPlayers);
                RemainingPlayers.Clear();
                _cardsInHandByPlayerId.Clear();
                return true;
            }
            else
                return false;
        }
        private void PayTribute(int payer, int receiver)
        {
            var tribute = _cardsInHandByPlayerId[payer].First(x => x.CardType != CardType.SPADE_ACE);

            _cardsInHandByPlayerId[payer].Remove(tribute);
            _cardsInHandByPlayerId[receiver].Add(tribute);
            _cardsInHandByPlayerId[receiver].Sort(Card.ReverseComparator);
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
                Console.WriteLine(payer + "->" + receiver);
                Console.WriteLine(string.Join(",", cards.Select(x => x.CardType.ToString()) ));
                foreach (var card in cards)
                {
                    _cardsInHandByPlayerId[receiver].Remove(card);
                    _cardsInHandByPlayerId[payer].Add(card);
                }
            }

            _returnTributeCardsBuffer.Clear();

            var playerCardsDictionary = new Dictionary<int, IEnumerable<Card>>();

            // TODO: only sort players with added cards
            for (int playerId = 0; playerId < Capacity; playerId++)
            {
                _cardsInHandByPlayerId[playerId].Sort(Card.Comparator);
                playerCardsDictionary[playerId] = _cardsInHandByPlayerId[playerId];
            }

            return playerCardsDictionary;
        }

    }

    public interface IGame
    {
        int Capacity { get; }
        List<int> RemainingPlayers { get; }
        int CurrentPlayer { get; }
        int LastValidPlayer { get; }
        IEnumerable<bool> IsBlackAceList();


        InitGameReturnValue InitGame(int numOfDecks = 1);
        ReturnTributeReturnValue ReturnTribute(int sourcePlayerId, int targetPlayerId, IEnumerable<Card> cards);
        int AddPlayer();
        void AceGoPublic(int goPublicPlayerId);
        PlayHandReturn PlayHand(int playerId, List<Card> UserCard);
        Dictionary<int, int> GetOpponentCardsCount();
    }
}
