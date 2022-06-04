using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZhuoHeiChaUI.Events
{
    public class InitializeGameStateEventArgs : EventArgs
    {
        public List<int> CardBefore { get; set; }
        public List<int> CardAfter { get; set; }
    }
}
