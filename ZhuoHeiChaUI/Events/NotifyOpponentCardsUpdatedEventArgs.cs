using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZhuoHeiChaUI.Events
{
    public class NotifyOpponentCardsUpdatedEventArgs : EventArgs
    {
        public int PlayerId { get; set; }
        public int UpdatedCardCount { get; set; }
        public List<int> LastHand { get; set; }
    }
}
