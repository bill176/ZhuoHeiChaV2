using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using ZhuoHeiChaCore;
using ZhuoHeiChaCore.Factories;
using ZhuoHeiChaCore.ReturnTypeAndValue;

namespace ZhuoHeiChaAPI.Services
{
    public class GameService : IGameService
    {
        // This table is for Returning Tribute one by one, is has form: <gameId, <receiverId, PayerInfo>>, PayerInfo contains
        // payerId and how many cards needed to be returned. With this table, we can call NotifyReturnTribute in GameController
        // one by one.
        private Dictionary<int, Dictionary<int, List<PayerInfo>>> _returnTable = new Dictionary<int, Dictionary<int, List<PayerInfo>>>();

        private int _gameCounter;
        private readonly ConcurrentDictionary<int, (IGame, object)> _gameSessions = new ConcurrentDictionary<int, (IGame, object)>();

        private readonly ILogger<GameService> _logger;
        private readonly ICardHelper _cardHelper;
        private readonly IGameFactory _gameFactory;



        public void InitReturnTable(int gameId, Dictionary<int, List<int>> returnTributeListByPlayerId, Dictionary<int, List<int>> cardsToBeReturnCount)
        {
            _returnTable[gameId] = new Dictionary<int, List<PayerInfo>>();

            foreach (var receiverId in returnTributeListByPlayerId.Keys)
            {

                _returnTable[gameId][receiverId] = new List<PayerInfo>();

                var ReturnTributeList = returnTributeListByPlayerId[receiverId].ToList();
                var cardsToBeReturnCountList = cardsToBeReturnCount[receiverId].ToList();

                for (int i = 0; i < ReturnTributeList.Count; i++)
                {
                    _returnTable[gameId][receiverId].Add(new PayerInfo
                    {
                        PayerId = ReturnTributeList[i],
                        IsFinishedReturnTribute = false,
                        ReturnTributeCount = cardsToBeReturnCountList[i]
                    });
                }
            }

        }

        public PayerInfo GetNextPayerTarget(int gameId, int receiverId)
        {
            return _returnTable[gameId][receiverId].FirstOrDefault(x => x.IsFinishedReturnTribute == false);
        }

        public void SetPayerTargetToValid(int gameId, int receiverId, int payerId)
        {
            var a = _returnTable[gameId][receiverId].Find(x => x.PayerId == payerId).IsFinishedReturnTribute = true;
        }

        public GameService(ILogger<GameService> logger, ICardHelper cardHelper, IGameFactory gameFactory)
        {
            _logger = logger;
            _cardHelper = cardHelper;
            _gameFactory = gameFactory;

            _gameCounter = 0;
        }

        /// <summary>
        /// Add a new player to the game
        /// </summary>
        /// <param name="gameId"></param>
        /// <returns>The id of the newly added player</returns>
        public int AddPlayerToGame(int gameId)
        {
            if (!_gameSessions.TryGetValue(gameId, out var gameLockPair))
                throw new ArgumentException("Invalid game id!");

            var (game, lockObject) = gameLockPair;
            lock (lockObject)
            {
                var newPlayerId = game.AddPlayer();
                return newPlayerId;
            }
        }

        public int CreateNewGame(int capacity)
        {
            if (capacity > 5 || capacity < 3)
                throw new Exception($"Room capacity should in range 3-5! You are tring to create a game with {capacity} people");

            // used Interlocked class here for atomicity
            var newGameId = Interlocked.Increment(ref _gameCounter);

            var newGame = _gameFactory.CreateGame(capacity);
            var gameLockPair = (newGame, new object());
            if (!_gameSessions.TryAdd(newGameId, gameLockPair))
                throw new Exception($"Unable to create room # {newGameId}!");

            return newGameId;
        }

        public InitGameReturnValue InitGame(int gameId, int numOfDecks = 1)
        {
            // check if game id is valid
            if (!_gameSessions.TryGetValue(gameId, out var gameLockPair))
            {
                throw new ArgumentException($"{gameId} is not a valid game id!");
            }

            // perform the card change
            var (game, lockObject) = gameLockPair;
            lock (lockObject)
            {
                return game.InitGame();
            }
        }

        public ReturnTributeReturnValue ReturnTribute(int gameId, int payer, int receiver, IEnumerable<Card> card)
        {
            // check if game id is valid
            if (!_gameSessions.TryGetValue(gameId, out var gameLockPair))
            {
                throw new ArgumentException($"{gameId} is not a valid game id!");
            }

            // perform the card change
            var (game, lockObject) = gameLockPair;
            lock (lockObject)
            {
                return game.ReturnTribute(payer, receiver, card);
            }
        }

