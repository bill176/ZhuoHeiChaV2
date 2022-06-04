using System;
using System.Collections.Generic;
using ZhuoHeiChaCore.CardValues;

namespace ZhuoHeiChaCore
{
    public static class HandFactory
    {
        public static Hand EMPTY_HAND = new NoCardValue(new List<Card> { }, 0);
        public static Hand CreateHand(List<Card> cards)
        {
            if (cards.Count == 0) return new NoCardValue(cards, 0);

            cards.Sort((x, y) => x.Number.CompareTo(y.Number));

            // check for single card
            if (cards.Count == 1)
            {
                return new SingleCardValue(cards, 1);
            }

            // check for flush
            var flushLastValue = HandHelper.IsFlush(cards);
            if (flushLastValue > 0)
            {
                //  tell straight Flush
                bool isStraightFlush = true;
                foreach (var card in cards)
                {
                    if (card.Suit != cards[0].Suit)
                    {
                        isStraightFlush = false;
                        break;
                    }
                }

                if (isStraightFlush)
                    return new FlushCardValue(flushLastValue + 13, cards.Count, 1, cards);
                else
                    return new FlushCardValue(flushLastValue, cards.Count, 1, cards);
            }

            // check for pair
            var pairLastValue = HandHelper.IsPair(cards);
            if (pairLastValue > 0)
            {
                return new PairCardValue(pairLastValue, cards.Count / 2, 1, cards);
            }

            // check for bomb
            var bombValue = HandHelper.IsBomb(cards);
            if (bombValue > 0)
            {
                return new BombCardValue(bombValue, cards.Count, cards.Count, cards);
            }

            var catValue = HandHelper.IsCat(cards);
            if (catValue > 0)
            {
                // TODO
                return new CatsCardValue(catValue, catValue, cards);
            }

            throw new Exception("Not a valid hand");
        }
    }
}
