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

        //List<Card> UserCard = new List<Card> { };
        //UserCard.Add(new Card(CardType.CLUBS_THREE));
        //UserCard.Add(new Card(CardType.SPADE_FOUR));
        //UserCard.Add(new Card(CardType.HEART_FIVE));

        // test 1:
        // 3 players, _currentPlayer = 0, _lastValidPlayer = 0, _lastValidHand = HandFactory.EMPTY_HAND
        // the first player can play any hand


        // test 2:
        // 3 players, _currentPlayer = 1, _lastValidPlayer = 0, _lastValidHand = {6, 6}
        // player 1 play {4, 5} should fail

        // test 3:
        // 3 players, _currentPlayer = 1, _lastValidPlayer = 0, _lastValidHand = {6, 6}
        // player 1 play {5, 5} should fail

        // test 4:
        // 3 players, _currentPlayer = 1, _lastValidPlayer = 0, _lastValidHand = {6, 6}
        // player 1 play EMPTYHAND { } should work

        // test 5:
        // 3 players, _currentPlayer = 1, _lastValidPlayer = 0, _lastValidHand = {6, 6}
        // player 1 play EMPTYHAND {9, 9} should work and _lastValidPlayer should equals 1, _currentPlayer should equals 2
        // _lastValidHand should equals {9, 9}

        // test 6:
        // 3 players, _currentPlayer = 1, _lastValidPlayer = 0, _lastValidHand = {6, 6}
        // player 1 play {7, 7} and player 1 finished game should work, and _lastValidPlayer should equals 2, _currentPlayer should equals 2
        // _lastValidHand should equals EMPTYHAND

        // test 7:
        // 3 players, _currentPlayer = 1, player 0 is BlackAce, _lastValidPlayer = 0, _lastValidHand = {6, 6}
        // player 1 play {7, 7} and player 1 finished game should work, and _lastValidPlayer should equals 2, _currentPlayer should equals 2
        // _lastValidHand should equals EMPTYHAND and game is not ended. finish order: {1, }

        // test 8:
        // 3 players, _currentPlayer = 1, player 1 is BlackAce, _lastValidPlayer = 0, _lastValidHand = {6, 6}
        // player 1 play {7, 7} and player 1 finished game should work, and _lastValidPlayer should equals 2, _currentPlayer should equals 2
        // _lastValidHand should equals EMPTYHAND and game should ended, black ace win. finish order: {1, 0, 2}

    }
}
