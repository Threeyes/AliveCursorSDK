using UnityEngine;
using Threeyes.Core;
#if USE_NaughtyAttributes
using NaughtyAttributes;
#endif
#if USE_DOTween
using DG.Tweening;
#endif

namespace Threeyes.Action
{   /// <summary>
    /// Material's float type (eg: _CutOff、_Metallic)
    /// </summary>
    [CreateAssetMenu(menuName = EditorDefinition_Action.AssetMenuPrefix_Action_Tween_Material + "Float", fileName = "TweenMaterialFloat")]
#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
#endif
    public class SOAction_TweenMaterialFloat : SOAction_TweenMaterialBase<ActionConfig_TweenFloat, float>
    {
        //通用设置，不另外放在DoTweenConfig中
        public MaterialFloatType materialFloatType = MaterialFloatType.AlphaCutoff;

#if USE_NaughtyAttributes
        [ShowIf("materialFloatType", MaterialFloatType.Custom)]
#endif
        public string customMaterialValueName;

#if USE_DOTween
        protected override Tween CreateTween(ActionTweenRuntimeData<ActionConfig_TweenFloat, float, Renderer> runtimeData)
        {
            Tween tween = null;

            Material material = GetMaterial(runtimeData.Receiver);
            if (material == null)
                return tween;

            var config = runtimeData.Config;
            float endValue = config.EndValue;
            string propertyName = materialFloatType.GetPropertyName(customMaterialValueName);
            if (!material.HasProperty(propertyName))
            {
                Debug.LogError(material + " doesn't have float: " + propertyName + " !");
            }
            else
            {
                tween = material.DOFloat(endValue, propertyName, config.Duration);
            }
            return tween;
        }
#endif
    }
}
