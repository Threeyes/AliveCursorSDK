using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Threeyes.Action
{
    /// <summary>
    /// Modify ActionConfigBase's EndValue
    /// 
    /// PS:
    /// -之所以需要该特殊接口，是因为Scale的类型各异，需要由Action进行针对性处理。如果是Duration等固定值就直接直接继承IActionModifier_Config并修改
    /// 
    ///【ToUpdate】:
    /// -每个 Config，标记支持的Modifier类型（该接口也需要显示自身的类型）
    /// -或者是Config有一个DefaultSettings，用于处理Config与Modifier的关系，用户有需要可以通过代码获取并重载，类似 JsonConvert.DefaultSettings
    /// </summary>
    public interface IActionModifier_EndValue : IActionModifier
    {
        object GetEndValueScale(ActionState actionState);
    }

    /// <summary>
    /// Modify ActionConfigBase's endValue.
    /// </summary>
    /// <typeparam name="TParam"></typeparam>
    public abstract class ActionModifier_EndValueBase<TActionConfigModifierData, TParam> : ActionModifierBase<TActionConfigModifierData>, IActionModifier_EndValue
        where TActionConfigModifierData : ActionConfigModifierDataBase<TParam>, new()
    {
        public object GetEndValueScale(ActionState actionState)
        {
            var config = GetConfig(actionState);
            if (config != null)
                return config.ObjEndValueScale;
            return null;
        }
    }
}