using ZhuoHeiChaCore;
using System.Collections.Generic;

public class CatsCardValue : Hand
{

    public CatsCardValue(float value, float group, List<Card> cards) : base(cards, group)
    {  }

    public override bool CompareValue(Hand lastHand)
    {
        // TODO
        return Group > lastHand.Group;        
    }
}