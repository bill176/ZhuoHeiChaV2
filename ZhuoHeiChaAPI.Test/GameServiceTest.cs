using System;
using Xunit;
using Moq;
using ZhuoHeiChaAPI.Services;
using ZhuoHeiChaCore;
using ZhuoHeiChaCore.Factories;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace ZhuoHeiChaCore.Test
{
    public class GameServiceTest
    {
        //public GameServiceForTest()
        //{
        //    _logger = new logger;
        //    _cardHelper = new cardHelper;
        //    _gameFactory = new gameFactory;

        //    _gameCounter = 0;
        //}

        private GameService GetGameService()
        {
            var logger = Mock.Of<ILogger<GameService>>();
            var cardHelper = Mock.Of<ICardHelper>();
            var gameFactory = Mock.Of<IGameFactory>();

            var gameService = new GameService(logger, cardHelper, gameFactory);
            return gameService;
        }

        [Fact]
        public void CreateNewGame_should_work()
        {
            int capacity = 3;
            var gameservice = GetGameService();
            int gameid = gameservice.CreateNewGame(capacity);
        }
        [Fact]
        public void CreateNewGame_with_6_people_should_fail()
        {
            int capacity = 6;
            try
            {
                var gameservice = GetGameService();
                int gameid = gameservice.CreateNewGame(capacity);
                Assert.True(false);
            }
            catch (Exception e)
            {
                // success
            }
            
        }
    }
}
