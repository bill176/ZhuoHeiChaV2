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
                (int)CardType - NUM_OF_CARDS_WITHOUT_JOKERS :
                (int)CardType % 13 + 3;

        public override string ToString()
        {
            return CardType.ToString();
        }
    }
}
