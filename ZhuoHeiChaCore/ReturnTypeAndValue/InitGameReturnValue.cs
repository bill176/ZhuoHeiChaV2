using System;
using System.Collections.Generic;
using System.Text;

namespace ZhuoHeiChaCore.ReturnTypeAndValue
{
    public class InitGameReturnValue
    {
        public Dictionary<int, (IEnumerable<Card>, IEnumerable<Card>)> CardsPairsByPlayerId { get; set; }
        public Dictionary<int, IEnumerable<int>> ReturnTributeListByPlayerId { get; set; }

        public Dictionary<int, IEnumerable<int>> cardsToBeReturnCount { get; set; }

        public IEnumerable<PlayerType> PlayerTypeListThisRound { get; set; }
    }
}
