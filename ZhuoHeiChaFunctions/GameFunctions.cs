using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace ZhuoHeiChaFunctions
{
    //public static class GameFunctions
    //{
    //    //[FunctionName(nameof(SendTribute))]
    //    //public static async Task SendTribute(
    //    //    // TODO: pass this gameId as roomId + gameId
    //    //    string gameId,
    //    //    [OrchestrationTrigger] IDurableOrchestrationContext context)
    //    //{
    //    //    var entityId = new EntityId(nameof(GameState), gameId);
    //    //    var gameStateProxy = context.CreateEntityProxy<IGameState>(entityId);

    //    //    // check if we have a valid tribute list
    //    //    if (gameStateProxy.PlayerIdsByFinishingOrder.Count == 0) return;

    //    //    // publish a message to the client

    //    //    // wait for their responses
    //    //    await context.WaitForExternalEvent(GameEvents.AllPlayersReady);

    //    //    // clear the tribute list
    //    //    gameStateProxy.ResetPlayerFinishingOrder();
    //    //}

    //    [FunctionName(nameof(DummyOrchestration))]
    //    public static async Task DummyOrchestration(
    //        [OrchestrationTrigger] IDurableOrchestrationContext context)
    //    {

    //    }

    //    // should be triggered by a listener that raises events when all players pressed ready
    //    [FunctionName(nameof(StartGame))]
    //    public static async Task<HttpResponseMessage> StartGame(
    //        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Room/{roomId}/StartGame")] HttpRequestMessage req,
    //        [DurableClient] IDurableOrchestrationClient starter,
    //        string roomId,
    //        ILogger logger)
    //    {
    //        //var instanceId = await starter.StartNewAsync(nameof(SendTribute), null);
    //        var instanceId = await starter.StartNewAsync(nameof(DummyOrchestration), null);

    //        return starter.CreateCheckStatusResponse(req, instanceId);
    //    }

    //    // trigger for game ends, clean up cards, connected players
    //}

    //public static class GameEvents
    //{
    //    public const string AllPlayersReady = nameof(AllPlayersReady);
    //}
}