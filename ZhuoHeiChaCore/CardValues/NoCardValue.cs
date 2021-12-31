using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZhuoHeiChaCore.CardValues
{
    public class NoCardValue : Hand
    {
        public NoCardValue(List<Card> cards, float group) : base(cards, group)
        {
           
        }
        public override bool CompareValue(Hand lastHand)
        {
            if (lastHand.Group == 0)
                return true;
            return false;
        }
    }
}
