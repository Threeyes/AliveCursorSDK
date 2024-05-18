using Threeyes.Core;
using UnityEngine;
#if USE_NaughtyAttributes
#endif
#if USE_DOTween
using DG.Tweening;
#endif
namespace Threeyes.Action
{
    /// <summary>
    /// ToTest
    /// </summary>
    [CreateAssetMenu(menuName = EditorDefinition_Action.AssetMenuPrefix_Action_Tween_Material + "Vector4", fileName = "TweenMaterialVector4")]
    public class SOAction_TweenMaterialVector4 : SOAction_TweenMaterialBase<ActionConfig_TweenVector4, Vector4>
    {
        //ToDelete
        public MaterialVector4Type materialVector4Type = MaterialVector4Type.Custom;
        public string customPropertyName;

#if USE_DOTween
        protected override Tween CreateTween(ActionTweenRuntimeData<ActionConfig_TweenVector4, Vector4, Renderer> runtimeData)
        {
            Tween tween = null;

            Material material = GetMaterial(runtimeData.Receiver);
            if (material == null)
                return tween;

#if UNITY_2021_1_OR_NEWER
            if (!material.HasVector(customPropertyName))
            {
                Debug.LogError(material + " doesn't have property: " + customPropertyName + " !");
                return tween;
            }
#endif

            var config = runtimeData.Config;
            Vector4 endValue = config.EndValue;
            tween = material.DOVector(endValue, customPropertyName, config.Duration);
            return tween;
        }
#endif
    }
}
