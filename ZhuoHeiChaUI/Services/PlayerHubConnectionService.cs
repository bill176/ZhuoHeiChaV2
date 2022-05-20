using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZhuoHeiChaUI.Events;
using ZhuoHeiChaShared;
using Microsoft.AspNetCore.SignalR.Client;

namespace ZhuoHeiChaUI.Services
{
    public class PlayerHubConnectionService
    {
        private HubConnection _connection;
        public string ConnectionId { get; private set; }

        public event EventHandler<ReceiveMessageEventArgs> ReceiveMessage;
        public event EventHandler NotifyCanStartGame;
        public event EventHandler<NotifyPlayCardEventArgs> NotifyPlayCard;
        public event EventHandler<NotifyAceGoPublicEventArgs> NotifyAceGoPublic;
        public event EventHandler<NotifyAskAceGoPublicEventArgs> NotifyAskAceGoPublic;
        public event EventHandler<NotifyPlayAnotherRoundEventArgs> NotifyPlayAnotherRound;
        public event EventHandler<NotifyReturnTributeEventArgs> NotifyReturnTribute;
        public event EventHandler<NotifyPlayHandSuccessEventArgs> NotifyPlayHandSuccess;
        public event EventHandler<NotifyOpponentCardsUpdatedEventArgs> NotifyOpponentCardsUpdated;
        public event EventHandler<InitializeGameStateEventArgs> InitializeGameState;

        /// <summary>
        /// Sets up and starts the connection to playerhub
        /// </summary>
        /// <returns>The connection id</returns>
        public async Task<string> EstablishConnection()
        {
            _connection = new HubConnectionBuilder().WithUrl("https://localhost:7001/playerhub").Build();

            RegisterEventListeners();

            // get ConnectionId
            var tcs = new TaskCompletionSource<string>();
            var task = tcs.Task;
            _connection.On<string>(ClientHubMethods.ReceiveConnectionId, connectionId =>
            {
                tcs.SetResult(connectionId);
            });

            await _connection.StartAsync();

            // wait for Hub to send ReceiveConnectionId notification
            ConnectionId = await task;
            return ConnectionId;
        }

        // for testing only!
        public async Task RequestPlayCardNotification()
        {
            await _connection.InvokeAsync("NotifyPlayCard");
        }

        private void RegisterEventListeners()
        {
            _connection.On<string>(ClientHubMethods.ReceiveMessage,
                (message) => ReceiveMessage?.Invoke(this, new ReceiveMessageEventArgs { Message = message }));

            _connection.On(ClientHubMethods.PlayCard,
                () => NotifyPlayCard?.Invoke(this, new NotifyPlayCardEventArgs()));

            _connection.On(ClientHubMethods.AskAceGoPublic,
                () => NotifyAskAceGoPublic?.Invoke(this, new NotifyAskAceGoPublicEventArgs()));

            _connection.On(ClientHubMethods.AceGoPublic,
                () => NotifyAceGoPublic?.Invoke(this, new NotifyAceGoPublicEventArgs()));

            _connection.On(ClientHubMethods.PlayAnotherRound,
                () => NotifyPlayAnotherRound?.Invoke(this, new NotifyPlayAnotherRoundEventArgs()));

            _connection.On(ClientHubMethods.ReturnTribute,
                () => NotifyReturnTribute?.Invoke(this, new NotifyReturnTributeEventArgs()));

            _connection.On(ClientHubMethods.PlayHandSuccess,
                () => NotifyPlayHandSuccess?.Invoke(this, new NotifyPlayHandSuccessEventArgs()));

            _connection.On(ClientHubMethods.UpdateCards,
                () => NotifyOpponentCardsUpdated?.Invoke(this, new NotifyOpponentCardsUpdatedEventArgs()));

            _connection.On(ClientHubMethods.CanStartGame,
                () => NotifyCanStartGame?.Invoke(this, null));

            _connection.On<InitalGamePackage>(ClientHubMethods.InitializeGameState,
                (initalGamePackage) => InitializeGameState?.Invoke(this, new InitializeGameStateEventArgs
                {
                    cardAfter = initalGamePackage.CardAfter,
                    cardBefore = initalGamePackage.CardBefore,
                    tributeList = initalGamePackage.TributeList

                }));
        }
    }
}
