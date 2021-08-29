using System;
using System.Collections.Generic;
using ZhuoHeiChaCore;

namespace ZhuoHeiChaConsole
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Hand hand = null;
            try
            {
                hand = HandFactory.CreateHand(new List<Card> { new Card(CardType.CLUBS_ACE), new Card(CardType.DIAMOND_ACE) });
            } catch (Exception e)
            {
                Console.WriteLine("Exception", e);
            }
           
            if (hand is PairCardValue)
            {
                Console.WriteLine("Yay!");
            }
            else
            {
                Console.WriteLine("Nah");
            }
        }
    }
}
