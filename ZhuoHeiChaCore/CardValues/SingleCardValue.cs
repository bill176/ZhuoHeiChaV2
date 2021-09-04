using System.Collections.Generic;
using ZhuoHeiChaCore;

public class SingleCardValue : Hand
{
    private readonly int _cardNumber;

    public SingleCardValue(List<Card> cards, float group) : base(cards, group)
    {
        _cardNumber = cards[0].Number;
    }
    
    public override bool CompareValue(Hand lastHand)
    {
        // TODO
        if (Group != lastHand.Group)
            return Group > lastHand.Group;

        var valueToBeCompared = lastHand as SingleCardValue;
        if (valueToBeCompared == null)
            return false;

        return _cardNumber > valueToBeCompared._cardNumber;
    }
}