using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ZhuoHeiChaCore
{
    public class GameHelper : IGameHelper
    {
        public PlayerType GetPlayerType(List<Card> cardsOfPlayer)
        {
            if (cardsOfPlayer.Select(x => x.CardType).Contains(CardType.SPADE_ACE))
                return PlayerType.Ace;
            else
                return PlayerType.Normal;
        }

        public IEnumerable<(int payer, int receiver)> GeneratePayerReceiverPairsForConsecutiveGroups(List<List<int>> groups)
        {
            var pairs = new List<(int, int)>();
            for (var i = 0; i < groups.Count - 1; ++i)
            {
                var thisGroup = groups[i];
                var nextGroup = groups[i + 1];

                foreach (var receiving in thisGroup)
                {
                    foreach (var paying in nextGroup)
                    {
                        pairs.Add((paying, receiving));
                    }
                }
            }

            return pairs;
        }

        /// <summary>
        /// A method that finds consecutive elements in a list that have the same "type" 
        /// and group them together. The "type" will be determined by the return value of
        /// the function typeSelector. It takes the index of the element in the list
        /// and returns the type as an int.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="playerTypeSelector"></param>
        /// <returns></returns>
        public List<List<int>> GroupConsecutivePlayersOfSameType(List<int> playerIdsByFinishOrder, List<PlayerType> playerTypesByPlayerId)
        {
            var groupStartIdx = 0;
            var currentIdx = 0;
            var groups = new List<List<int>>();

            while (currentIdx < playerIdsByFinishOrder.Count - 1)
            {
                var currentPlayerId = playerIdsByFinishOrder[currentIdx];
                var nextPlayerId = playerIdsByFinishOrder[currentIdx + 1];
                if (playerTypesByPlayerId[currentPlayerId].IsOfSameParty(playerTypesByPlayerId[nextPlayerId]))
                {
                    currentIdx++;
                }
                else
                {
                    // we want the range to be [start, current]
                    groups.Add(Enumerable.Range(groupStartIdx, currentIdx - groupStartIdx + 1).Select(idx => playerIdsByFinishOrder[idx]).ToList());
                    groupStartIdx = currentIdx + 1;
                    currentIdx = groupStartIdx;
                }
            }
            groups.Add(Enumerable.Range(groupStartIdx, currentIdx - groupStartIdx + 1).Select(idx => playerIdsByFinishOrder[idx]).ToList());

            return groups;
        }

        public bool HasFourTwo(List<Card> cards)
        {
            var cardTypeList = cards.Select(x => x.CardType);
            return cardTypeList.Contains(CardType.SPADE_TWO) && cardTypeList.Contains(CardType.HEART_TWO)
                && cardTypeList.Contains(CardType.DIAMOND_TWO) && cardTypeList.Contains(CardType.CLUBS_TWO);
        }

        public bool HasTwoCats(List<Card> cards)
        {
            var cardTypeList = cards.Select(x => x.CardType);
            return cardTypeList.Contains(CardType.JOKER_SMALL) && cardTypeList.Contains(CardType.JOKER_BIG);
        }
    }

    public interface IGameHelper
    {
        PlayerType GetPlayerType(List<Card> cardsOfPlayer);
        List<List<int>> GroupConsecutivePlayersOfSameType(List<int> playerIdsByFinishOrder, List<PlayerType> playerTypesByPlayerId);
        IEnumerable<(int payer, int receiver)> GeneratePayerReceiverPairsForConsecutiveGroups(List<List<int>> groups);
        bool HasFourTwo(List<Card> cards);
        bool HasTwoCats(List<Card> cards);
    }
}
