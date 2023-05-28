using Threeyes.ValueHolder;
namespace Threeyes.Action
{
    public abstract class SOAction_TweenCommonBase<TActionConfig_Tween, TValue> : SOAction_TweenBase<TActionConfig_Tween, TValue, IValueHolder<TValue>>
       where TActionConfig_Tween : ActionConfig_TweenBase<TValue>
    {
    }
}