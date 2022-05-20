using System;
using System.Collections.Generic;
using System.Text;

namespace ZhuoHeiChaShared
{
    public static class ClientHubMethods
    {
        public static readonly string ReceiveConnectionId = nameof(ReceiveConnectionId);
        public static readonly string ReceiveMessage = nameof(ReceiveMessage);
        public static readonly string CanStartGame = nameof(CanStartGame);
        public static readonly string PlayCard = nameof(PlayCard);
        public static readonly string AceGoPublic = nameof(AceGoPublic);
        public static readonly string AskAceGoPublic = nameof(AskAceGoPublic);
        public static readonly string PlayAnotherRound = nameof(PlayAnotherRound);
        public static readonly string ReturnTribute = nameof(ReturnTribute);
        public static readonly string PlayHandSuccess = nameof(PlayHandSuccess);
        public static readonly string UpdateOpponentCard = nameof(UpdateOpponentCard);
    }
}
