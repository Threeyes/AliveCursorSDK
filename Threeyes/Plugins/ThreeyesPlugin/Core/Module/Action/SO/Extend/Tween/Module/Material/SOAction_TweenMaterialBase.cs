using UnityEngine;

namespace Threeyes.Action
{
    public abstract class SOAction_TweenMaterialBase<TActionConfig_Tween, TParam> : SOAction_TweenBase<TActionConfig_Tween, TParam, Renderer>
        where TActionConfig_Tween : ActionConfig_TweenBase<TParam>
    {
        [Header(headerCommonSetting)]
        public int materialIndex = 0;

        #region Utility

        /// <summary>
        /// Get target from gameobject. LogError if not found
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        protected virtual Material GetMaterial(Renderer renderer)
        {
            if (materialIndex == 0)
                return renderer.material;
            else
            {
                if (renderer.materials.Length >= materialIndex + 1)
                {
                    return renderer.materials[materialIndex];
                }
                else
                {
                    Debug.LogError(renderer + " doesn't have material in index " + materialIndex + " !");
                    return null;
                }
            }
        }

        #endregion

    }
}
