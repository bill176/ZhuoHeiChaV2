using System;
using System.Collections.Generic;
using System.Linq;

namespace ZhuoHeiChaCore
{
    public class CardFactory : ICardFactory
    {
        private readonly Dictionary<CardType, Card> _cardsById;

        public CardFactory()
        {
            _cardsById = new Dictionary<CardType, Card>();

            foreach (CardType cardType in Enum.GetValues(typeof(CardType)))
            {
                _cardsById[cardType] = new Card(cardType);
            }
        }

        public Card GetCardByCardType(CardType type)
        {
            return _cardsById[type];
        }

        public Card GetCardById(int id)
        {
            if (!Enum.IsDefined(typeof(CardType), id)) return null;

            return _cardsById[(CardType)id];
        }

        public IEnumerable<Card> GetFullDeckShuffled(int numOfDecks)
        {
            var oneDeck = _cardsById.Values.AsEnumerable();
            var fullDeck = Enumerable.Repeat(oneDeck, numOfDecks).SelectMany(d => d).ToArray();
            FisherYatesShuffle(fullDeck);

            return fullDeck;
        }

        private void FisherYatesShuffle(Card[] values)
        {
            var rnd = new Random();
            for (var i = 0; i < values.Length - 2; ++i)
            {
                var j = rnd.Next(values.Length - i) + i;
                (values[i], values[j]) = (values[j], values[i]);
            }
        }
    }

    public interface ICardFactory
    {
        Card GetCardByCardType(CardType type);
        Card GetCardById(int id);
        IEnumerable<Card> GetFullDeckShuffled(int numOfDecks);
    }
}
