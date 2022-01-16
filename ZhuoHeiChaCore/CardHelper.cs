using System.Collections.Generic;
using System.Linq;
using System;

namespace ZhuoHeiChaCore
{
    public class CardHelper : ICardHelper
    {
        private readonly ICardFactory _cardFactory;

        public CardHelper(ICardFactory cardFactory)
        {
            _cardFactory = cardFactory;
        }

        public string ConvertCardsToString(IEnumerable<Card> cards)
        {
            return string.Join(',', cards.Select(c => c.ToString()));
        }

        public IEnumerable<Card> ConvertIdsToCards(IEnumerable<int> cardIds)
        {
            var cards = cardIds.Select(id => _cardFactory.GetCardById(id));

            if (cards.Any(x => x == null))
                throw new ArgumentException($"Cards {string.Join(',', cardIds)} contain an invalid value");

            return cards;
        }

        public IEnumerable<int> ConvertCardsToIds(IEnumerable<Card> cards)
        {
            return cards.Select(c => (int)c.CardType);
        }
    }

    public interface ICardHelper
    {
        IEnumerable<Card> ConvertIdsToCards(IEnumerable<int> cardIds);
        IEnumerable<int> ConvertCardsToIds(IEnumerable<Card> cards);
        string ConvertCardsToString(IEnumerable<Card> cards);
    }
}
