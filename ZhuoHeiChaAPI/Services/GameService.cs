using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ZhuoHeiChaCore;

namespace ZhuoHeiChaAPI.Services
{
    public class GameService : IGameService
    {
        private readonly ConcurrentDictionary<int, (IGame, object)> _gameSessions = new ConcurrentDictionary<int, (IGame, object)>();

        private readonly ILogger _logger;
        private readonly ICardHelper _cardHelper;

        public GameService(ILogger logger, ICardHelper cardHelper)
        {
            _logger = logger;
            _cardHelper = cardHelper;
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
                    updatedPlayerCards = game.SendCards(sourcePlayerId, targetPlayerId, cards);
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
    }
}
