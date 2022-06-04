using System;
using System.Collections.Generic;
using System.Text;

namespace ZhuoHeiChaCore.ReturnTypeAndValue
{
    public class PlayHandReturn
    {
        public PlayHandReturnType Type;
        public int NextPlayerId = -1;
        public string ErrorMessage = "";
        public List<Card> UpdatedCards = null;
        public bool isBlackAceWin;

        public PlayHandReturn(PlayHandReturnType Type)
        {
            this.Type = Type;
        }
        public PlayHandReturn(PlayHandReturnType Type, string ErrorMessage)
        {
            this.Type = Type;
            this.ErrorMessage = ErrorMessage;
        }
        public PlayHandReturn(PlayHandReturnType Type, bool isBlackAceWin)
        {
            this.Type = Type;
            this.isBlackAceWin = isBlackAceWin;
        }
        public PlayHandReturn(PlayHandReturnType Type, List<Card> UpdatedCards, int nextPlayerId)
        {
            this.Type = Type;
            this.UpdatedCards = UpdatedCards;
            this.NextPlayerId = nextPlayerId;
        }
    }
    public enum PlayHandReturnType
    {
        NoAction,
        Resubmit,
        PlayHandSuccess,
        GameEnded
    }
}
