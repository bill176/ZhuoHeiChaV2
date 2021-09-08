using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ZhuoHeiChaFunctions
{
    //public interface IGameState
    //{
    //    int NumOfPlayers { get; }
    //    IReadOnlyList<int> PlayerIdsByFinishingOrder { get; }

    //    Task SetNumOfPlayers(int numOfPlayers);
    //    Task AddFinishedPlayer(int playerId);
    //    void ResetPlayerFinishingOrder();
    //}


    //[JsonObject(MemberSerialization.OptIn)]
    //public class GameState : IGameState
    //{
    //    [JsonProperty(nameof(NumOfPlayers))]
    //    public int NumOfPlayers { get; private set; } = 0;

    //    [JsonProperty(nameof(PlayerIdsByFinishingOrder))]
    //    private List<int> _playerIdsByFinishingOrder = new List<int>();

    //    public IReadOnlyList<int> PlayerIdsByFinishingOrder => _playerIdsByFinishingOrder;

    //    public Task SetNumOfPlayers(int numOfPlayers)
    //    {
    //        NumOfPlayers = numOfPlayers;
    //        return Task.CompletedTask;
    //    }
        
    //    public Task AddFinishedPlayer(int playerId)
    //    {
    //        if ((_playerIdsByFinishingOrder.Count < NumOfPlayers)
    //            && (!_playerIdsByFinishingOrder.Contains(playerId)))
    //        {
    //            _playerIdsByFinishingOrder.Add(playerId);
    //        }

    //        return Task.CompletedTask;
    //    }

    //    public void ResetPlayerFinishingOrder()
    //    {
    //        _playerIdsByFinishingOrder.Clear();
    //    }

        
    //    [FunctionName(nameof(GameState))]
    //    public static Task Run([EntityTrigger] IDurableEntityContext ctx)
    //        => ctx.DispatchAsync<GameState>();

    //}
}
