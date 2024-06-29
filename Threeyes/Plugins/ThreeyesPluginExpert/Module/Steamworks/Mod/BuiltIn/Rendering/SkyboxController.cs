using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Threeyes.RuntimeEditor;
using Threeyes.Core;
using NaughtyAttributes;
using Threeyes.Localization;
using UnityEngine.Events;

namespace Threeyes.Steamworks
{
    /// <summary>
    /// Replace the global Skybox Texture
    /// 
    /// PS：
    /// -如果用户需要快速替换 SkyboxController，可以在Hierarchy中对该物体点击右键，然后选择：更改样式，折后选中其他SkyboxController即可
    /// 
    /// Todo:
    /// - 【待确认】或者改为从其他位置获取材质（如RenderHelper），好处是兼容他们的修改
    /// - 或者是MaterialController/RendererHelper继承IMaterialProvider，然后这个类引用他们，并且获取他们的材质
    /// -【优先】SkyboxController继承IMaterialProvider，然后MaterialController可以通过引用组件(引用Mono，字段叫materialProvider)获取该接口，从而修改对应材质（本类可以封装skyboxMaterial，让其直接返回克隆的材质，避免直接修改原材质）
    /// -需要通知DefaultEnvironmentController，表明要重载天空盒，让其停止修改天空盒材质
    /// - 一旦使用了SkyboxController，那么其材质属性就自行控制，EnvironmentController不会对其进行修改（由EnvironmentController来处理这些逻辑，同时会隐藏Config中关于Skybox的各种属性（通过运行时设置Config中的某个临时bool即可））
    /// -通过x的方法来更新天空盒，因为其内部有其他额外方法（如通知场景的反射探头更新，需要其有一个回调方便探头监听）
    /// -【重要】如果当前用户点击了天空盒（或者是空地方），那么就选中当前激活的SkyboxController对应物体，方便删除
    /// 
    /// -Config
    ///     -【不急】【非必须】AutoRotate(参考SkyboxManager，该字段可能不通用，仅在Panoramic上由，需要判断)
    ///     -材质上的其他配置都留给MaterialController修改
    /// </summary>
    public class SkyboxController : MonoBehaviour
         , IRuntimeHierarchyItemProvider
    {
        #region Property & Field
        public Material SkyboxMaterial
        {
            get
            {

                if (runtimeSkyboxMaterial == null)//#1 尝试从可选项中找到首个有效材质
                {
                    runtimeSkyboxMaterial = GetMaterialFromProvider(targetMaterialProvider, useSharedMaterial);
                }
                if (runtimeSkyboxMaterial == null)//#2 如果上述无效，则从返回克隆的实例材质
                {
                    runtimeSkyboxMaterial = Instantiate(skyboxMaterial);//返回克隆的材质，避免直接修改。(后期可增加字段isShareMaterial，决定是否返回原材质)
                }

                return runtimeSkyboxMaterial;
            }
        }

        [Header("Material Source")]
        //#Only one of the following fields is required to provide a valid material
        [ValidateInput(nameof(CheckIfTargetMaterialProviderInhericCorrectly), "This component must inherit IMaterialProvider")] [SerializeField] protected Component targetMaterialProvider;//Component inherits IMaterialProvider（可通过Renderer作为提供Material的载体，并且该字段引用RendererHelper，以便使用其他组件RE修改材质）
        [SerializeField] Material skyboxMaterial;//[Optional] material asset

        [Header("Config")]
        [SerializeField] protected bool useSharedMaterial = false;//是否使用共享材质（仅当targetRenderer不为空时有效），适用于多个物体共用同一个材质

        //#Runtime
        public bool HasInit { get { return hasInit; } }
        public bool IsActive { get { return isActive; } }
        Material runtimeSkyboxMaterial;
        bool hasInit = false;
        bool isActive;

        bool CheckIfTargetMaterialProviderInhericCorrectly(Component targetMaterialProvider)
        {
            if (!targetMaterialProvider)//因为该字段是可选，所以仅非空时才判断
                return true;
            return targetMaterialProvider is IMaterialProvider;
        }
        /// <summary>
        /// 尝试从目标中返回指定材质
        /// </summary>
        /// <param name="materialProvider">如果找不到，则返回null</param>
        /// <returns></returns>
        Material GetMaterialFromProvider(Component materialProvider, bool useSharedMaterial)
        {
            if (materialProvider)
            {
                if (materialProvider is IMaterialProvider materialProviderInst)
                {
                    if (!Application.isPlaying || useSharedMaterial)//非运行模式或共享材质
                    {
                        return materialProviderInst.TargetSharedMaterial;
                    }
                    else
                    {
                        return materialProviderInst.TargetMaterial;
                    }
                }
            }
            return null;
        }
        #endregion

        #region Init
        private void Start()//等待Ghost点击摆放后才生效，统一
        {
            cacheEnum_Init = CoroutineManager.StartCoroutineEx(IEInit());
        }
        private void OnDestroy()
        {
            TryStopCoroutine_Init();
            if (!hasInit)
                return;
            if (ManagerHolder.EnvironmentManager == null)
                return;
            ManagerHolder.EnvironmentManager.BaseActiveController.UnRegisterSkyboxController(this);
        }

        protected UnityEngine.Coroutine cacheEnum_Init;

        IEnumerator IEInit()
        {
            if (ManagerHolder.SceneManager == null)
                yield break;
            while (ManagerHolder.SceneManager.IsChangingScene)//等待场景初始化完成（主要是ActiveController被初始化）
                yield return null;

            if (ManagerHolder.EnvironmentManager == null)
                yield break;
            ManagerHolder.EnvironmentManager.BaseActiveController.RegisterSkyboxController(this);

            //把该物体挪到最远的位置，避免被框选误选中
            transform.position = Vector3.one * 10000;

            hasInit = true;
        }
        protected virtual void TryStopCoroutine_Init()
        {
            if (cacheEnum_Init != null)
            {
                CoroutineManager.StopCoroutineEx(cacheEnum_Init);
                cacheEnum_Init = null;
            }
        }

        #endregion

        #region Public
        public BoolEvent onActivateDeactivate = new BoolEvent();
        public UnityEvent onActivate = new UnityEvent();
        public UnityEvent onDeactivate = new UnityEvent();

        public virtual void SetActive(bool isActive)
        {
            this.isActive = isActive;

            onActivateDeactivate.Invoke(isActive);
            if (isActive)
                onActivate.Invoke();
            else
                onDeactivate.Invoke();
        }
        #endregion

        #region IRuntimeHierarchyItemProvider
        public RuntimeHierarchyItemInfo GetRuntimeHierarchyItemInfo()
        {
            RuntimeHierarchyItemInfo runtimeHierarchyItemInfo = new RuntimeHierarchyItemInfo();
            IEnvironmentController environmentController = ManagerHolder.EnvironmentManager.BaseActiveController;
            if (environmentController.SkyboxControllerCount > 1 && environmentController.ActiveSkyboxController != this)
            {
                runtimeHierarchyItemInfo.warningTips = LocalizationManagerHolder.LocalizationManager.GetTranslationText("RuntimeEditor/Hierarchy/OnlyOneCanExistInTheScene");//"Only one such object can exist in the scene at the same time, and this object will not take effect!";//Todo：多语言翻译：场景只能同时存在一个此类物体，该物体不会生效！
            }

            return runtimeHierarchyItemInfo;
        }
        #endregion
    }
}