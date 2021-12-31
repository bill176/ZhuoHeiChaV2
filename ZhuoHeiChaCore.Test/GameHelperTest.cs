using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ZhuoHeiChaCore.Test
{
    public class GameHelperTest
    {
        [Fact]
        public void GroupConsecutiveElementsOfSameType_ShouldReturnOneGroup_WhenAllElementsAreOfSameType()
        {
            var gameHelper = new GameHelper();

            // selector always return the same type: 1
            var group1 = gameHelper.GroupConsecutiveElementsOfSameType(new List<int> { 1, 2, 3 }, idx => 1);
            var group2 = gameHelper.GroupConsecutiveElementsOfSameType(new List<int> { 1, 4, 9 }, idx => 2);

            Assert.Single(group1);
            Assert.Single(group2);
        }
    }
}
