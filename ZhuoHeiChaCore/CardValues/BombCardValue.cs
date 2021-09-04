using ZhuoHeiChaCore;

public class BombCardValue : Hand
{
    private int _value;
    private int _length;

    public BombCardValue(int value, int length, float group) : base(null, group)
    {
        _value = value;
        _length = length;
    }

    public override bool CompareValue(Hand lastHand)
    {
        // TODO
        if (Group != lastHand.Group)
            return Group > lastHand.Group;

        var valueToBeCompared = lastHand as BombCardValue;
        if(valueToBeCompared == null)
            return false;

        if (this._length != valueToBeCompared._length)
            return this._length > valueToBeCompared._length;
        
        else 
            return this._value > valueToBeCompared._value;
        
    }
}