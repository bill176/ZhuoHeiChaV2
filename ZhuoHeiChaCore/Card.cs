using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZhuoHeiChaCore
{
    public class Card
    {
        private const int CARDS_IN_SUIT = 13;
        private const int NUM_OF_CARDS_WITHOUT_JOKERS = 52;

        public CardType CardType { get; }

        public Card(CardType cardType)
        {
            CardType = cardType;
        }

        public Suit Suit => (Suit)((int)CardType / CARDS_IN_SUIT);

        public int Number =>
            (int)CardType >= NUM_OF_CARDS_WITHOUT_JOKERS ?
                (int)CardType:
                (int)CardType % 13 + 3;

        public override string ToString()
        {
            return CardType.ToString();
        }

        public static int Comparator(Card c1, Card c2)
        {
            return c1.Number - c2.Number;
        }

        public static int ReverseComparator(Card c1, Card c2)
        {
            return c2.Number - c1.Number;
        }
    }
}
