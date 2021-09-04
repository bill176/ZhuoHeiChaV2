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

    public override bool CompareValue(Hand lastHand)
    {
        // TODO
        if (Group != lastHand.Group)
            return Group > lastHand.Group;

        var valueToBeCompared = lastHand as FlushCardValue;

        if (valueToBeCompared == null)
            return false;

        if (_flushSize != valueToBeCompared._flushSize)
            return false;
        
        else return _lastNumber > valueToBeCompared._lastNumber;
    }
}