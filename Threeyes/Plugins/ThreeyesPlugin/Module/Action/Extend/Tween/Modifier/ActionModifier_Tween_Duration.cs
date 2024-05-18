using UnityEngine;
#if USE_NaughtyAttributes
using NaughtyAttributes;
#endif
namespace Threeyes.Action
{
    ////ToDelete
    //public interface IActionModifier_Tween_Duration : IActionModifier
    //{
    //    //ToAdd: 其他常用参数，如LoopType（可以单独定义每个接口（如TweenDuration、TweenLoopCounts等），或者改为传入ActionRuntimeData，由Modifier按需修改后返回）
    //    float GetDurationScale(ActionState actionState);
    //}
    /// <summary>
    /// Change TweenAction that with specify param type
    /// </summary>
    /// <typeparam name="TParam"></typeparam>
    [AddComponentMenu(EditorDefinition_Action.AssetMenuPrefix_Action_Modifier + EditorDefinition_Action.Tween + "ActionConfigModifierData_Tween_Duration")]
    public class ActionModifier_Tween_Duration : ActionModifierBase<ActionConfigModifierData_Tween_Duration>, IActionModifier_Config
    {
        public void ModifyConfig(IActionConfig config, ActionState actionState, object value)
        {
            if (!IsStateActived(actionState))
                return;

            if (config is IActionConfig_Tween actionConfig_Tween)
            {
                var configData = GetConfig(actionState);
                if (configData != null)
                {
                    float duration = actionConfig_Tween.Duration;
                    if (configData.IsOverrideDuration)//Replace the origin duration
                        duration = configData.CustomDuration;
                    actionConfig_Tween.Duration = duration * configData.DurationScale;//Scale the duration
                }
            }
        }
    }

    /// <summary>
    /// 
    /// PS：
    /// -使用数据类存储，方便后续有特殊的控制字段（比如是替换duration还是直接倍乘）
    /// </summary>
    [System.Serializable]
    public class ActionConfigModifierData_Tween_Duration : ActionConfigModifierDataBase
    {
        public bool IsOverrideDuration { get { return isOverrideDuration; } set { isOverrideDuration = value; } }
        public float CustomDuration { get { return customDuration; } set { customDuration = value; } }
        public float DurationScale { get { return durationScale; } set { durationScale = value; } }

        [SerializeField] protected bool isOverrideDuration = false;
#if USE_NaughtyAttributes
        [AllowNesting]
        [ShowIf(nameof(IsOverrideDuration))]
#endif
        [SerializeField] protected float customDuration = 1;
        [SerializeField] protected float durationScale = 1;

        public ActionConfigModifierData_Tween_Duration() : base()
        {
            durationScale = 1;
        }
    }
}