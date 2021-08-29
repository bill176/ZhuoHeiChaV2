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

    public override bool CompareValue(Hand otherValue)
    {
        // TODO
        var a = (PairCardValue)otherValue;
        if(_pairNumber != a._pairNumber)
            return true;
        else return _lastNumber >= a._lastNumber;
    }
}