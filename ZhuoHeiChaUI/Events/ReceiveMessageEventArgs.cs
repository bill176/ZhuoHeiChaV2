using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZhuoHeiChaUI.Events
{
    public class ReceiveMessageEventArgs : EventArgs
    {
        public string Message { get; set; }
    }
}
