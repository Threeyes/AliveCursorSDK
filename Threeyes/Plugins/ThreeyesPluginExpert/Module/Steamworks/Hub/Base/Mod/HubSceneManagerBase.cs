using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Threeyes.Steamworks
{
    public class HubSceneManagerBase<T> : HubManagerWithLifeCycleBase<T>, IHubSceneManager
        where T: HubSceneManagerBase<T>
    {
		#region Interface
		public Scene HubScene { get { return hubScene; } }
		public Scene CurModScene { get { return curModScene; } }

		protected Scene hubScene;
		protected Scene curModScene;

		protected override void SetInstanceFunc()
		{
			base.SetInstanceFunc();
			hubScene = gameObject.scene;//Use self scene
		}

        #endregion

        #region ModInit
        protected virtual void InitMod(ModEntry modEntry)
        {
            //#0.调用Controller的Init       
            modEntry.Init();//优先初始化单例

            //#1.按顺序调用各Manager的OnModInit
            ManagerHolder.GetListManagerModInitOrder().ForEach(m => m.OnModInit(curModScene, modEntry));
            //#2：调用通用组件的OnModInited
            EventCommunication.SendMessage<IModHandler>((inst) => inst.OnModInit());
        }
        protected virtual void DeInitMod(ModEntry modEntry)
        {
            //#1.调用各Manager的Deinit
            ManagerHolder.GetListManagerModInitOrder().ForEach(m => m.OnModDeinit(curModScene, modEntry));
            //#2：调用其他通用组件的OnModDeinit
            EventCommunication.SendMessage<IModHandler>((inst) => inst.OnModDeinit());
        }
        #endregion
    }
}