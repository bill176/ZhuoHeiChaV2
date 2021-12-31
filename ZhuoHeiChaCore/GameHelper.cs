using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ZhuoHeiChaCore
{
    public class GameHelper : IGameHelper
    {
        /// <summary>
        /// A method that finds consecutive elements in a list that have the same "type" 
        /// and group them together. The "type" will be determined by the return value of
        /// the function typeSelector. It takes the index of the element in the list
        /// and returns the type as an int.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="typeSelector"></param>
        /// <returns></returns>
        public List<List<T>> GroupConsecutiveElementsOfSameType<T>(List<T> values, Func<int, int> typeSelector)
        {
            var start = 0;
            var current = 0;
            var groups = new List<List<T>>();

            while (current < values.Count - 1)
            {
                if (typeSelector(current) == typeSelector(current + 1))
                {
                    current++;
                }
                else
                {
                    // we want the range to be [start, current]
                    groups.Add(Enumerable.Range(start, current - start + 1).Select(idx => values[idx]).ToList());
                    start = current + 1;
                    current = start;
                }
            }
            groups.Add(Enumerable.Range(start, current - start + 1).Select(idx => values[idx]).ToList());

            return groups;
        }
    }

    public interface IGameHelper
    {
        List<List<T>> GroupConsecutiveElementsOfSameType<T>(List<T> values, Func<int, int> typeSelector);
    }
}
