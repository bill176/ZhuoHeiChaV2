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
using ZhuoHeiChaCore;
using ZhuoHeiChaShared;
using ZhuoHeiChaCore.ReturnTypeAndValue;

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
        private readonly ICardHelper _cardHelper;

        public GameController(
            IGameService gameService,
            IHubContext<PlayerHub> hubContext,
            IClientNotificationService clientNotificationService,
            ICardHelper cardHelper,
            ILogger<GameController> logger)
        {
            _gameService = gameService;
            _hubContext = hubContext;
            _clientNotificationService = clientNotificationService;
            _cardHelper = cardHelper;
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
                _clientNotificationService.RegisterClient(gameId, newPlayerId, player);

                if (newPlayerId == _gameService.GetGameCapacity(gameId) - 1)
                {
                    // this means that the room is now full
                    // TODO: replace it with a more robust way

                    var hostPlayerId = 0;
                    _clientNotificationService.NotifyCanStartGame(gameId, hostPlayerId);
                }

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

        /// <summary>
        /// Initial game + send cards info and tribute info to frontend
        /// </summary>
        /// <param name="gameId"></param>
        /// <returns></returns>
        [HttpPost("{gameId:int}/players")]
        public IActionResult InitGame(int gameId)
        {
            try
            {
                // Initial Game
                var initGameReturn = _gameService.InitGame(gameId);
                _logger.LogInformation($"Initial game: {gameId}");

                // send cards info and tribute info to frontend
                var CardsPairsByPlayerId = initGameReturn.CardsPairsByPlayerId;
                var ReturnTributeListByPlayerId = initGameReturn.ReturnTributeListByPlayerId;
                foreach(var playerId in CardsPairsByPlayerId.Keys)
                {
                    var cardBefore = _cardHelper.ConvertCardsToIds(CardsPairsByPlayerId[playerId].Item1);
                    var cardAfter = _cardHelper.ConvertCardsToIds(CardsPairsByPlayerId[playerId].Item2);
                    _clientNotificationService.SendCardsBeforeAndAfterPayTribute(gameId, (cardBefore, cardAfter));

                    _clientNotificationService.SendReturnTributeOrderByPlayerId(gameId, ReturnTributeListByPlayerId[playerId]);
                    // also send last round Ace type list, and this round Ace type list
                }
                _logger.LogInformation("finish sending cards info and tribute info to frontend");

                return Ok(gameId);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error in {nameof(InitGame)}!");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        //localhost:5000/api/game/5/returntribute
        //body:
        //    sourcePlayerId: 1,
        //    targetPlaeyrId: 3,
        //    card_id: ...

        [HttpPost("{gameId:int}/ReturnTribute")]
        public async Task<IActionResult> ReturnTribute(int gameId, int payer, int receiver, IEnumerable<int> card_id) 
        {
            try
            {
                // convert sharedcard(frontend) to Card (backend)
                var cards = _cardHelper.ConvertIdsToCards(card_id);

                var cardsAfteReturnTribute = _gameService.ReturnTribute(gameId, payer, receiver, cards);

                if (cardsAfteReturnTribute.Count != 0)    // the last player has finished pay tribute
                {

                    foreach (var playerId in cardsAfteReturnTribute.Keys)
                        await _clientNotificationService.SendCardUpdate(gameId, playerId, _cardHelper.ConvertCardsToIds(cardsAfteReturnTribute[playerId]));

                    // notify ace go public
                    var playerTypeList = _gameService.GetPlayerTypeList(gameId).ToList();
                    for (var id = 0; id < playerTypeList.Count; ++id)
                        await _clientNotificationService.AskAllAceGoPublic(gameId, id, playerTypeList[id]);
                }

                return Ok();
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e, "Argument Exception");
                return BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        [HttpPost("{gameId:int}/PlayHand")]
        public async Task<IActionResult> PlayHand(int gameId, int playerId, List<Card> UserCard)
        {
            try
            {
                var updatedCardsByPlayer = _gameService.PlayHand(gameId, playerId, UserCard);
                switch (updatedCardsByPlayer.Type)
                {
                    case PlayHandReturnType.Resubmit:
                        // tell player:{playerId} the error message 
                        break;

                    case PlayHandReturnType.PlayHandSuccess:
                        // tell frontend: current player, last valid player, last valid hand
                        // tell frontend change player:{playerId} UI(hide playhand botton), change player:{current player} UI(show playhand button)                        
                        break;

                    case PlayHandReturnType.GameEnded:
                        // tell everyone gameended
                        // call initGame()
                        break;

                }

                return Ok();
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e, "Argument Exception");
                return BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }




    }
}
