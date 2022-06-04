using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZhuoHeiChaUI.Events
{
    public class NotifyPlayCardEventArgs : EventArgs
    {
        public int CurrentPlayerId { get; set; }
        public int LastValidPlayer { get; set; }
        public List<int> LastValidHand { get; set; }
    }
}
