using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZhuoHeiChaCore
{
    public class Hand
    {
        protected float _group = 1;

        public Hand(List<Card> cards, float group)
        {
            _group = group;
        }

        public virtual bool CompareValue(Hand otherValue)
        {
            return false;
        }
    }
}
