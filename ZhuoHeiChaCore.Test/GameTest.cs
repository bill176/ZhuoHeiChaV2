//using System;
//using Xunit;
//using Moq;
//using ZhuoHeiChaCore;
//using System.Collections.Generic;

//namespace ZhuoHeiChaCore.Test
//{
//    public class GameForTestTest : Game
//    {

//        public GameForTestTest()
//        {
//            _cardFactory = new CardFactory();
//            _cardHelper = new CardHelper(_cardFactory);
//            _gameHelper = new GameHelper();
//            _capacity = 3;
//        }


//        [Fact]// 0->1,2
//        public void ReturnTribute_WhenPlayerIdsAreNotValid_ShouldThrowException()
//        {
//            _tributePairs.Clear();
//            _tributePairs.AddRange(new List<(int, int)> { (0, 1), (0, 2) });

//            var cardList = new List<Card> { _cardFactory.GetCardByCardType(CardType.SPADE_THREE) };

//            try
//            {
//                ReturnTribute(2, 0, cardList);
//                Assert.True(false);
//            }
//            catch (ArgumentException e)
//            {
//                // success
//            }
//            catch (Exception e)
//            {
//                Assert.True(false);
//            }
//        }

//        [Fact] // 0->1,2
//        public void ReturnTribute_WhenPlayerDoesntHaveCards_ShouldThrowException()
//        {
//            _cardsInHandByPlayerId.Clear();
//            _cardsInHandByPlayerId[0] = new List<Card>  { _cardFactory.GetCardByCardType(CardType.SPADE_FOUR) };

//            _tributePairs.Clear();
//            _tributePairs.AddRange(new List<(int, int)> { (0, 1), (0, 2) });

//            var cardList = new List<Card> { _cardFactory.GetCardByCardType(CardType.SPADE_THREE) };

//            try
//            {
//                ReturnTribute(0, 1, cardList);
//                Assert.True(false);
//            }
//            catch (ArgumentException e)
//            {
//                // success
//            }
//            catch (Exception e)
//            {
//                Assert.True(false);
//            }
//        }

//        [Fact]
//        public void ReturnTribute_WhenPlayerIdsAndCardsAreValid_ShouldReturnUpdatedPlayerCards()
//        {
//            _cardsInHandByPlayerId.Clear();
//            _cardsInHandByPlayerId[0] = new List<Card> { _cardFactory.GetCardByCardType(CardType.SPADE_FOUR), _cardFactory.GetCardByCardType(CardType.HEART_FOUR) };
//            _cardsInHandByPlayerId[1] = new List<Card> { };
//            _cardsInHandByPlayerId[2] = new List<Card> { };

//            _tributePairs.Clear();
//            _tributePairs.AddRange(new List<(int, int)> { (0, 1), (0, 2) });
//            _playerTypeList.Clear();
//            _playerTypeList.AddRange(new List<PlayerType> { PlayerType.Ace, PlayerType.Normal, PlayerType.Normal});

//            var cardList = new List<Card> { _cardFactory.GetCardByCardType(CardType.SPADE_FOUR) };
//            ReturnTribute(0, 1, cardList);

//            cardList = new List<Card> { _cardFactory.GetCardByCardType(CardType.HEART_FOUR) };            
//            Console.WriteLine(ReturnTribute(0, 2, cardList));


//        }

//        // extremely informal test, change the init to public then run it.
//        [Fact]
//        public void InitGame_1deck3Players_Should_get_their_random_cards_for_test()
//        {
//            _cardsInHandByPlayerId.Clear();
//            _remainingPlayers.AddRange(new List<int>{ 0,1,2});
//            _tributePairs.Clear();
//            _playerTypeList.Clear();
//            InitGame();
//            _cardsInHandByPlayerId[0] = new List<Card> { _cardFactory.GetCardByCardType(CardType.SPADE_SIX), _cardFactory.GetCardByCardType(CardType.HEART_SIX), _cardFactory.GetCardByCardType(CardType.HEART_JACK) };
//            _cardsInHandByPlayerId[1] = new List<Card> { _cardFactory.GetCardByCardType(CardType.HEART_FIVE), _cardFactory.GetCardByCardType(CardType.SPADE_FIVE), _cardFactory.GetCardByCardType(CardType.SPADE_FOUR), _cardFactory.GetCardByCardType(CardType.SPADE_NINE), _cardFactory.GetCardByCardType(CardType.HEART_NINE), _cardFactory.GetCardByCardType(CardType.SPADE_SEVEN), _cardFactory.GetCardByCardType(CardType.HEART_SEVEN) };
//            _cardsInHandByPlayerId[2] = new List<Card> { _cardFactory.GetCardByCardType(CardType.JOKER_BIG), _cardFactory.GetCardByCardType(CardType.SPADE_ACE), _cardFactory.GetCardByCardType(CardType.CLUBS_EIGHT) };
//            _playerTypeList.Clear();
//            _playerTypeList.AddRange(new List<PlayerType> { PlayerType.Normal, PlayerType.Normal, PlayerType.Ace });
//        }

