using UnityEngine;
using Threeyes.Core;
#if USE_NaughtyAttributes
using NaughtyAttributes;
#endif

namespace Threeyes.Action
{
    public interface IActionConfig : ICloneableData
    {
    }

    public abstract class ActionConfigBase<TValue> : CloneableDataBase, IActionConfig
    {
        #region Property & Field

        /// <summary>
        /// Target Value
        /// </summary>
        public virtual TValue EndValue { get { return endValue; } set { endValue = value; } }
#if USE_NaughtyAttributes
        [AllowNesting]
        [ShowIf(nameof(IsShowDefaultEndValue))]
#endif
        [SerializeField] protected TValue endValue;

        #endregion

        #region Public Method

        /// <summary>
        /// Modify endValue with scale param (without changing the origin value)
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public abstract TValue ScaleEndValue(TValue origin, object scale);//Scale the EndValue

        #endregion

        #region NaughtAttribute
        /// <summary>
        /// Should show the default [endValue] field on Inspector. Set to false to show custom field 
        /// </summary>
        protected virtual bool IsShowDefaultEndValue { get { return true; } }
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
