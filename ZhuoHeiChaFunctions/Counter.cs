using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace ZhuoHeiChaFunctions
{
    public static class Counter
    {
        [FunctionName("Counter_Add_O")]
        public static async Task<int> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var entityId = new EntityId(nameof(CounterState), "counter1");
            return await context.CallEntityAsync<int>(entityId, "Add", 1);
        }

        [FunctionName("Counter_Add")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("Counter_Add_O", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        [FunctionName("Counter_Get_O")]
        public static async Task<int> GetCounter_O(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var entityId = new EntityId(nameof(CounterState), "counter1");
            return await context.CallEntityAsync<int>(entityId, "Get");
        }


        [FunctionName("Counter_Get")]
        public static async Task<HttpResponseMessage> GetCounter(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("Counter_Get_O", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}