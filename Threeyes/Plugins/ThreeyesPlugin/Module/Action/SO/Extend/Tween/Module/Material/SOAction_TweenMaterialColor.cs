using DG.Tweening;
using UnityEngine;
using Threeyes.Core;
#if USE_NaughtyAttributes
using NaughtyAttributes;
#endif

namespace Threeyes.Action
{   /// <summary>
    /// Material's color type (eg: alpha, emission)
    /// </summary>
    [CreateAssetMenu(menuName = EditorDefinition_Action.AssetMenuPrefix_Action_Tween_Material + "Color", fileName = "TweenMaterialColor")]
#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
#endif
    public class SOAction_TweenMaterialColor : SOAction_TweenMaterialBase<ActionConfig_TweenColorEx, Color>
    {
        public MaterialColorType materialColorType = MaterialColorType.BaseColor;
#if USE_NaughtyAttributes
        [ShowIf("materialColorType", MaterialColorType.Custom)]
#endif
        public string customMaterialColorName;

        protected override Tween CreateTween(ActionTweenRuntimeData<ActionConfig_TweenColorEx, Color, Renderer> runtimeData)
        {
            Tween tween = null;

            Material material = GetMaterial(runtimeData.Receiver);
            if (material == null)
                return tween;

            string propertyName = materialColorType.GetPropertyName(customMaterialColorName);

#if UNITY_2021_1_OR_NEWER
            if (!material.HasColor(propertyName))
            {
                Debug.LogError(material + " doesn't have color: " + propertyName + " !");
                return tween;
            }
#endif

            var endValue = runtimeData.EndValue;
            if (runtimeData.config.Option.onlyChangeAlpha)
            {
                tween = material.DOFade(endValue.a, propertyName, runtimeData.Duration);
            }
            else
            {
                tween = material.DOColor(endValue, propertyName, runtimeData.Duration);
            }
            return tween;
        }

    }
}
