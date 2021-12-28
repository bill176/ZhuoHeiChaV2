using System;
using System.Collections.Generic;
using System.Text;

namespace ZhuoHeiChaCore.Factories
{
    public class GameFactory : IGameFactory
    {
        private readonly ICardFactory _cardFactory;
        private readonly ICardHelper _cardHelper;

        public GameFactory(ICardFactory cardFactory, ICardHelper cardHelper)
        {
            _cardFactory = cardFactory;
            _cardHelper = cardHelper;
        }

        public IGame CreateGame(int capacity)
        {
            return new Game(_cardFactory, _cardHelper, capacity);
        }
    }

    public interface IGameFactory
    {
        IGame CreateGame(int capacity);
    }
}