//        [Fact]
//        public void InitGame_1deck3Players_Should_get_their_random_cards_full_deck()
//        {
//            _cardsInHandByPlayerId.Clear();
//            _remainingPlayers.AddRange(new List<int> { 0, 1, 2 });
//            _tributePairs.Clear();
//            _playerTypeList.Clear();
//            InitGame();
//        }


//        // test 1:
//        // 3 players, _currentPlayer = 0, _lastValidPlayer = 0, _lastValidHand = HandFactory.EMPTY_HAND
//        // the first player can play any hand but not empty
//        [Fact]
//        public void PlayHand_the_first_player_can_play_any_but_not_empty() 
//        {
//            InitGame_1deck3Players_Should_get_their_random_cards_for_test();

//            var userHand = new List<Card> {  };
//            Assert.True(PlayHand(0, userHand).Type == GameReturnType.Resubmit);

//            userHand = new List<Card> { _cardFactory.GetCardByCardType(CardType.SPADE_SIX) };            
//            Assert.True(PlayHand(0, userHand).Type == GameReturnType.PlayHandSuccess);
//        }


//        // test 2:
//        // 3 players, _currentPlayer = 1, _lastValidPlayer = 0, _lastValidHand = {6, 6}
//        // player 1 play {4, 5} should fail

//        [Fact]
//        public void PlayHand_4_5_should_fail()
//        {
//            InitGame_1deck3Players_Should_get_their_random_cards_for_test();

//            var lastHand = HandFactory.CreateHand(new List<Card> { _cardFactory.GetCardByCardType(CardType.SPADE_SIX), _cardFactory.GetCardByCardType(CardType.HEART_SIX) });
//            _currentPlayer = 1;
//            _lastValidPlayer = 0;
//            _lastValidHand = lastHand;

//            var userHand = new List<Card> { _cardFactory.GetCardByCardType(CardType.SPADE_FOUR) , _cardFactory.GetCardByCardType(CardType.SPADE_FIVE) };
//            Assert.True(PlayHand(1, userHand).Type == GameReturnType.Resubmit);

//        }

//        // test 3:
//        // 3 players, _currentPlayer = 1, _lastValidPlayer = 0, _lastValidHand = {6, 6}
//        // player 1 play {5, 5} should fail

//        [Fact]
//        public void PlayHand_5_5_should_fail()
//        {
//            InitGame_1deck3Players_Should_get_their_random_cards_for_test();

//            var lastHand = HandFactory.CreateHand( new List<Card> { _cardFactory.GetCardByCardType(CardType.SPADE_SIX), _cardFactory.GetCardByCardType(CardType.HEART_SIX) });
//            _currentPlayer = 1;
//            _lastValidPlayer = 0;
//            _lastValidHand = lastHand;

//            var userHand = new List<Card> { _cardFactory.GetCardByCardType(CardType.SPADE_FIVE), _cardFactory.GetCardByCardType(CardType.HEART_FIVE) };
//            Assert.True(PlayHand(1, userHand).Type == GameReturnType.Resubmit);

//        }

//        // test 4:
//        // 3 players, _currentPlayer = 1, _lastValidPlayer = 0, _lastValidHand = {6, 6}
//        // player 1 play EMPTYHAND { } should work

//        [Fact]
//        public void PlayHand_empty_should_work()
//        {
//            InitGame_1deck3Players_Should_get_their_random_cards_for_test();

