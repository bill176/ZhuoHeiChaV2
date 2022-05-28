using System;
using System.Collections.Generic;
using System.Text;

namespace ZhuoHeiChaCore.ReturnTypeAndValue
{
    public class ReturnTributeReturnValue
    {
        public Dictionary<int, IEnumerable<Card>> cardsAfterReturnTribute { get; set; }
        public bool returnTributeValid { get; set; }
    }
}
