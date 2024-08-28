using System;
using System.Collections;
using System.Collections.Generic;
using Threeyes.Config;
using Threeyes.Core;
using UnityEngine;
namespace Threeyes.GameFramework
{
    public abstract class MaterialControllerBase<TContainer, TSOConfig, TConfig, TPropertyBag> : ConfigurableComponentBase<TContainer, TSOConfig, TConfig, TPropertyBag>
        where TContainer : Component, IConfigurableComponent<TConfig>
        where TSOConfig : SOConfigBase<TConfig>
        where TConfig : SerializableComponentConfigInfoBase, new()
        where TPropertyBag : ConfigurableComponentPropertyBagBase<TContainer, TConfig>, new()
    {

        #region Utility
        /// <summary>
        /// 通过Renderer组件获取指定的材质
        /// </summary>
        /// <param name="targetRenderer"></param>
        /// <param name="materialIndex"></param>
        /// <param name="isShareMaterial"></param>
        /// <returns>如果找不到，则返回null</returns>
        protected Material GetMaterialFromRenderer(Renderer targetRenderer, int materialIndex, bool isShareMaterial)
        {
            if (targetRenderer)
            {
                Material desireMaterial = null;
                if (materialIndex >= 0)
                {
                    if (!Application.isPlaying || isShareMaterial)//非运行模式或共享材质
                    {
                        if (targetRenderer.sharedMaterials.Length > materialIndex)
                            desireMaterial = targetRenderer.sharedMaterials[materialIndex];
                    }
                    else
                    {
                        ///PS:以下实现可用，但是会克隆列表的所有材质。
                        ///【优先】更优的解决办法：不使用多材质，进行模型拆分
                        if (targetRenderer.materials.Length > materialIndex)
                        {
                            desireMaterial = targetRenderer.materials[materialIndex];
                        }

                        /////PS：以下实现只获取单个材质，而不是调用materials导致返回多个克隆材质（可以通过sharedMaterials获取指定的材质，然后直接克隆，并且赋值给materials字段）
                        /////     -待解决：以上实现仅调用一次，然后就通过字典等进行存储，避免重复调用
                        /////     -如果由其他物体调用materials，可能还是会导致重新生成材质克隆导致无法正常链接
                        //desireMaterial = GetCacheMaterial(targetRenderer, materialIndex);
                        //if (desireMaterial == null)
                        //{
                        //    if (targetRenderer.sharedMaterials.Length > materialIndex)
                        //    {
                        //        Material shareDesireMaterial = targetRenderer.sharedMaterials[materialIndex];
                        //        desireMaterial = Instantiate(shareDesireMaterial);
                        //        Material[] materialsClone = targetRenderer.sharedMaterials.Clone() as Material[];
                        //        materialsClone[materialIndex] = desireMaterial;//替换对应的材质
                        //        targetRenderer.materials = materialsClone;

                        //        listRuntimeRMInfo.Add(new RuntimeRenderMaterialInfo(targetRenderer, materialIndex, desireMaterial));
                        //    }
                        //}
                    }
                }
                return desireMaterial;
            }
            return null;
        }
        #endregion  
    }


    /// <summary>
    /// 针对其他模型
    /// </summary>
    [Serializable]
    public class RendererMaterialInfo
    {
        public Renderer renderer;
        public int materialIndex = 0;//对应的材质序号
    }
}