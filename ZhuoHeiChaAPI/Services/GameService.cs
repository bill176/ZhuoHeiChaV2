using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using ZhuoHeiChaCore;
using ZhuoHeiChaCore.Factories;

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
            // used Interlocked class here for atomicity
            var newGameId = Interlocked.Increment(ref _gameCounter);

            var newGame = _gameFactory.CreateGame(capacity);
            var gameLockPair = (newGame, new object());
            if (!_gameSessions.TryAdd(newGameId, gameLockPair))
                throw new Exception($"Unable to create room # {newGameId}!");

            return newGameId;
        }

        public bool SendCards(int gameId, int sourcePlayerId, int targetPlayerId, IEnumerable<int> cardIds, out Dictionary<int, IEnumerable<Card>> updatedPlayerCards)
        {
            updatedPlayerCards = null;

            // check if game id is valid
            if (!_gameSessions.TryGetValue(gameId, out var gameLockPair))
            {
                _logger.LogError($"{gameId} is not a valid game id!");
                return false;
            }

            // check if card ids are valid
            var cards = _cardHelper.ConvertIdsToCards(cardIds);
            if (cards == null)
            {
                _logger.LogError($"{string.Join(',', cardIds)} contains invalid cards");
                return false;
            }

            // perform the card change
            var (game, lockObject) = gameLockPair;
            lock (lockObject)
            {
                try
                {
                    updatedPlayerCards = game.ReturnTribute(sourcePlayerId, targetPlayerId, cards);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unable to send cards!");
                    return false;
                }
            }

            return true;
        }
    }

    public interface IGameService
    {
        bool SendCards(int gameId, int sourcePlayerId, int targetPlayerId, IEnumerable<int> cardIds, out Dictionary<int, IEnumerable<Card>> updatedPlayerCards);
        int AddPlayerToGame(int gameId);
        int CreateNewGame(int capacity);
    }
}
