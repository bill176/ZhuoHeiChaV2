using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZhuoHeiChaCore
{
    public class Hand
    {
        public virtual float Group { get; protected set; } = 1;
        public List<Card> ListOfCards = new List<Card>();

        public Hand(List<Card> cards, float group)
        {
            Group = group;
            ListOfCards = cards;
        }

        public virtual bool CompareValue(Hand otherValue)
        {
            return false;
        }
    }
}
