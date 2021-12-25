using System.Collections.Generic;
using System.Linq;

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

            if (cards.Any(x => x == null)) return null;

            return cards;
        }
    }

    public interface ICardHelper
    {
        IEnumerable<Card> ConvertIdsToCards(IEnumerable<int> cardIds);
        string ConvertCardsToString(IEnumerable<Card> cards);
    }
}
