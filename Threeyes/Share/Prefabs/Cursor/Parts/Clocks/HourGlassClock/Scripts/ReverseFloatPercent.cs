using Threeyes.ValueHolder;

public class ReverseFloatPercent : ValueHolder_Float
{
    public override float CurValue
    {
        get
        {
            return base.CurValue;
        }

        set
        {
            curValue = value;
            onValueChanged.Invoke(1 - value);
        }
    }
}
