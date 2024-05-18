using UnityEngine.UI;

namespace Threeyes.Action
{

    public abstract class SOAction_TweenGraphicBase<TActionConfig_Tween, TParam> : SOAction_TweenBase<TActionConfig_Tween, TParam, Graphic>
    where TActionConfig_Tween : ActionConfig_TweenBase<TParam>
    {
    }
}