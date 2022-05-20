using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZhuoHeiChaUI.Events
{
    public class InitializeCardsBeforeAndAfterPayTributeEventArgs : EventArgs
    {
        public List<int> CardsBeforeTribute { get; set; }
        public List<int> CardsAfterTribute { get; set; }
    }
}
