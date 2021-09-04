using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ZhuoHeiChaFunctions
{
    [JsonObject(MemberSerialization.OptIn)]
    public class CounterState
    {
        [JsonProperty("value")]
        public int CurrentValue { get; set; } = 0;

        public Task<int> Add(int amount)
        {
            this.CurrentValue += amount;
            return Task.FromResult(CurrentValue);
        }

        public Task Reset()
        {
            this.CurrentValue = 0;
            return Task.CompletedTask;
        }

        public Task<int> Get() => Task.FromResult(CurrentValue);

        [FunctionName(nameof(CounterState))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx)
            => ctx.DispatchAsync<CounterState>();
    }
}