        public void AceGoPublic(int gameId, int goPublicPlayerId)
        {
            // add lock

            if (!_gameSessions.TryGetValue(gameId, out var gameLockPair))
            {
                throw new ArgumentException($"{gameId} is not a valid game id!");
            }

            var (game, lockObject) = gameLockPair;
            lock (lockObject)
            {
                game.AceGoPublic(goPublicPlayerId);
            }
        }

        public PlayHandReturn PlayHand(int gameId, int playerId, List<Card> UserCard)
        {
            // add lock

            if (!_gameSessions.TryGetValue(gameId, out var gameLockPair))
            {
                throw new ArgumentException($"{gameId} is not a valid game id!");
            }

            var (game, lockObject) = gameLockPair;
            lock (lockObject)
            {
                return game.PlayHand(playerId, UserCard);
            }
        }

        public IEnumerable<bool> IsBlackAceList(int gameId)
        {
            // assume gameId is always valid since it's called after ReturnTribute succeeded
            if (!_gameSessions.TryGetValue(gameId, out var gameLockPair))
            {
                throw new Exception($"Failed to get playerTypes for game {gameId}");
            }

            var game = gameLockPair.Item1;
            return game.IsBlackAceList();

        }

        public int GetGameCapacity(int gameId)
        {
            if (!_gameSessions.TryGetValue(gameId, out var gameLockPair))
            {
                throw new Exception($"Failed to get capacity for game {gameId}");
            }

            var game = gameLockPair.Item1;
            return game.Capacity;
        }

        public int GetCurrentPlayerId(int gameId)
        {
            if (!_gameSessions.TryGetValue(gameId, out var gameLockPair))
            {
                throw new Exception($"Failed to get playerTypes for game {gameId}");
            }

            var game = gameLockPair.Item1;
            return game.CurrentPlayer;

        }

        public int GetLastValidPlayer(int gameId)
        {
            if (!_gameSessions.TryGetValue(gameId, out var gameLockPair))
            {
                throw new Exception($"Failed to get LastValidPlayer for game {gameId}");
            }

            var game = gameLockPair.Item1;
            return game.LastValidPlayer;

        }


        public IEnumerable<int> GetRemainingPlayerList(int gameId)
        {
            if (!_gameSessions.TryGetValue(gameId, out var gameLockPair))
            {
                throw new Exception($"Failed to get playerTypes for game {gameId}");
            }

            var game = gameLockPair.Item1;
            return game.RemainingPlayers;

        }

        public Dictionary<int, int> GetOpponentCardsCount(int gameId)
        {
            if (!_gameSessions.TryGetValue(gameId, out var gameLockPair))
            {
                throw new Exception($"Failed to get Opponent Cards for game {gameId}");
            }

            var game = gameLockPair.Item1;
            return game.GetOpponentCardsCount();

        }

        public bool DoesGameExist(int gameId)
        {
            return _gameSessions.ContainsKey(gameId);
        }
    }

    public interface IGameService
    {
        InitGameReturnValue InitGame(int gameId, int numOfDecks = 1);
        ReturnTributeReturnValue ReturnTribute(int gameId, int payer, int receiver, IEnumerable<Card> cardIds);
        int AddPlayerToGame(int gameId);
        int CreateNewGame(int capacity);
        void AceGoPublic(int gameId, int goPublicPlayerId);
        PlayHandReturn PlayHand(int gameId, int playerId, List<Card> UserCard);
        IEnumerable<bool> IsBlackAceList(int gameId);
        int GetGameCapacity(int gameId);
        void InitReturnTable(int gameId, Dictionary<int, List<int>> returnTributeListByPlayerId, Dictionary<int, List<int>> cardsToBeReturnCount);
        PayerInfo GetNextPayerTarget(int gameId, int receiverId);
        void SetPayerTargetToValid(int gameId, int receiverId, int payerId);
        IEnumerable<int> GetRemainingPlayerList(int gameId);
        int GetCurrentPlayerId(int gameId);
        int GetLastValidPlayer(int gameId);
        Dictionary<int, int> GetOpponentCardsCount(int gameId);
        bool DoesGameExist(int gameId);
    }


    public class PayerInfo
    {
        public int PayerId { get; set; }
        public bool IsFinishedReturnTribute { get; set; }
        public int ReturnTributeCount { get; set; }
    }
}

