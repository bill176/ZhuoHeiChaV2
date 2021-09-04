using ZhuoHeiChaCore;

public class PairCardValue : Hand
{
    private int _pairNumber;
    private int _lastNumber;

    public PairCardValue(int number, int pairNumber, float group) : base(null, group)
    {
        _lastNumber = number;
        _pairNumber = pairNumber;
    }

    public override bool CompareValue(Hand lastHand)
    {
        // TODO
        if (Group != lastHand.Group)
            return Group > lastHand.Group;

        var valueToBeCompared = lastHand as PairCardValue;
        if (valueToBeCompared == null)
            return false;
        
        if(_pairNumber != valueToBeCompared._pairNumber)
            return false;

        else return _lastNumber > valueToBeCompared._lastNumber;
    }
}