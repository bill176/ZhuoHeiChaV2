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
        public event EventHandler<NotifyNewPlayerAddedEventArgs> NotifyNewPlayerAdded;
        public event EventHandler<NotifyPlayCardEventArgs> NotifyPlayCard;
        public event EventHandler<NotifyAceGoPublicEventArgs> NotifyAceGoPublic;
        public event EventHandler<NotifyReturnTributeEventArgs> NotifyReturnTributeStart;
        public event EventHandler<NotifyReturnTributeSuccessfulEventArgs> NotifyReturnTributeSuccesful;
        public event EventHandler<NotifyResubmitEventArgs> NotifyResubmit;
        public event EventHandler<NotifyOpponentCardsUpdatedEventArgs> NotifyOpponentCardsUpdated;
        public event EventHandler<NotifyCardsUpdatedEventArgs> NotifyCardsUpdated;
        public event EventHandler<InitializeGameStateEventArgs> InitializeGameState;
        public event EventHandler<NotifyGameEndedEventArgs> NotifyGameEnded;

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

            _connection.On<PlayHandPackage>(ClientHubMethods.NotifyPlayCard,
                (playHandPackage) => NotifyPlayCard?.Invoke(this, new NotifyPlayCardEventArgs
                {
                    CurrentPlayerId = playHandPackage.CurrentPlayer,
                    LastValidPlayer = playHandPackage.LastValidPlayer,
                    LastValidHand = playHandPackage.LastValidHand
                }));

            _connection.On<bool>(ClientHubMethods.AceGoPublic,
                (isPublicAce) => NotifyAceGoPublic?.Invoke(this, new NotifyAceGoPublicEventArgs
                {
                    IsPublicAce = isPublicAce
                }));

            _connection.On<int, int>(ClientHubMethods.NotifyReturnTributeStart,
                (payer, cardsToBeReturnCount) => NotifyReturnTributeStart?.Invoke(this, new NotifyReturnTributeEventArgs
                {
                    Payer = payer,
                    CardsToBeReturnCount = cardsToBeReturnCount
                }));

            _connection.On<List<int>>(ClientHubMethods.NotifyReturnTributeSuccessful,
                (returnedCardIds) => NotifyReturnTributeSuccesful?.Invoke(this, new NotifyReturnTributeSuccessfulEventArgs
                {
                    ReturnedCardIds = returnedCardIds
                }));

            _connection.On(ClientHubMethods.NotifyResubmit,
                () => NotifyResubmit?.Invoke(this, new NotifyResubmitEventArgs()));

            _connection.On(ClientHubMethods.UpdateOpponentCardsCount,
                () => NotifyOpponentCardsUpdated?.Invoke(this, new NotifyOpponentCardsUpdatedEventArgs()));

            _connection.On(ClientHubMethods.CanStartGame,
                () => NotifyCanStartGame?.Invoke(this, null));

            _connection.On<List<int>>(ClientHubMethods.UpdateCards,
                (cards) => NotifyCardsUpdated?.Invoke(this, new NotifyCardsUpdatedEventArgs
                {
                    UpdatedCard = cards
                }));


            _connection.On<InitalGamePackage>(ClientHubMethods.InitializeGameState,
                (initalGamePackage) => InitializeGameState?.Invoke(this, new InitializeGameStateEventArgs
                {
                    CardAfter = initalGamePackage.CardAfter,
                    CardBefore = initalGamePackage.CardBefore

                }));

            _connection.On<List<Player>>(ClientHubMethods.NewPlayerAdded,
                (players) => NotifyNewPlayerAdded?.Invoke(this, new NotifyNewPlayerAddedEventArgs { UpdatedPlayers = players }));

            _connection.On<bool>(ClientHubMethods.NotifyGameEnded,
                (isBlackAceWin) => NotifyGameEnded?.Invoke(this, new NotifyGameEndedEventArgs { IsBlackAceWin = isBlackAceWin }));
        }
    }
}
