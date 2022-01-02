using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using ZhuoHeiChaShared;

namespace ZhuoHeiChaAPI.Hubs
{
    public class PlayerHub : Hub
    {
        /// <summary>
        /// This method is invoked whenever a new client is connected to SignalR
        /// </summary>
        /// <returns></returns>
        public override async Task OnConnectedAsync()
        {
            // notify the connected client of its connection id
            await Clients.Caller.SendAsync(ClientHubMethods.ReceiveConnectionId, Context.ConnectionId);

            // for testing only!
            await Clients.Caller.SendAsync(ClientHubMethods.ReceiveMessage, "hello world!");
            await base.OnConnectedAsync();
        }

        // for testing only!!!
        public async Task NotifyPlayCard()
        {
            await Clients.Caller.SendAsync(ClientHubMethods.PlayCard);
        }
    }
}