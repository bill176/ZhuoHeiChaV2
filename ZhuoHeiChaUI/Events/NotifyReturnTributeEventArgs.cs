using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZhuoHeiChaUI.Events
{
    public class NotifyReturnTributeEventArgs : EventArgs
    {
        public int payer;
        public int cardsToBeReturnCount;
    }
}
