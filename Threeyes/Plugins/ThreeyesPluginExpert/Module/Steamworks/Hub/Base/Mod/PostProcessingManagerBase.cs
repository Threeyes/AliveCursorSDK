using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Threeyes.Steamworks
{
    /// <summary>
    ///
    /// PS:
    /// 1.整体流程参考AC_EnvironmentManagerBase
    /// 2.因为AC只能使用部分字段，所以不提供通用的DefaultController
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PostProcessingManagerBase<T, TDefaultController> : HubManagerWithControllerBase<T, IPostProcessingController, TDefaultController>, IPostProcessingManager
        where T : PostProcessingManagerBase<T, TDefaultController>
        where TDefaultController : MonoBehaviour, IPostProcessingController
    {
        #region Unity Method
        private void Awake()
        {
            //开始时监听一次
            defaultController.IsUsePostProcessingChanged += OnIsUsePostProcessingChanged;
        }
        private void OnDestroy()
        {
            defaultController.IsUsePostProcessingChanged -= OnIsUsePostProcessingChanged;
        }
        #endregion

        #region Callback
        public virtual void OnModInit(Scene scene, ModEntry modEntry)
        {
            modController = scene.GetComponents<IPostProcessingController>().FirstOrDefault();
            defaultController.gameObject.SetActive(modController == null);//两者互斥
            if (modController != null)//Mod有自定义EnvironmentController：更新Environment
            {
                //监听ModController是否OverrideDefaultController的设置
                modController.IsUsePostProcessingChanged += OnIsUsePostProcessingChanged;
            }

            ActiveController.OnModControllerInit();//初始化
        }
        public virtual void OnModDeinit(Scene scene, ModEntry modEntry)
        {
            modController?.OnModControllerDeinit();
            modController = null;//重置，否则会有引用残留
        }
        #endregion

        #region Controller Callback
        void OnIsUsePostProcessingChanged(bool isUse)
        {
            var uacData = ManagerHolder.EnvironmentManager.MainCamera.GetComponent<UniversalAdditionalCameraData>();
            uacData.renderPostProcessing = isUse;
            uacData.antialiasing = isUse ? AntialiasingMode.None : AntialiasingMode.FastApproximateAntialiasing;//PS: FXAA会导致PP的透明度失效，因此两者互斥
        }
        #endregion
    }
}