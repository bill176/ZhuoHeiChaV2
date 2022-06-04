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
        public static readonly string NotifyPlayCard = nameof(NotifyPlayCard);
        public static readonly string AceGoPublic = nameof(AceGoPublic);
        public static readonly string NotifyResubmit = nameof(NotifyResubmit);
        public static readonly string UpdateCards = nameof(UpdateCards);
        public static readonly string UpdateOpponentCardsCount = nameof(UpdateOpponentCardsCount);
        public static readonly string InitializeGameState = nameof(InitializeGameState);
        public static readonly string NewPlayerAdded = nameof(NewPlayerAdded);
        public static readonly string NotifyReturnTribute = nameof(NotifyReturnTribute);
        public static readonly string NotifyGameEnded = nameof(NotifyGameEnded);
        
    }
}
