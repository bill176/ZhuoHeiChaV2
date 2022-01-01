using System;
using System.Collections.Generic;
using System.Text;

namespace ZhuoHeiChaCore.Factories
{
    public class GameFactory : IGameFactory
    {
        private readonly ICardFactory _cardFactory;
        private readonly ICardHelper _cardHelper;
        private readonly IGameHelper _gameHelper;

        public GameFactory(ICardFactory cardFactory, ICardHelper cardHelper, IGameHelper gameHelper)
        {
            _cardFactory = cardFactory;
            _cardHelper = cardHelper;
            _gameHelper = gameHelper;
        }

        public IGame CreateGame(int capacity)
        {
            return new Game(_cardFactory, _cardHelper, _gameHelper, capacity);
        }
    }

    public interface IGameFactory
    {
        IGame CreateGame(int capacity);
    }
}
