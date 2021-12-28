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
        private readonly ConcurrentDictionary<string, string> _connectionIdsByClientId = new ConcurrentDictionary<string, string>();

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

        public void RegisterClient(int gameId, int playerId, string connectionId)
        {
            var clientId = GetClientId(gameId, playerId);
            _connectionIdsByClientId[clientId] = connectionId;

            _hubContext.Groups.AddToGroupAsync(connectionId, gameId.ToString());

            // No need to wait here; ignore the warning
            SendMessageToAll(gameId, $"Player {playerId} of game {gameId} just joined!");
        }

        public Task SendCardUpdate(int gameId, int playerId, List<Card> newCards)
        {
            throw new NotImplementedException();
        }

        public async Task SendMessageToAll(int gameId, string message)
        {
            await _hubContext.Clients.Group(gameId.ToString()).SendAsync(ClientHubMethods.ReceiveMessage, message);
        }

        private string GetClientId(int gameId, int playerId)
        {
            return gameId.ToString() + ":" + playerId.ToString();
        }
    }

    public interface IClientNotificationService
    {
        void RegisterClient(int gameId, int playerId, string connectionId);
        Task SendCardUpdate(int gameId, int playerId, List<Card> newCards);
        Task NotifyReturnTribute(int gameId, int playerId);
        Task NotifyPlayCards(int gameId, int playerId);
        Task SendMessageToAll(int gameId, string message);
        
        // TODO: complete this list for things such as NotifyAceGoPublic, NotifyCardsBeforeTribute, etc.
    }
}
