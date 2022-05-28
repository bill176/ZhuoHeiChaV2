using System;
using System.Collections.Generic;
using System.Text;

namespace ZhuoHeiChaCore.ReturnTypeAndValue
{
    public class InitGameReturnValue
    {
        public Dictionary<int, (IEnumerable<Card>, IEnumerable<Card>)> CardsPairsByPlayerId { get; set; }
        public Dictionary<int, IEnumerable<int>> ReturnTributeListByPlayerId { get; set; }

        public Dictionary<int, IEnumerable<int>> CardsToBeReturnCount { get; set; }

        public IEnumerable<PlayerType> PlayerTypeListThisRound { get; set; }
        public bool IsFirstRound { get; set; }
    }
}