//            var lastHand = HandFactory.CreateHand(new List<Card> { _cardFactory.GetCardByCardType(CardType.SPADE_SIX), _cardFactory.GetCardByCardType(CardType.HEART_SIX) });
//            _currentPlayer = 1;
//            _lastValidPlayer = 0;
//            _lastValidHand = lastHand;

//            var userHand  = new List<Card> { };
//            Assert.True(PlayHand(1, userHand).Type == GameReturnType.PlayHandSuccess);

//        }

//        // test 5:
//        // 3 players, _currentPlayer = 1, _lastValidPlayer = 0, _lastValidHand = {6, 6}
//        // player 1 play EMPTYHAND {9, 9} should work and _lastValidPlayer should equals 1, _currentPlayer should equals 2
//        // _lastValidHand should equals {9, 9}

//        [Fact]
//        public void PlayHand_lastValidHand_should_be_9_9()
//        {
//            InitGame_1deck3Players_Should_get_their_random_cards_for_test();

//            var lastHand = HandFactory.CreateHand(new List<Card> { _cardFactory.GetCardByCardType(CardType.SPADE_SIX), _cardFactory.GetCardByCardType(CardType.HEART_SIX) });
//            _currentPlayer = 1;
//            _lastValidPlayer = 0;
//            _lastValidHand = lastHand;

//            var userHand = new List<Card> { _cardFactory.GetCardByCardType(CardType.SPADE_NINE), _cardFactory.GetCardByCardType(CardType.HEART_NINE) };
//            Assert.True(PlayHand(1, userHand).Type == GameReturnType.PlayHandSuccess);
//            Assert.True(_lastValidPlayer == 1);
//            Assert.True(_currentPlayer == 2);
//        }

//        // test 6:
//        // 3 players, _currentPlayer = 1, _lastValidPlayer = 0, _lastValidHand = {6, 6}
//        // player 1 play {7, 7} and player 1 finished game should work, and _lastValidPlayer should equals 2, _currentPlayer should equals 2
//        // _lastValidHand should equals EMPTYHAND

//        [Fact]
//        public void PlayHand_lastValidHand_should_be_empty()
//        {
//            InitGame_1deck3Players_Should_get_their_random_cards_for_test();

//            var lastHand = HandFactory.CreateHand(new List<Card> { _cardFactory.GetCardByCardType(CardType.SPADE_SIX), _cardFactory.GetCardByCardType(CardType.HEART_SIX) });
//            _currentPlayer = 1;
//            _lastValidPlayer = 0;
//            _lastValidHand = lastHand;
//            _cardsInHandByPlayerId[1] = new List<Card> { _cardFactory.GetCardByCardType(CardType.SPADE_SEVEN), _cardFactory.GetCardByCardType(CardType.HEART_SEVEN) };

//            var userHand = new List<Card> { _cardFactory.GetCardByCardType(CardType.SPADE_SEVEN), _cardFactory.GetCardByCardType(CardType.HEART_SEVEN) };
//            Assert.True(PlayHand(1, userHand).Type == GameReturnType.PlayHandSuccess);
//            Assert.True(_lastValidPlayer == 2);
//            Assert.True(_currentPlayer == 2);
//            Assert.True(_remainingPlayers.Count == 2);
//            Assert.True(_finishOrder.Count == 1);

//        }

//        // test 7:
//        // 3 players, _currentPlayer = 0, player 1 is finished, player 2 is BlackAce, _lastValidPlayer = 2, _lastValidHand = {6, 6}
//        // player 0 play {7, 7} and player 0 finished game should work, and _lastValidPlayer should equals 2, _currentPlayer should equals 2
//        // _lastValidHand should equals EMPTYHAND and game is not ended. finish order: {0, }

//        [Fact]
//        public void PlayHand_player_1_finished_current_player_should_skip_1_and_equals_to_2()
//        {
//            PlayHand_lastValidHand_should_be_empty(); // player 1 has finished.

//            var lastHand = HandFactory.CreateHand(new List<Card> { _cardFactory.GetCardByCardType(CardType.CLUBS_NINE) });
//            _currentPlayer = 0;
//            _lastValidPlayer = 2;
//            _lastValidHand = lastHand;

