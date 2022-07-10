using UnityEngine;

namespace Threeyes.Action
{
    /// <summary>
    /// Change TweenAction that with specify param type
    /// </summary>
    /// <typeparam name="TParam"></typeparam>
    public class ActionModifier_Tween : ActionModifierBase<ActionConfigModifier_Tween>, IActionModifier_Tween
    {
        public float GetDurationScale(ActionState actionState)
        {
            var config = GetConfig(actionState);
            if (config != null)
                return config.DurationScale;
            return 1;
        }
    }

    [System.Serializable]
    public class ActionConfigModifier_Tween : ActionConfigModifierDataBase
    {
        public ActionConfigModifier_Tween() : base()
        {
            durationScale = 1;
        }

        public float DurationScale
        {
            get { return durationScale; }
            set
            {
                durationScale = value;
            }
        }

        [SerializeField] protected float durationScale = 1;
    }

    public interface IActionModifier_Tween : IActionModifier
    {
        float GetDurationScale(ActionState actionState);
    }

}
