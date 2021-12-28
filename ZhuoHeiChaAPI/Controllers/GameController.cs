using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZhuoHeiChaAPI.Hubs;
using ZhuoHeiChaAPI.Services;
using ZhuoHeiChaShared;

namespace ZhuoHeiChaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly ILogger<GameController> _logger;
        private readonly IGameService _gameService;
        private readonly IHubContext<PlayerHub> _hubContext;
        private readonly IClientNotificationService _clientNotificationService;

        public GameController(
            IGameService gameService,
            IHubContext<PlayerHub> hubContext,
            IClientNotificationService clientNotificationService,
            ILogger<GameController> logger)
        {
            _gameService = gameService;
            _hubContext = hubContext;
            _clientNotificationService = clientNotificationService;
            _logger = logger;
        }

        /// <summary>
        /// Add a player to the game and store its SignalR connection id
        /// </summary>
        /// <param name="gameId"></param>
        /// <returns></returns>
        [HttpPost("{gameId:int}/players")]
        public IActionResult AddPlayerToGame(int gameId, Player player)
        {
            if (player == null)
            {
                _logger.LogError("Cannot add an empty player to game!");
                return BadRequest();
            }

            try
            {
                var newPlayerId = _gameService.AddPlayerToGame(gameId);
                _clientNotificationService.RegisterClient(gameId, newPlayerId, player.ConnectionId);

                return Ok(newPlayerId);
            }
            catch (ArgumentException e)
            {
                e.Data.Add(nameof(gameId), gameId);
                e.Data.Add(nameof(player), player);
                _logger.LogError(e, $"Argument error in {nameof(AddPlayerToGame)}!");
                return BadRequest();
            }
            catch (Exception e)
            {
                e.Data.Add(nameof(gameId), gameId);
                e.Data.Add(nameof(player), player);
                _logger.LogError(e, $"Error in {nameof(AddPlayerToGame)}!");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Creates a new game with the specified capacity
        /// </summary>
        /// <param name="capacity"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CreateNewGame([FromQuery]int capacity)
        {
            if (capacity <= 0)
            {
                _logger.LogError($"{capacity} is not a valid capacity for a game!");
                return BadRequest();
            }

            try
            {
                var newGameId = _gameService.CreateNewGame(capacity);
                _logger.LogInformation($"New room created with id {newGameId} and capacity {capacity}");

                return Ok(newGameId);
            }
            catch (Exception e)
            {
                e.Data.Add(nameof(capacity), capacity);
                _logger.LogError(e, $"Error in {nameof(CreateNewGame)}!");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
