using UnityEngine;
using Threeyes.Core;
#if USE_NaughtyAttributes
using NaughtyAttributes;
#if USE_JsonDotNet
using Newtonsoft.Json;
#endif
#endif
#if USE_DOTween
using DG.Tweening;
#endif

namespace Threeyes.Action
{
    /// <summary>
    /// Material's color type (eg: alpha, emission)
    /// </summary>
    [CreateAssetMenu(menuName = EditorDefinition_Action.AssetMenuPrefix_Action_Tween_Material + "Color", fileName = "TweenMaterialColor")]
#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
#endif
    public class SOAction_TweenMaterialColor : SOAction_TweenMaterialBase<ActionConfig_TweenColorEx, Color>
    {
#if USE_JsonDotNet
        [JsonIgnore]
#endif
        public MaterialColorType MaterialColorType { get { return materialColorType; } set { materialColorType = value; } }
#if USE_JsonDotNet
        [JsonIgnore]
#endif
        public string CustomMaterialColorName { get { return customMaterialColorName; } set { customMaterialColorName = value; } }

        [SerializeField] protected MaterialColorType materialColorType = MaterialColorType.BaseColor;
#if USE_NaughtyAttributes
        [ShowIf("materialColorType", MaterialColorType.Custom)]
#endif
        [SerializeField] protected string customMaterialColorName;

#if USE_DOTween
        protected override Tween CreateTween(ActionTweenRuntimeData<ActionConfig_TweenColorEx, Color, Renderer> runtimeData)
        {
            Tween tween = null;

            Material material = GetMaterial(runtimeData.Receiver);
            if (material == null)
                return tween;

            string propertyName = MaterialColorType.GetPropertyName(CustomMaterialColorName);

#if UNITY_2021_1_OR_NEWER
            if (!material.HasColor(propertyName))
            {
                Debug.LogError(material + " doesn't have color: " + propertyName + " !");
                return tween;
            }
#endif

            var config = runtimeData.Config;
            var endValue = config.EndValue;
            if (runtimeData.Config.Option.OnlyChangeAlpha)
            {
                tween = material.DOFade(endValue.a, propertyName, config.Duration);
            }
            else
            {
                tween = material.DOColor(endValue, propertyName, config.Duration);
            }
            return tween;
        }
#endif
    }
}
