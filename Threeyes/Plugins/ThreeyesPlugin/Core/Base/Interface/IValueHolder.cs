namespace Threeyes.Core
{
    public interface IValueHolder<TParam>
    {
        TParam CurValue { get; set; }
    }
}