//            var userHand = new List<Card> { _cardFactory.GetCardByCardType(CardType.HEART_JACK)};
//            Assert.True(PlayHand(0, userHand).Type == GameReturnType.PlayHandSuccess);
//            Assert.True(_lastValidPlayer == 0);
//            Assert.True(_currentPlayer == 2);
//            Assert.True(_remainingPlayers.Count == 2);
//            Assert.True(_finishOrder.Count == 1);

//        }


//        // test 8:
//        // 3 players, _currentPlayer = 1, player 1 is BlackAce, _lastValidPlayer = 0, _lastValidHand = {6, 6}
//        // player 1 play {7, 7} and player 1 finished game should work, and _lastValidPlayer should equals 2, _currentPlayer should equals 2
//        // _lastValidHand should equals EMPTYHAND and game should ended, black ace win. finish order: {1, 0, 2}

//        [Fact]
//        public void PlayHand_lastValidHand_should_be_empty_and_game_ended()
//        {
//            InitGame_1deck3Players_Should_get_their_random_cards_for_test();
//            _playerTypeList.Clear();
//            _playerTypeList.AddRange(new List<PlayerType> { PlayerType.Normal, PlayerType.Ace, PlayerType.Normal });
//            _cardsInHandByPlayerId[1] = new List<Card> { _cardFactory.GetCardByCardType(CardType.SPADE_SEVEN), _cardFactory.GetCardByCardType(CardType.HEART_SEVEN) };

//            var lastHand = HandFactory.CreateHand(new List<Card> { _cardFactory.GetCardByCardType(CardType.SPADE_SIX), _cardFactory.GetCardByCardType(CardType.HEART_SIX) });
//            _currentPlayer = 1;
//            _lastValidPlayer = 0;
//            _lastValidHand = lastHand;

//            var userHand = new List<Card> { _cardFactory.GetCardByCardType(CardType.SPADE_SEVEN), _cardFactory.GetCardByCardType(CardType.HEART_SEVEN) };
//            Assert.True(PlayHand(1, userHand).Type == GameReturnType.GameEnded);
//            Assert.True(_lastValidPlayer == 2);
//            Assert.True(_currentPlayer == 2);
//            Assert.True(_remainingPlayers.Count == 0);
//            Assert.True(_finishOrder.Count == 3);

//        }

//        // test 9:
//        // 3 players, at last game: BlackAceList={0,0,2}, finishOrder = {2,0,1}
//        // After initGame(), blacklist,tributePair should be updated, finishOrder, lastvalidplayer,lastHand,currentplayer should be cleared or equals to 0.

//        [Fact]
//        public void InitGame_After_Initialization_list_should_be_restored()
//        {

//            _cardsInHandByPlayerId.Clear();
//            _remainingPlayers.AddRange(new List<int> { 0, 1, 2 });
//            _tributePairs.Clear();
//            _playerTypeList.Clear();
//            _playerTypeList.AddRange(new List<PlayerType> { PlayerType.Normal, PlayerType.Normal, PlayerType.PublicAce });
//            _finishOrder.AddRange(new List<int> { 2, 0, 1 });
//            InitGame();
//            Console.WriteLine(_playerTypeList);
//            Assert.True(_finishOrder.Count == 0);
//            Assert.True(_lastValidPlayer == 0);
//            Assert.True(_lastValidHand == HandFactory.EMPTY_HAND);
//            Assert.True(_currentPlayer == 0);
//        }


//        // test 10:
//        // 3 players, BlackAceList={0,0,1}
//        // AceGoPublic(3), AceGoPublic(0) should fail; AceGoPublic(2) should work and the playerTypeList should be updated.
//        [Fact]
//        public void AceGoPublic_0_should_fail_2_should_work()
//        {
//            _remainingPlayers.Clear();
//            _remainingPlayers.AddRange(new List<int> { 0, 1, 2 });
//            _playerTypeList.Clear();
//            _playerTypeList.AddRange(new List<PlayerType> { PlayerType.Normal, PlayerType.Normal, PlayerType.Ace });
//            Assert.True(AceGoPublic(0).Type == GameReturnType.Error);
//            Assert.True(AceGoPublic(2).Type == GameReturnType.NoAction);
//            Assert.True(_playerTypeList[2] == PlayerType.PublicAce);
//        }
//    }
//}
