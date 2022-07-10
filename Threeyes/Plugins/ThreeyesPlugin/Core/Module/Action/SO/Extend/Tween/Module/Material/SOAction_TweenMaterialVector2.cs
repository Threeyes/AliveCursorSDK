using DG.Tweening;
using UnityEngine;
#if USE_NaughtyAttributes
using NaughtyAttributes;
#endif

namespace Threeyes.Action
{
    [CreateAssetMenu(menuName = EditorDefinition_Action.AssetMenuPrefix_Action_Tween_Material + "Vector2", fileName = "TweenMaterialVector2")]
    public class SOAction_TweenMaterialVector2 : SOAction_TweenMaterialBase<ActionConfig_TweenVector2, Vector2>
    {
        public MaterialVector2Type materialVector2Type = MaterialVector2Type.Offset;
        public MaterialTextureType materialTextureType = MaterialTextureType.BaseMap;
#if USE_NaughtyAttributes
        [ShowIf("materialTextureType", MaterialTextureType.Custom)]
#endif
        public string customMaterialTextureName;

        protected override Tween CreateTween(ActionTweenRuntimeData<ActionConfig_TweenVector2, Vector2, Renderer> runtimeData)
        {
            Tween tween = null;

            Material material = GetMaterial(runtimeData.Receiver);
            if (material == null)
                return tween;

            string textureName = materialTextureType.GetPropertyName(customMaterialTextureName);

#if UNITY_2021_1_OR_NEWER
            if (!material.HasTexture(textureName))
            {
                Debug.LogError(material + " doesn't have texture: " + textureName + " !");
                return tween;
            }
#endif

            Vector2 endValue = runtimeData.EndValue;
            switch (materialVector2Type)
            {
                case MaterialVector2Type.Offset:
                    tween = material.DOOffset(endValue, textureName, runtimeData.Duration); break;
                case MaterialVector2Type.Tiling:
                    tween = material.DOTiling(endValue, textureName, runtimeData.Duration); break;
                default:
                    Debug.LogError("Not define!"); break;
            }
            return tween;
        }
    }
}
