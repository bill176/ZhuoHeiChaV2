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

            // selector always return the same type
            var group1 = gameHelper.GroupConsecutivePlayersOfSameType(new List<int> { 1, 2, 3 }, idx => PlayerType.Ace);
            var group2 = gameHelper.GroupConsecutivePlayersOfSameType(new List<int> { 1, 4, 9 }, idx => PlayerType.PublicAce);

            Assert.Single(group1);
            Assert.Single(group2);
        }

        [Fact]
        public void GroupConsecutiveElementsOfSameType_ShouldReturnOneGroupPerElement_WhenNoConsecutiveElementsHaveTheSameType()
        {
            var gameHelper = new GameHelper();

            var valueList = new List<int> { 1, 2, 3 };
            var group1 = gameHelper.GroupConsecutivePlayersOfSameType(valueList, idx => (PlayerType)(idx % 2));
            var group2 = gameHelper.GroupConsecutivePlayersOfSameType(valueList, idx => (PlayerType)idx);

            Assert.Equal(valueList.Count, group1.Count);
            Assert.Equal(valueList.Count, group2.Count);
        }

        // test 1:
        // GetPlayerType(blackAce) should return PlayerType.Ace GetPlayerType(other cards) should return PlayerType.Normal; 

        [Theory]
        [MemberData(nameof(GetTestData))]
        public void GroupConsecutiveElementsOfSameType_ShouldGroupConsecutiveElementsOfSameType_WhenThereAreConsecutiveElementsOfSameType(List<int> valueList, List<List<int>> expectedGroups)
        {
            var gameHelper = new GameHelper();

            var groups = gameHelper.GroupConsecutivePlayersOfSameType(valueList, x=>(PlayerType)valueList[x]);

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
                new object[] {new List<int> { 1,1,2,1 }, new List<List<int>> { new List<int> { 1,1 }, new List<int> { 2 }, new List<int> { 1 } } },
                new object[] {new List<int> { 1,2,1,1 }, new List<List<int>> { new List<int> { 1 }, new List<int> { 2 }, new List<int> { 1,1 } } },
                new object[] {new List<int> { 1,2,2,1 }, new List<List<int>> { new List<int> { 1 }, new List<int> { 2,2 }, new List<int> { 1 } } },
                new object[] {new List<int> { 1,2,2,1,1 }, new List<List<int>> { new List<int> { 1 }, new List<int> { 2,2 }, new List<int> { 1,1 } } },
                new object[] {new List<int> { 1,1,2,2,1,1 }, new List<List<int>> { new List<int> { 1,1 }, new List<int> { 2,2 }, new List<int> { 1,1 } } },
            };
        }
    }
}
