using System;
using System.Collections.Generic;
using System.Text;

namespace ZhuoHeiChaCore.ReturnTypeAndValue
{
    public class InitGameReturnValue
    {
        public Dictionary<int, (IEnumerable<Card>, IEnumerable<Card>)> CardsPairsByPlayerId { get; set; }
        public Dictionary<int, List<int>> ReturnTributeListByPlayerId { get; set; }

        public Dictionary<int, List<int>> CardsToBeReturnCount { get; set; }

        public bool IsFirstRound { get; set; }
    }
}
