using UnityEngine;
using System.Linq;
using Threeyes.Core;
using System.Collections.Generic;
#if USE_NaughtyAttributes
using NaughtyAttributes;
#endif
#if USE_JsonDotNet
using Newtonsoft.Json;
#endif

namespace Threeyes.Action
{
    public interface IActionConfig : ICloneableData
    {
        object BaseEndValue { get; set; }
        void ApplyModifierSettings(ActionState actionState, object value = null, ActionModifierSettings modifierSettings = null);
    }

    /// <summary>
    /// 
    /// PS:
    /// -子类应该统一提供属性，方便外部通过接口访问及修改字段
    /// </summary>
    /// <typeparam name="TValue">Main value to modify, If multiple fields are involved, they should be encapsulated as a data class</typeparam>
#if USE_JsonDotNet
    [JsonObject(MemberSerialization.Fields)]
#endif
    public abstract class ActionConfigBase<TValue> : CloneableDataBase, IActionConfig
    {
        #region Property & Field
        public virtual object BaseEndValue
        {
            get { return EndValue; }
            set
            {
                if (value == null)
                {
                    Debug.LogError("The input value is null!");
                    return;
                }

                try
                {
                    EndValue = (TValue)System.Convert.ChangeType(value, typeof(TValue));//可能会因为格式不同而出错
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Try Convert [{value}] failed with exception:\r\n" + e);
                }
            }
        }

        /// <summary>
        /// Target Value
        /// </summary>
#if USE_JsonDotNet
        [JsonIgnore]
#endif
        public virtual TValue EndValue { get { return endValue; } set { endValue = value; } }//PS:方便Color等自定义重载的字段

#if USE_NaughtyAttributes
        [AllowNesting]
        [ShowIf(nameof(IsShowDefaultEndValue))]
#endif
        [SerializeField] protected TValue endValue;
        #endregion

        #region Public Method
        public virtual void ApplyModifierSettings(ActionState actionState, object value = null, ActionModifierSettings modifierSettings = null)
        {
            if (modifierSettings != null && modifierSettings.CustomModifyConfig != null)//如果用户提供了自定义解析Modifier的方式，则优先使用
            {
                modifierSettings.CustomModifyConfig.Invoke(this, actionState, value);
            }
            else//否则使用默认的配置
            {
                DefaultModifyConfig(actionState, value, modifierSettings);
            }
        }

        /// <summary>
        /// Default way to deal with modifiers
        /// 
        /// PS:已经确保传入的modifierSettings.CustomModifyConfig为空，子类重载时不需要再次判断（使用ModifierSettings作为参数是为了方便以后新增字段）
        /// </summary>
        /// <param name="actionState"></param>
        /// <param name="value"></param>
        /// <param name="modifierSettings"></param>
        protected virtual void DefaultModifyConfig(ActionState actionState, object value = null, ActionModifierSettings modifierSettings = null)
        {
            //——EndValue——
            if (value != null)//#1: Use income value to scale the endValue (e.g., Mouse Scroll)
            {
                EndValue = ModifyEndValue(EndValue, value);
            }

            if (modifierSettings == null)
                return;
            List<IActionModifier> listModifier = modifierSettings.listModifier;
            //#2: Use IActionModifier_Common to scale the endValue (Hosted as Component)
            foreach (var actionModifier_Common in GetListModifier<IActionModifier_EndValue>(listModifier))//处理通用字段
            {
                //PS：之所以在config处理而不是由Modifier端处理，是因为Modifier较为通用，而config可能后续会添加多种类型，且对不同类型的endValue由不同的处理方式。
                if (actionModifier_Common.IsStateActived(actionState))
                    EndValue = ModifyEndValue(EndValue, actionModifier_Common.GetEndValueScale(actionState));
            }

            //——ActionRuntimeData——
            foreach (var actionRuntimeDataModifier in GetListModifier<IActionModifier_Config>(listModifier))//#3 自定义的Modifier，按需修改Config的任意字段
            {
                actionRuntimeDataModifier.ModifyConfig(this, actionState, value);
            }
        }

        /// <summary>
        /// Modify endValue with scale param (without changing the origin value)
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        protected abstract TValue ModifyEndValue(TValue origin, object scale);//Scale the EndValue
        #endregion

        #region NaughtAttribute
        /// <summary>
        /// Should show the default [endValue] field on Inspector. Set to false to show custom field 
        /// </summary>
        protected virtual bool IsShowDefaultEndValue { get { return true; } }
        #endregion

        #region Utility
        /// <summary>
        /// 筛选符合特定类型的接口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected static IEnumerable<T> GetListModifier<T>(List<IActionModifier> listModifier)
                where T : class
        {
            if (listModifier == null)
                return new List<T>();
            return from item in listModifier
                   where item is T
                   select item as T;
        }
        #endregion
    }

    public interface IActionOption : System.ICloneable { }

    /// <summary>
    /// Extra option for config
    /// (Useful if you need extra setting base on common config (eg: ActionConfig_TweenVector3 and ActionConfig_TweenTransform))
    /// </summary>
    public abstract class ActionOptionBase : CloneableDataBase, IActionOption { }

    /// <summary>
    /// 
    /// (Useful for those need subclass (eg: ActionConfig_TweenColor & ActionConfig_TweenColorEx))
    /// </summary>
    public class ActionOption_Empty : ActionOptionBase
    {

    }
}
