using ZhuoHeiChaCore;

public class CatsCardValue : Hand
{

    public CatsCardValue(float value, float group) : base(null, group)
    {  }

    public override bool CompareValue(Hand lastHand)
    {
        // TODO
        return Group > lastHand.Group;        
    }
}