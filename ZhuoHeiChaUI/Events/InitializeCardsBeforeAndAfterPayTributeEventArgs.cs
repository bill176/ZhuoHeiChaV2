using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZhuoHeiChaUI.Events
{
    public class InitializeCardsBeforeAndAfterPayTributeEventArgs : EventArgs
    {
        public IEnumerable<int> CardsBeforeTribute { get; set; }
        public IEnumerable<int> CardsAfterTribute { get; set; }
    }
}
