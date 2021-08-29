using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZhuoHeiChaCore
{
    public static class HandHelper
    {
        private const int MINIMUM_CARDS_IN_BOMB = 3;
        private const int ACE_CARD_VALUE = 14;
        private const int TWO_CARD_VALUE = 15;

        // return -1 if not a Flush; otherwise, return its value
        // (that is determined by value of the end of the Flush sequence)
        public static int IsFlush(List<Card> cards)
        {
            int cardLength = cards.Count;
            if (cardLength < 3)
                return -1;

            // if the flush is A23, then the order is 3A2, so here i adjust the order.
            // if there are 3 to 2, then change it to A to K does not matter.
            var tempFlush = cards.Select(card => new Card(card.CardType)).ToList();
            if ((tempFlush[0].Number == 3) && (tempFlush[cardLength - 1].Number == 15))
            {
                var two = tempFlush[cardLength - 1];
                var Ace = tempFlush[cardLength - 2];
                tempFlush.RemoveRange(cardLength - 2, 2);
                tempFlush.Insert(0, two);
                tempFlush.Insert(0, Ace);
            }

            else if (tempFlush[cardLength - 1].Number == 15)      // no KA2
                return -1;

            for (int i = 0; i < cardLength - 1; i++)
            {
                if (((tempFlush[i].Number + 1) % 13) != (tempFlush[i + 1].Number % 13))
                    return -1;
            }
            return tempFlush[cardLength - 1].Number;
        }

        // return -1 if not a Pair; otherwise, return its value
        // (that is determined by value of the end of the Pair sequence)
        public static int IsPair(List<Card> cards)
        {
            int cardLength = cards.Count;
            if ((cardLength % 2 == 1) || (cardLength == 4))  // no tractor: 3344
                return -1;

            var tempCards = cards.Select(card => new Card(card.CardType)).ToList();
            if ((tempCards[0].Number == 3) && (tempCards[cardLength - 1].Number == 15))
            {
                var two = tempCards[cardLength - 1];
                var two2 = tempCards[cardLength - 2];
                var Ace = tempCards[cardLength - 3];
                var Ace2 = tempCards[cardLength - 4];
                tempCards.RemoveRange(cardLength - 4, 4);
                tempCards.Insert(0, two);
                tempCards.Insert(0, two2);
                tempCards.Insert(0, Ace);
                tempCards.Insert(0, Ace2);
            }

            else if (tempCards[cardLength - 1].Number == 15 && cardLength != 2)     // no KKAA22
                return -1;

            // check pair
            for (int i = 0; i < cardLength / 2; i++)
            {
                if ((tempCards[2 * i].Number != tempCards[2 * i + 1].Number))
                    return -1;
            }

            // check continuous pair
            for (int i = 0; i < cardLength / 2 - 1; i++)
            {
                if (((tempCards[2 * i].Number + 1)) % 13 != (tempCards[2 * (i + 1)].Number % 13))
                    return -1;
            }
            return tempCards[cardLength - 1].Number;
        }

        public static int IsBomb(List<Card> cards)
        {
            // a bomb must have at least 3 cards
            if (cards.Count() < MINIMUM_CARDS_IN_BOMB)
                return -1;

            var numberSet = new HashSet<int>(cards.Select(c => c.Number));
            if (numberSet.Count > 1)
                return -1;

            return cards.First().Number;
        }

        public static int IsCat(List<Card> cards)
        {
            // TODO
            return -1;
        }
    }
}
