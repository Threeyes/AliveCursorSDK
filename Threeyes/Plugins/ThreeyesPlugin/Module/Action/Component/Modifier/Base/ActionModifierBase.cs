using System.Collections;
using System.Collections.Generic;
using Threeyes.Core;
using UnityEngine;
#if USE_NaughtyAttributes
using NaughtyAttributes;
#endif
namespace Threeyes.Action
{
    /// <summary>
    /// Base Interface for all Action Modifier
    /// Use this interface to achieve different outcome with the same soAction asset
    /// 
    /// PS:
    /// -接口应使用专用的名称，且只负责单一功能，如IActionModifier_EndValue，子类也是相同的命名规则，如ActionModifier_EndValue_Bool
    /// </summary>
    public interface IActionModifier
    {
        bool IsActiveEnter { get; set; }
        bool IsActiveExit { get; set; }

        bool IsStateActived(ActionState actionState);
    }
    /// <summary>
    /// Modify the fields inside based on the specific type of actionRuntimeData
    /// 
    /// PS:
    /// -可使用is转换config的具体类型，然后修改里面的字段
    /// 
    /// Todo：
    /// -可以作为ModifierSettings的ModifyConfig参数
    /// </summary>
    public interface IActionModifier_Config : IActionModifier
    {
        void ModifyConfig(IActionConfig config, ActionState actionState, object value);
    }

    public abstract class ActionModifierBase : MonoBehaviour, IActionModifier
    {
        public bool IsActiveEnter { get { return isActiveEnter; } set { isActiveEnter = value; } }
        public bool IsActiveExit { get { return isActiveExit; } set { isActiveExit = value; } }
        [SerializeField] protected bool isActiveEnter = true;
        [SerializeField] protected bool isActiveExit = true;

        public bool IsStateActived(ActionState actionState)
        {
            if (actionState.Has(ActionState.Enter) && IsActiveEnter)
                return true;
            if (actionState.Has(ActionState.Exit) && IsActiveExit)
                return true;
            return false;
        }
    }
    public abstract class ActionModifierBase<TActionConfigModifier> : ActionModifierBase
          where TActionConfigModifier : ActionConfigModifierDataBase, new()
    {

#if USE_NaughtyAttributes
        [EnableIf(nameof(IsActiveEnter))]
#endif
        public TActionConfigModifier enterConfigModifier = new TActionConfigModifier();
#if USE_NaughtyAttributes
        [EnableIf(nameof(IsActiveExit))]
#endif
        public TActionConfigModifier exitConfigModifier = new TActionConfigModifier();


        protected TActionConfigModifier GetConfig(ActionState actionState)
        {
            if (actionState.Has(ActionState.Enter))
                return enterConfigModifier;
            else if (actionState.Has(ActionState.Exit))
                return exitConfigModifier;

            Debug.LogError("State not match: " + actionState);
            return null;
        }
    }
}