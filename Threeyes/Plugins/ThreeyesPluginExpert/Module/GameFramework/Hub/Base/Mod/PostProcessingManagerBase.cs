using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Threeyes.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Threeyes.GameFramework
{
    /// <summary>
    ///
    /// PS:
    /// 1.整体流程参考AC_EnvironmentManagerBase
    /// 2.因为AC只能使用部分字段，所以不提供通用的DefaultController
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PostProcessingManagerBase<T, TControllerInterface, TDefaultController, TSOControllerConfigInterface> : HubManagerWithControllerBase<T, TControllerInterface, TDefaultController>, IPostProcessingManager<TControllerInterface>
        where T : PostProcessingManagerBase<T, TControllerInterface, TDefaultController, TSOControllerConfigInterface>
        where TDefaultController : MonoBehaviour, TControllerInterface
        where TControllerInterface : IPostProcessingController
        where TSOControllerConfigInterface : ISOPostProcessingControllerConfig
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
        public virtual void OnModPreInit(Scene scene, ModEntry modEntry)
        {
            modController = scene.GetComponents<TControllerInterface>().FirstOrDefault();
            defaultController.gameObject.SetActive(modController == null);//两者互斥
        }
        public virtual void OnModInit(Scene scene, ModEntry modEntry)
        {
            if (modController != null)//Mod有自定义EnvironmentController：更新Environment
            {
                //监听ModController是否OverrideDefaultController的设置
                modController.IsUsePostProcessingChanged += OnIsUsePostProcessingChanged;
            }

            ActiveController.OnModControllerInit();//初始化
            ManagerHolderManager.Instance.FireGlobalControllerConfigStateEvent<TSOControllerConfigInterface>(modController == null);//设置对应的全局Config是否可用
        }
        public virtual void OnModDeinit(Scene scene, ModEntry modEntry)
        {
            modController?.OnModControllerDeinit();
            modController = default(TControllerInterface);//重置，否则会有引用残留
        }
        #endregion

        #region Controller Callback
        protected virtual void OnIsUsePostProcessingChanged(bool isUse)
        {
        }
        #endregion
    }
}