using System;
using System.Collections.Generic;
using System.Text;

namespace ZhuoHeiChaCore.ReturnTypeAndValue
{
    public class AceGoPublicReturn
    {
        public AceGoPublicReturnType Type;
        public int PublicAceId;

        public AceGoPublicReturn(AceGoPublicReturnType Type)
        {
            this.Type = Type;
        }
        public AceGoPublicReturn(AceGoPublicReturnType Type, int PublicAceId)
        {
            this.Type = Type;
            this.PublicAceId = PublicAceId;
        }
    }
    public enum AceGoPublicReturnType
    {
        NoAction,
        PublicAce
    }
}
