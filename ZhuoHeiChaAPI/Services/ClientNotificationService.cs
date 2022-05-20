using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZhuoHeiChaAPI.Hubs;
using ZhuoHeiChaCore;
using ZhuoHeiChaShared;

namespace ZhuoHeiChaAPI.Services
{
    public class ClientNotificationService : IClientNotificationService
    {
        /// <summary>
        /// A list of SignalR connection ids indexed by client id
        /// </summary>
        private readonly ConcurrentDictionary<string, Player> _playersByClientId = new ConcurrentDictionary<string, Player>();

        private readonly IHubContext<PlayerHub> _hubContext;

        public ClientNotificationService(IHubContext<PlayerHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task NotifyPlayCards(int gameId, int playerId)
        {
            throw new NotImplementedException();
        }

        public Task NotifyReturnTribute(int gameId, int playerId)
        {
            throw new NotImplementedException();
        }

        public void RegisterClient(int gameId, int playerId, Player player)
        {
            var clientId = GetClientId(gameId, playerId);
            _playersByClientId[clientId] = player;

            _hubContext.Groups.AddToGroupAsync(player.ConnectionId, gameId.ToString());

            // No need to wait here; ignore the warning
            SendMessageToAll(gameId, $"Player {playerId} of game {gameId} just joined!");
        }

        public async Task SendCardUpdate(int gameId, int playerId, IEnumerable<int> newCards)
        {
            throw new NotImplementedException();
        }

        public async Task SendMessageToAll(int gameId, string message)
        {
            await _hubContext.Clients.Group(gameId.ToString()).SendAsync(ClientHubMethods.ReceiveMessage, message);
        }

        public async Task AskAllAceGoPublic(int gameId, int playerId, PlayerType playerType)
        {
            // TODO:
            throw new NotImplementedException();
        }

        private string GetClientId(int gameId, int playerId)
        {
            return gameId.ToString() + ":" + playerId.ToString();
        }

        public async Task NotifyCanStartGame(int gameId, int playerId)
        {
            var clientId = GetClientId(gameId, playerId);
            var connectionId = _playersByClientId[clientId].ConnectionId;
            await _hubContext.Clients.Client(connectionId).SendAsync(ClientHubMethods.CanStartGame);
        }

        public Task SendCardsBeforeAndAfterPayTribute(int gameId, (IEnumerable<int>, IEnumerable<int>) CardsPairsByPlayerId)
        {
            throw new NotImplementedException();
        }

        public Task SendReturnTributeOrderByPlayerId(int gameId, IEnumerable<int> ReturnTributeListByPlayerId)
        {
            throw new NotImplementedException();
        }
    }

    public interface IClientNotificationService
    {
        void RegisterClient(int gameId, int playerId, Player player);
        Task SendCardUpdate(int gameId, int playerId, IEnumerable<int> newCards);

        Task SendCardsBeforeAndAfterPayTribute(int gameId, (IEnumerable<int>, IEnumerable<int>) CardsPairsByPlayerId);

        Task SendReturnTributeOrderByPlayerId(int gameId, IEnumerable<int> ReturnTributeListByPlayerId);

        Task NotifyReturnTribute(int gameId, int playerId);
        Task NotifyPlayCards(int gameId, int playerId);
        Task SendMessageToAll(int gameId, string message);
        Task AskAllAceGoPublic(int gameId, int playerId, PlayerType playerType);
        Task NotifyCanStartGame(int gameId, int playerId);

        // TODO: complete this list for things such as NotifyAceGoPublic, NotifyCardsBeforeTribute, etc.
    }
}
