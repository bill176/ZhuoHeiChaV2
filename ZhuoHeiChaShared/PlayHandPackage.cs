using System;
using System.Collections.Generic;
using System.Text;

namespace ZhuoHeiChaShared
{
    public class PlayHandPackage
    {
        public int CurrentPlayer { get; set; }
        public int LastValidPlayer { get; set; }
        public List<int> LastValidHand { get; set; }
    }
}
