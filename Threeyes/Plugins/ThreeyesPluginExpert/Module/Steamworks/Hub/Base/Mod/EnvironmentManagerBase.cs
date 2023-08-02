using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Threeyes.Steamworks
{
    public class EnvironmentManagerBase<T, TDefaultController> : HubManagerWithControllerBase<T, IEnvironmentController, TDefaultController>, IEnvironmentManager
    where T : EnvironmentManagerBase<T, TDefaultController>
       where TDefaultController : MonoBehaviour, IEnvironmentController
    {
        #region Interface
        public Camera MainCamera { get { return mainCamera; } }// 1.为了方便Modder计算可视区域，只能暴露Camera给用户，但是要提醒用户注意还原相机！否则其FOV等属性可能会被篡改（ToUpdate：可以通过将Camera放到一个子场景中，每次加载Mod就重置子场景）
        #endregion

        #region Property & Field
        [SerializeField] protected Camera mainCamera;
        #endregion

        #region Callback
        public virtual void OnModInit(Scene scene, ModEntry modEntry)
        {
            //设置Mod环境
            modController = scene.GetComponents<IEnvironmentController>().FirstOrDefault();
            defaultController.gameObject.SetActive(modController == null);//两者互斥
            ActiveController.OnModControllerInit();//初始化
        }
        public virtual void OnModDeinit(Scene scene, ModEntry modEntry)
        {
            modController?.OnModControllerDeinit();
            modController = null;//重置，否则会有引用残留
        }
        #endregion
    }
}