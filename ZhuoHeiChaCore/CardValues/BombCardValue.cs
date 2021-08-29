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

    public override bool CompareValue(Hand otherValue)
    {
        // TODO
        var valueToBeCompared = (BombCardValue)otherValue;

        if (this._length != valueToBeCompared._length)
        {
            return this._length > valueToBeCompared._length;
        }
        else 
        {
            return this._value >= valueToBeCompared._value;
        }
    }
}