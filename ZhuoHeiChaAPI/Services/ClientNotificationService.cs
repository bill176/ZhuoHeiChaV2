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

        /// <summary>
        /// A list of Player objects indexed by game id
        /// </summary>
        private readonly ConcurrentDictionary<int, (object, List<Player>)> _playersByGameId = new ConcurrentDictionary<int, (object, List<Player>)>();

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

            _playersByGameId.GetOrAdd(gameId, (new object(), new List<Player>()));
            var (lockObj, players) = _playersByGameId[gameId];
            lock (lockObj)
            {
                players.Add(player);
            }

            _hubContext.Groups.AddToGroupAsync(player.ConnectionId, gameId.ToString());

            // No need to wait here; ignore the warning
            _hubContext.Clients.Group(gameId.ToString()).SendAsync(ClientHubMethods.NewPlayerAdded, players);
        }

        public async Task SendCardUpdate(int gameId, int playerId, List<int> newCards)
        {
            var clientId = GetClientId(gameId, playerId);
            var connectionId = _playersByClientId[clientId].ConnectionId;
            await _hubContext.Clients.Client(connectionId).SendAsync(ClientHubMethods.UpdateCards, newCards);
        }

        public async Task SendMessageToAll(int gameId, string message)
        {
            await _hubContext.Clients.Group(gameId.ToString()).SendAsync(ClientHubMethods.ReceiveMessage, message);
        }

        public async Task SendMessageToClient(int gameId, int playerId, string message)
        {
            var clientId = GetClientId(gameId, playerId);
            var connectionId = _playersByClientId[clientId].ConnectionId;
            await _hubContext.Clients.Client(connectionId).SendAsync(ClientHubMethods.ReceiveMessage, message);
        }

        public async Task NotifyAceGoPublic(int gameId, int playerId, bool isPublicAce)
        {
            var clientId = GetClientId(gameId, playerId);
            var connectionId = _playersByClientId[clientId].ConnectionId;
            await _hubContext.Clients.Client(connectionId).SendAsync(ClientHubMethods.AceGoPublic, isPublicAce);
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

        public async Task SendInitalGamePackage(int gameId, int playerId, InitalGamePackage initalGamePackage)
        {
            var clientId = GetClientId(gameId, playerId);
            var connectionId = _playersByClientId[clientId].ConnectionId;
            await _hubContext.Clients.Client(connectionId).SendAsync(ClientHubMethods.InitializeGameState, initalGamePackage);
        }
        public async Task NotifyReturnTribute(int gameId, int receiver, int payer, int returnTributeCount)
        {
            var clientId = GetClientId(gameId, receiver);
            var connectionId = _playersByClientId[clientId].ConnectionId;
            await _hubContext.Clients.Client(connectionId).SendAsync(ClientHubMethods.NotifyReturnTribute, payer, returnTributeCount);
        }
        public async Task NotifyPlayHand(int gameId, int playerId, PlayHandPackage playHandPackage)
        {
            var clientId = GetClientId(gameId, playerId);
            var connectionId = _playersByClientId[clientId].ConnectionId;
            await _hubContext.Clients.Client(connectionId).SendAsync(ClientHubMethods.NotifyPlayCard, playHandPackage);
        }

        public async Task NotifyResubmit(int gameId, int playerId)
        {
            var clientId = GetClientId(gameId, playerId);
            var connectionId = _playersByClientId[clientId].ConnectionId;
            await _hubContext.Clients.Client(connectionId).SendAsync(ClientHubMethods.NotifyResubmit);
        }
        public async Task NotifyGameEnded(int gameId, bool isBlackAceWin)
        {
            await _hubContext.Clients.Group(gameId.ToString()).SendAsync(ClientHubMethods.NotifyGameEnded, isBlackAceWin);
        }
    }

    public interface IClientNotificationService
    {
        void RegisterClient(int gameId, int playerId, Player player);
        Task SendCardUpdate(int gameId, int playerId, List<int> newCards);

        Task SendInitalGamePackage(int gameId, int playerId, InitalGamePackage initalGamePackage);
        Task NotifyReturnTribute(int gameId, int playerId);
        Task NotifyPlayCards(int gameId, int playerId);
        Task SendMessageToAll(int gameId, string message);
        Task SendMessageToClient(int gameId, int playerId, string message);
        Task NotifyAceGoPublic(int gameId, int playerId, bool isPublicAce);
        Task NotifyCanStartGame(int gameId, int playerId);
        Task NotifyReturnTribute(int gameId, int receiver, int payer, int cardsToBeReturned);
        Task NotifyPlayHand(int gameId, int playerId, PlayHandPackage playHandPackage);
        Task NotifyResubmit(int gameId, int playerId);
        Task NotifyGameEnded(int gameId, bool blackAceWin);

    }
}
