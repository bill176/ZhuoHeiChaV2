using System;
using System.Collections.Generic;
using System.Text;

namespace ZhuoHeiChaCore
{
    public class GameActionResult
    {
        public GameReturnType Type;
        public List<Card> UpdatedCards = null;
        public string ErrorMessage = "";
        public int NextPlayerId = -1;

        public GameActionResult()
        {
            this.Type = GameReturnType.NoAction;
        }

        public GameActionResult(GameReturnType Type)
        {
            this.Type = Type;
        }

        public GameActionResult(GameReturnType Type, List<Card> UpdatedCards, int nextPlayerId)
        {
            this.Type = Type;
            this.UpdatedCards = UpdatedCards;
            NextPlayerId = nextPlayerId;
        }

        public GameActionResult(GameReturnType Type, string ErrorMessage)
        {
            this.Type = Type;
            this.ErrorMessage = ErrorMessage;
        }
    }

    public enum GameReturnType
    {
        NoAction,
        Resubmit,
        PlayHandSuccess,
        GameEnded,
        Error
    }
}
