using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZhuoHeiChaUI.Events
{
    public class NotifyAceGoPublicEventArgs : EventArgs
    {
        public bool IsPublicAce { get; set; }
    }
}
