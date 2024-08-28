using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using Threeyes.Core;
using UnityEngine;
using Threeyes.Persistent;
using UnityEngine.Events;
using System.Linq;
using Threeyes.Data;

namespace Threeyes.GameFramework
{
    /// <summary>
    /// Change target renderer's material
    /// </summary>
    public class MaterialSwitchController : MaterialControllerBase<MaterialSwitchController, SOMaterialSwitchControllerConfig, MaterialSwitchController.ConfigInfo, MaterialSwitchController.PropertyBag>
    {
        #region Property & Field
        [Header("Target")]
        [SerializeField] protected Renderer targetRenderer;//Where the main material attached
        [SerializeField] protected int targetMaterialIndex = 0;
        [SerializeField] protected List<RendererMaterialInfo> listRendererMaterialInfo = new List<RendererMaterialInfo>();//其他模型的材质信息，方便针对多个使用了相同或类似材质的模型进行统一修改

        public IntEvent onOptionMaterialIndexChanged;//Notify index changed
        #endregion

        public void SetRandomMaterial()
        {
            SetMaterialByIndex(UnityEngine.Random.Range(0, Config.listOptionMaterial.Count));
        }

        /// <summary>
        /// Manually set material with specified numbers in the list
        /// 
        /// Use case:
        /// -需要与父类MaterialSwitchController同步设置的不同材质，如LOD（通过父类的onOptionMaterialIndexChanged调用该方法实现）
        /// </summary>
        /// <param name="index"></param>
        public void SetMaterialByIndex(int index)
        {
            if (index < 0 || index >= Config.listOptionMaterial.Count)
            {
                Debug.LogError($"Index out of bounds! {index} in {Config.listOptionMaterial.Count}!");
                return;
            }
            Material targetMaterial = Config.listOptionMaterial[index];
            Config.curOptionMaterial = targetMaterial;
            Config.curOptionMaterialIndex = index;
            SetMaterial(targetMaterial);

            onOptionMaterialIndexChanged.Invoke(index);//Notify
        }

        public override void UpdateSetting()
        {
            //PS:因为可能还会设置listRendererMaterialInfo，所有暂不判断是否与当前Renderer的材质相同（仅设置材质，不更改属性应该性能消耗不大）

            Material curMaterial = Config.curOptionMaterial;
            SetMaterial(curMaterial);

            onOptionMaterialIndexChanged.Invoke(Config.curOptionMaterialIndex);//Notify
        }

        void SetMaterial(Material curMaterial)
        {
            SetMaterialToRenderer(targetRenderer, targetMaterialIndex, curMaterial);

            //针对额外的Renderer进行修改（因为使用的材质可能不一致，仅仅是某些字段相同，所以不能直接用Material对其他Renderer进行替换）
            foreach (var rmInfo in listRendererMaterialInfo)
            {
                SetMaterialToRenderer(rmInfo.renderer, rmInfo.materialIndex, curMaterial);
            }
        }

        #region Utility
        void SetMaterialToRenderer(Renderer renderer, int materialIndex, Material material)
        {
            if (!renderer)
                return;

            if (!material)
            {
                //Debug.LogError("Cur material is null!");
                return;
            }

            if (materialIndex == 0)
            {
                renderer.material = material;
            }
            else
            {
                var materials = renderer.sharedMaterials.ToList();//使用共享材质，避免克隆原材质
                if (materialIndex <= materials.Count - 1)
                {
                    materials[materialIndex] = material;
                    renderer.materials = materials.ToArray();
                }
                else
                {
                    Debug.LogError($"Index out of bounds! {materialIndex} in {materials.Count}!");
                }
            }
        }
        #endregion

        #region Editor
#if UNITY_EDITOR
        [ContextMenu("EditorSetup")]
        void EditorSetup()
        {
            //Set renderer
            if (!targetRenderer)
                targetRenderer = GetComponent<Renderer>();
            if (!targetRenderer)
            {
                Debug.LogError("Can't find Renderer in " + gameObject);
            }

            //Set cur index and material (尽量与Renderer当前材质相同)
            if (Config.curOptionMaterialIndex < Config.listOptionMaterial.Count)
            {
                Config.curOptionMaterial = Config.listOptionMaterial[Config.curOptionMaterialIndex];
            }
            UnityEditor.EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// 使用当前Renderer的材质，作为默认序号
        /// 
        /// Warning:
        /// -You should Init other field first, you can first invoke EditorSetup
        /// </summary>
        [ContextMenu("EditorUseCurRendererMaterialAsDefault")]
        void EditorUseCurRendererMaterialAsDefault()
        {
            if (!targetRenderer)
            {
                Debug.LogError("targetRenderer is null!");
                return;
            }

            Material curMaterial = GetMaterialFromRenderer(targetRenderer, targetMaterialIndex, true);
            int relatedIndex = Config.listOptionMaterial.IndexOf(curMaterial);
            if (relatedIndex < 0)
            {
                Debug.LogError($"{gameObject}'s Material [{curMaterial}] not exist in list!");
            }
            else
            {
                Config.curOptionMaterialIndex = relatedIndex;
                Config.curOptionMaterial = Config.listOptionMaterial[Config.curOptionMaterialIndex];
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
#endif
        #endregion

        #region Define
        [Serializable]
        public class ConfigInfo : SerializableComponentConfigInfoBase
        {
            [JsonIgnore] public Material curOptionMaterial;//Cur selected material (通过RuntimeEditor设置更新)
            [Tooltip("All optional material")][JsonIgnore] public List<Material> listOptionMaterial = new List<Material>();
            [PersistentOption(nameof(listOptionMaterial), nameof(curOptionMaterial))] public int curOptionMaterialIndex = 0;//提供下拉菜单选项
        }
        public class PropertyBag : ConfigurableComponentPropertyBagBase<MaterialSwitchController, ConfigInfo> { }

        /// <summary>
        /// 针对其他模型
        /// 
        /// PS:
        /// -暂不复用MaterialController的类，方便以后按需添加字段
        /// </summary>
        [Serializable]
        public class RendererMaterialInfo
        {
            public Renderer renderer;
            public int materialIndex = 0;//对应的材质序号
        }

        #endregion
    }
}