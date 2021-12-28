using System;
using Xunit;
using Moq;
using ZhuoHeiChaCore;

namespace ZhuoHeiChaCore.Test
{
    public class GameTest
    {
        [Fact]
        public void SendCards_WhenPlayerIdsAreNotValid_ShouldThrowException()
        {

        }

        [Fact]
        public void SendCards_WhenPlayerDoesntHaveCards_ShouldThrowException()
        {

        }

        [Fact]
        public void SendCards_WhenPlayerIdsAndCardsAreValid_ShouldReturnUpdatedPlayerCards()
        {

        }

        // extremely informal test, change the init to public then run it.
        [Fact]
        public void InitGame_1deck3Players_Should_get_their_random_cards()
        {
            var f = new CardFactory();
            var h = new CardHelper(f);
            //var a = new Game(f, h);
            //a.InitGame();
        }

    }
}
