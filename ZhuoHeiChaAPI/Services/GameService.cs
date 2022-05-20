using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using ZhuoHeiChaCore;
using ZhuoHeiChaCore.Factories;
using ZhuoHeiChaCore.ReturnTypeAndValue;

namespace ZhuoHeiChaAPI.Services
{
    public class GameService : IGameService
    {
        private int _gameCounter;
        private readonly ConcurrentDictionary<int, (IGame, object)> _gameSessions = new ConcurrentDictionary<int, (IGame, object)>();

        private readonly ILogger<GameService> _logger;
        private readonly ICardHelper _cardHelper;
        private readonly IGameFactory _gameFactory;

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
            if(capacity>5 || capacity<3)
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

        public Dictionary<int, IEnumerable<Card>> ReturnTribute(int gameId, int sourcePlayerId, int targetPlayerId, IEnumerable<Card> card)
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
                return game.ReturnTribute(sourcePlayerId, targetPlayerId, card);
            }
        }

        public AceGoPublicReturn AceGoPublic(int gameId, int goPublicPlayerId, bool isGoingPublic)
        {
            // add lock

            if (!_gameSessions.TryGetValue(gameId, out var gameLockPair))
            {
                throw new ArgumentException($"{gameId} is not a valid game id!");
            }

            var (game, lockObject) = gameLockPair;
            lock (lockObject)
            {
                return game.AceGoPublic(goPublicPlayerId, isGoingPublic);
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

        public IEnumerable<PlayerType> GetPlayerTypeList(int gameId)
        {
            // assume gameId is always valid since it's called after ReturnTribute succeeded
            if (!_gameSessions.TryGetValue(gameId, out var gameLockPair))
            {
                throw new Exception($"Failed to get playerTypes for game {gameId}");
            }

            var (game, lockObject) = gameLockPair;
            lock (lockObject)
            {
                return game.PlayerTypeList;
            }
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
    }

    public interface IGameService
    {
        InitGameReturnValue InitGame(int gameId, int numOfDecks = 1);
        Dictionary<int, IEnumerable<Card>> ReturnTribute(int gameId, int sourcePlayerId, int targetPlayerId, IEnumerable<Card> cardIds);
        int AddPlayerToGame(int gameId);
        int CreateNewGame(int capacity);
        AceGoPublicReturn AceGoPublic(int gameId, int goPublicPlayerId, bool isGoingPublic);
        PlayHandReturn PlayHand(int gameId, int playerId, List<Card> UserCard);
        IEnumerable<PlayerType> GetPlayerTypeList(int gameId);
        int GetGameCapacity(int gameId);
    }
}
