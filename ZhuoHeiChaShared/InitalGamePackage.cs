using System;
using System.Collections.Generic;
using System.Text;

namespace ZhuoHeiChaShared
{
    public class InitalGamePackage
    {
        public List<int> CardBefore { get; set; }
        public List<int> CardAfter { get; set; }
        public List<int> TributeList { get; set; }
        public List<int> PlayerTypeListLastRound { get; set; }
        public List<int> PlayerTypeListThisRound { get; set; }

        //public InitalGamePackage(List<int> cardBefore, List<int> cardAfter, List<int> tributeList)
        //{
        //    this.cardBefore = cardBefore;
        //    this.cardAfter = cardAfter;
        //    this.tributeList = tributeList;
        //}
    }
}
