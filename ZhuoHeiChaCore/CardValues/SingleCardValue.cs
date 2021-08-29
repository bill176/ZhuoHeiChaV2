using System.Collections.Generic;
using ZhuoHeiChaCore;

public class SingleCardValue : Hand
{
    private readonly int _cardNumber;

    public SingleCardValue(List<Card> cards, float group) : base(cards, group)
    {
        _cardNumber = cards[0].Number;
    }
    
    public override bool CompareValue(Hand otherValue)
    {
        // TODO
        var a = (SingleCardValue)otherValue;
        return _cardNumber >= a._cardNumber;
    }
}