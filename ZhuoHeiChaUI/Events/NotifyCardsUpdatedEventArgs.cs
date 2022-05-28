using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZhuoHeiChaUI.Events
{
    public class NotifyCardsUpdatedEventArgs : EventArgs
    {
        public List<int> UpdatedCard { get; set; }
    }
}
