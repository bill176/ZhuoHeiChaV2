using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ZhuoHeiChaFunctions
{
    public interface IRoomState
    {
        Task<int> GetCapacity();
        Task SetCapacity(int numOfPlayers);

        Task<IReadOnlyList<int>> GetConnectedPlayers();
        Task AddPlayer(int playerId);
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class RoomState : IRoomState
    {
        [JsonProperty(nameof(_capacity))]
        private int _capacity = 0;

        [JsonProperty(nameof(_connectedPlayers))]
        private readonly List<int> _connectedPlayers = new List<int>();

        public Task<IReadOnlyList<int>> GetConnectedPlayers() => Task.FromResult((IReadOnlyList<int>)_connectedPlayers);

        public Task AddPlayer(int playerId)
        {
            if ((_connectedPlayers.Count < _capacity)
                && (!_connectedPlayers.Contains(playerId)))
            {
                _connectedPlayers.Add(playerId);
            }

            return Task.CompletedTask;
        }

        public Task<int> GetCapacity() => Task.FromResult(_capacity);

        public Task SetCapacity(int capacity)
        {
            _capacity = capacity;
            return Task.CompletedTask;
        }

        [FunctionName(nameof(RoomState))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx)
            => ctx.DispatchAsync<RoomState>();
    }
}
