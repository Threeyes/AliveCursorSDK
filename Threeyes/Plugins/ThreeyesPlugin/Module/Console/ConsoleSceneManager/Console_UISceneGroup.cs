using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace ConsoleScene
{
    public class Console_UISceneGroup : UIContentPanelBase<Console_UISceneToggle, SOSceneInfo>, ISetInstance
    {

        #region Instance
        public static Console_UISceneGroup Instance;
        bool isInit = false;
        public virtual void SetInstance()
        {
            if (!isInit)
            {
                Instance = this as Console_UISceneGroup;
                isInit = true;
            }
        }
        #endregion

        readonly string errMsg_sOConsoleSceneConfigNotFound = "未设置BuildInfo中的sOConsoleSceneManager！";

        public FloatEvent onLoadSceneProgress;
        public UnityEvent onLoadSceneNearComplete;
        public UnityEvent onLoadSceneComplete;
        public float delayLoadNextScene = 2f;//延后跳转到下一场景（让玩家可以有时间选择参数）

        public GameObject goInputEventReturnToConsolefSingleton;

        static SOConsoleSceneConfig sOConsoleSceneConfig
        {
            get
            {
                SOConsoleSceneConfig sOConsoleSceneConfig = SOConsoleSceneManager.Instance.CurConsoleSceneConfig;
                if (!sOConsoleSceneConfig)
                {
                    Debug.LogError("当前没有设置场景配置文件！");
                    return null;
                }
                return sOConsoleSceneConfig;
            }
        }

        /// <summary>
        /// 是否正在加载场景
        /// </summary>
        public bool IsLoadingScene
        {
            get
            {
                return isLoadingScene;
            }

            set
            {
                isLoadingScene = value;
            }
        }
        bool isLoadingScene = false;

        protected override void Start()
        {
            base.Start();

            if (!sOConsoleSceneConfig)
            {
                Debug.LogError(errMsg_sOConsoleSceneConfigNotFound);
                return;
            }

            //记录当前的场景
            SOSceneInfo.sceneLoaded += RecordLastScene;

            //创建UI场景列表
            Init(sOConsoleSceneConfig.listSceneInfoToDisplay);

            //首次运行后自动加载
            if (sOConsoleSceneConfig.isAutoLoad)
            {
                Invoke("LoadTargetScene", delayLoadNextScene);
            }

            //按返回键返回中控台
            if (sOConsoleSceneConfig.isPressKeyToReturnConsole)
            {
                goInputEventReturnToConsolefSingleton.SetActive(true);
            }
        }

        static void RecordLastScene(SOSceneInfo sOSceneInfo)
        {
            //查找当前的场景信息
            foreach (SOSceneInfo sOSceneInfotemp in sOConsoleSceneConfig.listSceneInfoToDisplay)
            {
                if (sOSceneInfotemp == sOSceneInfo)
                {
                    sOConsoleSceneConfig.sceneInfoLastLoaded = sOSceneInfotemp;
                    return;
                }
            }
            if (!sOSceneInfo == sOConsoleSceneConfig.consoleSceneInfo)
                Debug.Log("在列表中找不到该场景信息: " + sOSceneInfo.buildName);
            return;
        }

        #region LoadScene

        public void LoadTargetScene()
        {
            if (!sOConsoleSceneConfig)
            {
                Debug.LogError(errMsg_sOConsoleSceneConfigNotFound);
                return;
            }
            if (IsLoadingScene)//已经通过头显选中场景
                return;

            //获取下个场景
            if (sOConsoleSceneConfig.isAutoLoad)//再次检测是否自动加载（避免用户取消勾选）
            {
                SOSceneInfo sceneNext = sOConsoleSceneConfig.sceneInfoToAutoLoad;
                if (sceneNext)
                {
                    foreach (var uiEle in listUIElement)
                    {
                        if (uiEle.data.buildName == sceneNext.buildName)
                        {
                            uiEle.SetToggle(true);//加载指定场景
                        }
                    }
                }
            }
        }

        public void OnLoadProgress(float percent)
        {
            onLoadSceneProgress.Invoke(percent);
        }
        public void OnLoadNearCompleted()
        {
            if (Instance)
                onLoadSceneNearComplete.Invoke();
        }
        public void OnLoadCompleted()
        {
            if (Instance)
                onLoadSceneComplete.Invoke();
        }

        /// <summary>
        /// 获取指定序列号场景的名称
        /// </summary>
        /// <param name="buildIndex"></param>
        /// <returns></returns>
        public static string GetSceneNameByBuildIndex(int buildIndex)
        {
            //注意：不能用GetSceneByBuildIndex，因为其需要场景已经加载
            string path = SceneUtility.GetScenePathByBuildIndex(buildIndex);
            int slash = path.LastIndexOf('/');
            string name = path.Substring(slash + 1);
            int dot = name.LastIndexOf('.');
            return name.Substring(0, dot);
        }

        #endregion


        /// <summary>
        /// 弃用/禁用交互
        /// </summary>
        /// <param name="isEnable"></param>
        public void SetInteractable(bool isEnable)
        {
#if USE_VIU
            HTC.UnityPlugin.Pointer3D.CanvasRaycastTarget canvasRaycastTarget = transform.GetComponentInParent<Canvas>().GetComponent<HTC.UnityPlugin.Pointer3D.CanvasRaycastTarget>();
            if (canvasRaycastTarget)
                canvasRaycastTarget.enabled = isEnable;
#endif
        }
    }
}