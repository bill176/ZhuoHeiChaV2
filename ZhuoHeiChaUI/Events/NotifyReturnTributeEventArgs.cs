using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZhuoHeiChaUI.Events
{
    public class NotifyReturnTributeEventArgs : EventArgs
    {
        public int Payer { get; set; }
        public int CardsToBeReturnCount { get; set; }
    }
}
