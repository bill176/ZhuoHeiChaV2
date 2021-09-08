using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ZhuoHeiChaFunctions
{
    public static class RoomFunctions
    {
        [FunctionName(nameof(CreateRoomOrchestration))]
        public static async Task CreateRoomOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var args = context.GetInput<CreateRoomArgs>();
            var roomProxy = context.CreateEntityProxy<IRoomState>(args.RoomId);

            await roomProxy.SetCapacity(args.Capacity);
        }

        [FunctionName(nameof(CreateRoom))]
        public static async Task<HttpResponseMessage> CreateRoom(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Room/Create/{capacity:int}")] HttpRequestMessage req,
            int capacity,
            [DurableClient] IDurableOrchestrationClient starter)
        {
            // TODO: sanitize input

            var roomId = Guid.NewGuid().ToString();

            // TODO: optionally add code here to track the number of opened rooms

            string instanceId = await starter.StartNewAsync(
                nameof(CreateRoomOrchestration),
                new CreateRoomArgs { RoomId = roomId, Capacity = capacity });

            return req.CreateResponse(System.Net.HttpStatusCode.OK, roomId);
        }

        /// <summary>
        /// Orchestration function for adding a player to a room.
        /// </summary>
        /// <returns>id of the added player</returns>
        [FunctionName(nameof(JoinRoomOrchestration))]
        public static async Task<int> JoinRoomOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger logger)
        {
            var args = context.GetInput<JoinRoomArgs>();
            // TODO: check the validity of the roomId
            var roomProxy = context.CreateEntityProxy<IRoomState>(args.RoomId);

            var connectedPlayersCount = (await roomProxy.GetConnectedPlayers()).Count;
            var capacity = await roomProxy.GetCapacity();
            if (connectedPlayersCount >= capacity)
            {
                logger.LogError($"Room {args.RoomId} is already full!");
                throw new InvalidOperationException("Room is full");
            }

            var newPlayerId = connectedPlayersCount;
            await roomProxy.AddPlayer(newPlayerId);
            logger.LogInformation($"Player {newPlayerId} has joined the room {args.RoomId}");

            return newPlayerId;
        }

        // this is also where server pushes clients info on how to listen to events
        [FunctionName(nameof(JoinRoomClient))]
        public static async Task<HttpResponseMessage> JoinRoomClient(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Room/{roomId}/Join")] HttpRequestMessage req,
            string roomId,
            [DurableClient] IDurableOrchestrationClient starter,
            [DurableClient] IDurableEntityClient client)
        {
            var instanceId = await starter.StartNewAsync(nameof(JoinRoomOrchestration), null, new JoinRoomArgs { RoomId = roomId });

            // poll for orchestration status
            var status = await starter.GetStatusAsync(instanceId);
            while (status.RuntimeStatus != OrchestrationRuntimeStatus.Completed
                && status.RuntimeStatus != OrchestrationRuntimeStatus.Failed)
            {
                await Task.Delay(500);
                status = await starter.GetStatusAsync(instanceId);
            }
            if (status.RuntimeStatus != OrchestrationRuntimeStatus.Completed)
            {
                // TODO: investigate the correct return code
                // -1 here is the player id
                return req.CreateResponse(System.Net.HttpStatusCode.BadRequest, -1);
            }

            var playerId = (int)status.Output;

            // TODO: also need to return pub sub information
            return req.CreateResponse(System.Net.HttpStatusCode.OK, playerId);
        }
    }

    public class CreateRoomArgs
    {
        public string RoomId { get; set; }
        public int Capacity { get; set; }
    }

    public class JoinRoomArgs
    {
        public string RoomId { get; set; }
    }
}