using ZhuoHeiChaCore;

public class FlushCardValue : Hand
{
    private int _lastNumber;
    private int _flushSize;

    public FlushCardValue(int lastNumber, int flushSize, float group) : base(null, group)
    {
        _lastNumber = lastNumber;
        _flushSize = flushSize;
    }

    public override bool CompareValue(Hand otherValue)
    {
        // TODO
        var a = (FlushCardValue)otherValue;
        if(_flushSize != a._flushSize)
            return true;
        else return _lastNumber >= a._lastNumber;
    }
}