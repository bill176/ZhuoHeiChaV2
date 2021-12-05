using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ZhuoHeiChaFunctions.Entities
{
    public interface IRoomState
    {
        Task<int> GetCapacity();
        Task SetCapacity(int numOfPlayers);

        Task<IReadOnlyList<int>> GetConnectedPlayers();
        Task AddPlayer(int playerId);
        Task<string> GetRoomData();
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class RoomState : IRoomState
    {
        private static readonly int CapacityMin = 3;
        private static readonly int CapacityMax = 5;

        [JsonProperty(nameof(_capacity))]
        private int _capacity = 0;

        [JsonProperty(nameof(_connectedPlayers))]
        private readonly List<int> _connectedPlayers = new List<int>();

        public Task<int> GetCapacity() => Task.FromResult(_capacity);

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

        public Task SetCapacity(int capacity)
        {
            if (!ValidateCapacity(capacity))
            {
                return Task.FromException(new ArgumentOutOfRangeException());
            }

            _capacity = capacity;
            return Task.CompletedTask;
        }

        public Task<string> GetRoomData()
        {
            return Task.FromResult(JsonConvert.SerializeObject(this));
        }

        private bool ValidateCapacity(int capacity)
        {
            return capacity >= CapacityMin && capacity <= CapacityMax;
        }

        [FunctionName(nameof(RoomState))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx)
            => ctx.DispatchAsync<RoomState>();
    }
}
