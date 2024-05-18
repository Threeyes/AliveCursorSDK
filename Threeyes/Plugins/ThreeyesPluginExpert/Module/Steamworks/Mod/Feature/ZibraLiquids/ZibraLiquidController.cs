using com.zibra.liquid.DataStructures;
using com.zibra.liquid.Manipulators;
using com.zibra.liquid.Solver;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Threeyes.Persistent;
using UnityEngine.Events;
using Newtonsoft.Json;
using System.Linq;
using Threeyes.Config;
using Threeyes.Core;

namespace Threeyes.Steamworks
{

    /// <summary>
    /// [Required]
    /// Function:
    /// 1.Fix init error when umod loaded
    ///
    /// 
    /// PS:
    /// 1.该类应该是通用的针对Config的通用代码，本类中任意参数设置后都需要重新调用Init方法
    /// 2.为了避免Config字段过多导致设置麻烦，且为了减少耦合，每个组件都要有对应的Controller及配置，其中过长的名字可以缩短（参考ZibraLiquid中的字段命名）
    ///
    /// Warning：
    /// 1.
    /// </summary>
    public class ZibraLiquidController : ConfigurableComponentBase<ZibraLiquid, SOZibraLiquidControllerConfig, ZibraLiquidController.ConfigInfo>, IZibraLiquidController_SettingHandler
        , IModHandler
        , IHubSystemWindow_ChangeCompletedHandler
    {
        #region Unity Method
        private void Awake()
        {
            Config.actionPersistentChanged += OnPersistentChanged;
        }
        private void OnDestroy()
        {
            Config.actionPersistentChanged -= OnPersistentChanged;
        }
        #endregion

        #region Callback
        public void OnWindowChangeCompleted()
        {
            UpdateContainerSize();
        }
        public void OnModInit()
        {
            CoroutineManager.StartCoroutineEx(IEInit());
        }
        public void OnModDeinit()
        {
        }
        bool hasInit = false;
        IEnumerator IEInit()
        {
            //等待ZibraLiquid.Start调用完毕		
            yield return null;
            yield return null;

            //#1 删除因为ZibraLiquid的[RequireComponent]导致某些组件被强制加上，导致与UMod反序列化的组件重合
            if (!hasInit)
            {
                if (GetComponents<ZibraLiquidMaterialParameters>().Length > 1)
                    DestroyImmediate(Comp.materialParameters);
                if (GetComponents<ZibraLiquidSolverParameters>().Length > 1)
                    DestroyImmediate(Comp.solverParameters);
                if (GetComponents<ZibraLiquidAdvancedRenderParameters>().Length > 1)
                    DestroyImmediate(Comp.renderingParameters);
                if (GetComponents<ZibraManipulatorManager>().Length > 1)
                    DestroyImmediate(Comp.manipulatorManager);
                yield return null;//等待销毁完成

                //重新绑定
                Comp.materialParameters = GetComponent<ZibraLiquidMaterialParameters>();
                Comp.solverParameters = GetComponent<ZibraLiquidSolverParameters>();
                Comp.renderingParameters = GetComponent<ZibraLiquidAdvancedRenderParameters>();
                Comp.manipulatorManager = GetComponent<ZibraManipulatorManager>();

            }
            //#2 更新设置
            //通知场景所有与PD相关的Controller更新参数(通过查询指定接口并调用）
            UnityEngine.SceneManagement.Scene scene = gameObject.scene;
            IEnumerable<IZibraLiquidController_SettingHandler> controllers = scene.GetComponents<IZibraLiquidController_SettingHandler>(true);
            controllers.ToList().ForEach(
                (i) =>
                {
                    i.UpdateSetting();
                });
        }

        public void UpdateSetting()
        {
            gameObject.SetActive(false);//禁用才能修改字段
            UpdateContainerSize();
            gameObject.SetActive(true);//重新激活物体，让其进行初始化
        }

        private void UpdateContainerSize()
        {
            if (Config.isContainerMatchView)
            {
                float screenAspect = (float)Screen.width / Screen.height;
                Comp.containerSize = new Vector3(Comp.containerSize.y * screenAspect, Comp.containerSize.y, Comp.containerSize.z);
            }
        }


        void OnPersistentChanged(PersistentChangeState persistentChangeState)
        {
            if (persistentChangeState == PersistentChangeState.Load)
                return;
            UpdateSetting();
        }
        #endregion

        #region Define
        [System.Serializable]
        [PersistentChanged(nameof(ConfigInfo.OnPersistentChanged))]
        public class ConfigInfo : SerializableDataBase
        {
            [JsonIgnore] public UnityAction<PersistentChangeState> actionPersistentChanged;

            public bool isContainerMatchView = true;//Change containerSize to match ScreenView, mainly for fullscreen liquid

            [Header("Common")]
            [Range(1024, 2097152)] public int maxParticleCount = 4864;//(太低会出现藕断丝连的闪烁现象）

            #region Callback
            void OnPersistentChanged(PersistentChangeState persistentChangeState)
            {
                actionPersistentChanged.Execute(persistentChangeState);
            }
            #endregion
        }
        #endregion
    }
}