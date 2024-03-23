using System.Linq;
using Threeyes.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Threeyes.Steamworks
{
    /// <summary>
    /// 用于用户自定义环境环境设置
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TControllerInterface"></typeparam>
    /// <typeparam name="TDefaultController"></typeparam>
    /// <typeparam name="TSOControllerConfigInterface"></typeparam>
    public class EnvironmentManagerBase<T, TControllerInterface, TDefaultController, TSOControllerConfigInterface> : HubManagerWithControllerBase<T, TControllerInterface, TDefaultController>, IEnvironmentManager<TControllerInterface>
        where T : EnvironmentManagerBase<T, TControllerInterface, TDefaultController, TSOControllerConfigInterface>
        where TDefaultController : MonoBehaviour, TControllerInterface
        where TControllerInterface : IEnvironmentController
            where TSOControllerConfigInterface : ISOEnvironmentControllerConfig
    {
        #region Interface
        public virtual Camera MainCamera { get { return mainCamera; } }// 1.为了方便Modder计算可视区域，只能暴露Camera给用户，但是要提醒用户注意还原相机！否则其FOV等属性可能会被篡改（ToUpdate：可以通过将Camera放到一个子场景中，每次加载Mod就重置子场景）（ToRemove：移动到）
        #endregion

        #region Property & Field
        [SerializeField] protected Camera mainCamera;
        #endregion

        #region Callback
        public virtual void OnModInit(Scene scene, ModEntry modEntry)
        {
            //设置Mod环境
            modController = scene.GetComponents<TControllerInterface>().FirstOrDefault();
            defaultController.gameObject.SetActive(modController == null);//两者互斥
            ActiveController.OnModControllerInit();//初始化
            ManagerHolderManager.Instance.FireGlobalControllerConfigStateEvent<TSOControllerConfigInterface>(modController == null);//设置对应的全局Config是否可用
        }
        public virtual void OnModDeinit(Scene scene, ModEntry modEntry)
        {
            modController?.OnModControllerDeinit();
            modController = default(TControllerInterface);//重置，否则会有引用残留
        }
        #endregion
    }
}