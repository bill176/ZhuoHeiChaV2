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
        public IActionResult CreateNewGame([FromQuery] int capacity)
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
        [HttpPost("{gameId:int}/init")]
        public IActionResult InitGame(int gameId)
        {
            try
            {
                // Initial Game
                var initGameReturn = _gameService.InitGame(gameId);
                _logger.LogInformation($"Initial game: {gameId}");

                // send cards info and tribute info to frontend
                var cardsPairsByPlayerId = initGameReturn.CardsPairsByPlayerId;
                var returnTributeListByPlayerId = initGameReturn.ReturnTributeListByPlayerId;
                var cardsToBeReturnCount = initGameReturn.CardsToBeReturnCount;

                // send initial data
                foreach (var playerId in cardsPairsByPlayerId.Keys)
                {
                    var cardBefore = _cardHelper.ConvertCardsToIds(cardsPairsByPlayerId[playerId].Item1).ToList();
                    var cardAfter = _cardHelper.ConvertCardsToIds(cardsPairsByPlayerId[playerId].Item2).ToList();
                    var initalGamePackage = new InitalGamePackage { CardBefore = cardBefore, CardAfter = cardAfter};

                    _clientNotificationService.SendInitalGamePackage(gameId, playerId, initalGamePackage);

                }

                if (initGameReturn.IsFirstRound)
                {
                    // If the game is in first round, no need to return tribute, Start AceGoPublic directly
                    StartAceGoPublic(gameId);
                    return Ok(gameId);
                }


                // return tribute
                // initial table value
                _gameService.InitReturnTable(gameId, returnTributeListByPlayerId, cardsToBeReturnCount);

                // first payer start return tribute 
                foreach (var playerId in returnTributeListByPlayerId.Keys)
                {
                    var firstPayerTarget = _gameService.GetNextPayerTarget(gameId, playerId);
                    if (firstPayerTarget != null)
                        _clientNotificationService.NotifyReturnTributeStart(gameId, playerId, firstPayerTarget.PayerId, firstPayerTarget.ReturnTributeCount);

                }

                _logger.LogInformation("finish sending cards info to frontend + return tribute finished");

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
        public async Task<IActionResult> ReturnTribute(int gameId, [FromQuery] int payer, [FromQuery] int receiver, [FromQuery] string cardsToBeReturnedString)
        {
            var returnedCardIds = cardsToBeReturnedString.Split(',').Select(Int32.Parse).ToList();
            try
            {
                // convert sharedcard(frontend) to Card (backend)
                var cards = _cardHelper.ConvertIdsToCards(returnedCardIds);


                var returnTributeReturnValue = _gameService.ReturnTribute(gameId, payer, receiver, cards);
                var cardsAfterReturnTribute = returnTributeReturnValue.cardsAfterReturnTribute;
                var returnTributeValid = returnTributeReturnValue.returnTributeValid;

                // if returned cards are valid, change flag value and begin returning cards to next payer.

                await _clientNotificationService.NotifyReturnTributeSuccessful(gameId, receiver, returnedCardIds);

                _gameService.SetPayerTargetToValid(gameId, receiver, payer);
                var nextPayerTarget = _gameService.GetNextPayerTarget(gameId, receiver);
                if (nextPayerTarget != null)
                {
                    await _clientNotificationService.NotifyReturnTributeStart(gameId, receiver, nextPayerTarget.PayerId, nextPayerTarget.ReturnTributeCount);
                    return Ok();
                }

                if (cardsAfterReturnTribute.Count != 0)    // the last player has finished pay tribute
                {
                    foreach (var playerId in cardsAfterReturnTribute.Keys)
                        await _clientNotificationService.SendCardUpdate(gameId, playerId, _cardHelper.ConvertCardsToIds(cardsAfterReturnTribute[playerId]).ToList());

                    // Start AceGoPublic After return tribute
                    StartAceGoPublic(gameId);
                }

                return Ok();
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.Message);
                return BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        private void StartAceGoPublic(int gameId)
        {
            // notify ace go public
            var isPublicAceList = _gameService.IsBlackAceList(gameId).ToList();
            for (var id = 0; id < isPublicAceList.Count; ++id)
                _clientNotificationService.NotifyAceGoPublic(gameId, id, isPublicAceList[id]);
        }

        /// <summary>
        /// Set player to public ace
        /// </summary>
        [HttpPost("{gameId:int}/acegopublic")]
        public async Task<IActionResult> AceGoPublic(int gameId, [FromQuery] int playerId, [FromQuery] bool IsGoPublic)
        {
            try
            {
                if (IsGoPublic)
                    _gameService.AceGoPublic(gameId, playerId);
                NotifyPlayHand(gameId, new List<int>());
                return Ok(gameId);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.Message);
                return BadRequest();
            }

            catch (Exception e)
            {
                _logger.LogError(e, $"Error in {nameof(AceGoPublic)}!");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        private void NotifyPlayHand(int gameId, List<int> lastValidHand)
        {
            // get current player id
            var remainingPlayerList = _gameService.GetRemainingPlayerList(gameId).ToList();
            var lastValidPlayer = _gameService.GetLastValidPlayer(gameId);
            var currentPlayerId = _gameService.GetCurrentPlayerId(gameId);

            var playHandPackage = new PlayHandPackage
            {
                CurrentPlayer = currentPlayerId,
                LastValidPlayer = lastValidPlayer,
                LastValidHand = lastValidHand
            };
            // notify each remaining players, and twll thwm who is the cureent player
            foreach (var playerId in remainingPlayerList)
            {
                _clientNotificationService.NotifyPlayHand(gameId, playerId, playHandPackage);
            }
        }

        [HttpPost("{gameId:int}/PlayHand")]
        public async Task<IActionResult> PlayHand(int gameId, [FromQuery] int playerId, [FromQuery] string cardsTobePlay)
        {
            try
            {
                IEnumerable<int> card_ids;
                if (cardsTobePlay == null)
                    card_ids = new List<int>();
                else
                    card_ids = cardsTobePlay.Split(',').Select(Int32.Parse);
                var userCards = _cardHelper.ConvertIdsToCards(card_ids).ToList();
                var updatedCardsByPlayer = _gameService.PlayHand(gameId, playerId, userCards);
                switch (updatedCardsByPlayer.Type)
                {
                    case PlayHandReturnType.Resubmit:
                        // tell player:{playerId} the error message 
                        await _clientNotificationService.SendMessageToClient(gameId, playerId, updatedCardsByPlayer.ErrorMessage);
                        await _clientNotificationService.NotifyResubmit(gameId, playerId);
                        break;

                    case PlayHandReturnType.PlayHandSuccess:
                        // tell frontend: current player, last valid player, last valid hand
                        // tell frontend change player:{playerId} UI(hide playhand botton), change player:{current player} UI(show playhand button)
                        var lastValidHand = _cardHelper.ConvertCardsToIds(updatedCardsByPlayer.LastValidHand).ToList();
                        NotifyPlayHand(gameId, lastValidHand);
                        break;

                    case PlayHandReturnType.GameEnded:
                        // tell everyone gameended
                        // TODO: need to know Ace win or not
                        await _clientNotificationService.NotifyGameEnded(gameId, updatedCardsByPlayer.isBlackAceWin);
                        break;

                }

                return Ok();
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e, "Argument Exception");
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        //[HttpPost("{gameId:int}/PlayOneMoreRound")]
        //public async Task<IActionResult> PlayOneMoreRound(int gameId, [FromQuery] int playerId, [FromQuery] bool isPlayOneMoreRound)
        //{

        //}



    }
}
