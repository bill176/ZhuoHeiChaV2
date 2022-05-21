using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZhuoHeiChaShared;

namespace ZhuoHeiChaUI.Events
{
    public class NotifyNewPlayerAddedEventArgs : EventArgs
    {
        public List<Player> UpdatedPlayers { get; set; }
    }
}
