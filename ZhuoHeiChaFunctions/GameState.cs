using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Threading.Tasks;

namespace ZhuoHeiChaFunctions
{
    public class GameState
    {

        [FunctionName(nameof(GameState))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx)
            => ctx.DispatchAsync<GameState>();
    }

    public class PlayerState
    {

    }
}
