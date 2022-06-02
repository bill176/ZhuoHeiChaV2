using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.Linq;

namespace ZhuoHeiChaCore.Test
{
    public class GameHelperTest
    {
        [Fact]
        public void GroupConsecutiveElementsOfSameType_ShouldReturnOneGroup_WhenAllElementsAreOfSameType()
        {
            var gameHelper = new GameHelper();

            var allAcePlayerTypes = new List<PlayerType> { PlayerType.Ace, PlayerType.Ace, PlayerType.Ace };
            var allNormalPlayerTypes = new List<PlayerType> { PlayerType.Normal, PlayerType.Normal, PlayerType.Normal};

            // selector always return the same type
            var group1 = gameHelper.GroupConsecutivePlayersOfSameType(new List<int> { 0, 1, 2 }, allAcePlayerTypes);
            var group2 = gameHelper.GroupConsecutivePlayersOfSameType(new List<int> { 1, 0, 2 }, allNormalPlayerTypes);
            var group3 = gameHelper.GroupConsecutivePlayersOfSameType(new List<int> { 2, 1, 0 }, allNormalPlayerTypes);

            Assert.Single(group1);
            Assert.Single(group2);
            Assert.Single(group3);
        }

        [Fact]
        public void GroupConsecutiveElementsOfSameType_ShouldReturnOneGroupPerElement_WhenNoConsecutiveElementsHaveTheSameType()
        {
            var gameHelper = new GameHelper();

            var differentPlayerTypes = new List<PlayerType> { PlayerType.Normal, PlayerType.Ace, PlayerType.Normal };

            var valueList = new List<int> { 0, 1, 2 };
            var group1 = gameHelper.GroupConsecutivePlayersOfSameType(valueList, differentPlayerTypes);

            Assert.Equal(valueList.Count, group1.Count);
        }

        [Fact]
        public void GroupConsecutiveElementOfSameType_ShouldReturnTwoGroups_WhenThereAreBothAceAndPublicAceTogether()
        {
            var gameHelper = new GameHelper();

            var playerTypes = new List<PlayerType> { PlayerType.Normal, PlayerType.Ace, PlayerType.PublicAce };

            var valueList = new List<int> { 0, 1, 2 };
            var group1 = gameHelper.GroupConsecutivePlayersOfSameType(valueList, playerTypes);

            Assert.Equal(2, group1.Count);
        }

        [Fact]
        public void GroupConsecutiveElementsOfSameType_ShouldReturnCurrectGroup_WhenFinishOrderIsRandomlyArranged()
        {
            var gameHelper = new GameHelper();

            var valueList = new List<int> { 0, 2, 1 };
            var typeList = new List<PlayerType> { PlayerType.Normal, PlayerType.Normal, PlayerType.Ace };
            var group1 = gameHelper.GroupConsecutivePlayersOfSameType(valueList, typeList);

            Assert.Equal(valueList.Count, group1.Count);
        }

        // test 1:
        // GetPlayerType(blackAce) should return PlayerType.Ace GetPlayerType(other cards) should return PlayerType.Normal; 

        [Theory]
        [MemberData(nameof(GetTestData))]
        public void GroupConsecutiveElementsOfSameType_ShouldGroupConsecutiveElementsOfSameType_WhenThereAreConsecutiveElementsOfSameType(List<int> valueList, List<PlayerType> playerTypes, List<List<int>> expectedGroups)
        {
            var gameHelper = new GameHelper();

            var groups = gameHelper.GroupConsecutivePlayersOfSameType(valueList, playerTypes);

            Assert.Equal(expectedGroups.Count, groups.Count);
            for (var i = 0; i < expectedGroups.Count; ++i)
            {
                Assert.True(expectedGroups[i].SequenceEqual(groups[i]));
            }
        }

        public static IEnumerable<object[]> GetTestData()
        {
            return new List<object[]>
            {
                new object[]
                {
                    new List<int> { 0, 1, 2, 3 },
                    new List<PlayerType> { PlayerType.Normal, PlayerType.Normal, PlayerType.Ace, PlayerType.Normal },
                    new List<List<int>> { new List<int> { 0 ,1 }, new List<int> { 2 }, new List<int> { 3 } } 
                },
                new object[]
                {
                    new List<int> { 3, 2, 1, 0 },
                    new List<PlayerType> { PlayerType.Normal, PlayerType.Normal, PlayerType.Ace, PlayerType.Normal },
                    new List<List<int>> { new List<int> { 3 }, new List<int> { 2 }, new List<int> { 1, 0 } }
                },
                new object[]
                {
                    new List<int> { 0, 1, 2, 3 }, 
                    new List<PlayerType> { PlayerType.Normal, PlayerType.Ace, PlayerType.Normal, PlayerType.Normal }, 
                    new List<List<int>> { new List<int> { 0 }, new List<int> { 1 }, new List<int> { 2, 3 } } 
                },
                new object[]
                {
                    new List<int> { 0, 1, 2, 3 },
                    new List<PlayerType> { PlayerType.Normal, PlayerType.Ace, PlayerType.PublicAce, PlayerType.Normal },
                    new List<List<int>> { new List<int> { 0 }, new List<int> { 1, 2 }, new List<int> { 3 } } 
                },
                new object[] 
                {
                    new List<int> { 0, 1, 2, 3, 4 },
                    new List<PlayerType> { PlayerType.Normal, PlayerType.Ace, PlayerType.PublicAce, PlayerType.Normal, PlayerType.Normal },
                    new List<List<int>> { new List<int> { 0 }, new List<int> { 1, 2 }, new List<int> { 3, 4 } } 
                },
                new object[]
                {
                    new List<int> { 0, 1, 2, 3, 4, 5 },
                    new List<PlayerType> { PlayerType.Normal, PlayerType.Normal, PlayerType.Ace, PlayerType.PublicAce, PlayerType.Normal, PlayerType.Normal },
                    new List<List<int>> { new List<int> { 0, 1 }, new List<int> { 2, 3 }, new List<int> { 4, 5 } } 
                },
            };
        }
    }
}
