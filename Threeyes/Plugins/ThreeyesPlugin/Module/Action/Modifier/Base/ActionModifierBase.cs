using System.Collections;
using System.Collections.Generic;
using Threeyes.Core;
using UnityEngine;
namespace Threeyes.Action
{
    public abstract class ActionModifierBase<TActionConfigModifier> : MonoBehaviour, IActionModifier
          where TActionConfigModifier : ActionConfigModifierDataBase, new()
    {
        public bool IsActiveEnter { get { return isActiveEnter; } set { isActiveEnter = value; } }
        public bool IsActiveExit { get { return isActiveExit; } set { isActiveExit = value; } }
        [SerializeField] protected bool isActiveEnter = true;
        [SerializeField] protected bool isActiveExit = true;

        public TActionConfigModifier enterConfigModifier = new TActionConfigModifier();
        public TActionConfigModifier exitConfigModifier = new TActionConfigModifier();

        public bool IsStateActived(ActionState actionState)
        {
            if (actionState.Has(ActionState.Enter) && IsActiveEnter)
                return true;
            if (actionState.Has(ActionState.Exit) && IsActiveExit)
                return true;
            return false;
        }

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
    /// <summary>
    /// Modify soAction's common config.
    /// Use this component toachieve different outcome with the same soAction asset
    /// </summary>
    /// <typeparam name="TParam"></typeparam>
    public abstract class ActionModifierBase<TActionConfigModifierData, TParam> : ActionModifierBase<TActionConfigModifierData>, IActionModifier_Common
        where TActionConfigModifierData : ActionConfigModifierDataBase<TParam>, new()
    {
        public object GetObjEndValueScale(ActionState actionState)
        {
            var config = GetConfig(actionState);
            if (config != null)
                return config.ObjEndValueScale;
            return null;
        }
    }


    /// <summary>
    /// Base Interface for all Action Modifier
    /// </summary>
    public interface IActionModifier
    {
        bool IsActiveEnter { get; set; }
        bool IsActiveExit { get; set; }

        bool IsStateActived(ActionState actionState);
    }

    /// <summary>
    /// Modify Value for SoActionBase
    /// </summary>
    public interface IActionModifier_Common : IActionModifier
    {
        object GetObjEndValueScale(ActionState actionState);
    }
}