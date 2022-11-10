using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AC_SceneManagerBase<T> : AC_ManagerWithLifeCycleBase<T>, IAC_SceneManager
	where T : AC_SceneManagerBase<T>
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

	protected virtual void InitCursor(AC_AliveCursor aliveCursor)
	{
		//#1.调用Controller的Init       
		aliveCursor.Init();//优先初始化AC

		//按顺序调用各Manager.OnModInit
		AC_ManagerHolder.CommonSettingManager.OnModInit(curModScene, aliveCursor);
		AC_ManagerHolder.EnvironmentManager.OnModInit(curModScene, aliveCursor);
		AC_ManagerHolder.PostProcessingManager.OnModInit(curModScene, aliveCursor);
		AC_ManagerHolder.TransformManager.OnModInit(curModScene, aliveCursor);
		AC_ManagerHolder.StateManager.OnModInit(curModScene, aliveCursor);
		AC_ManagerHolder.SystemCursorManager.OnModInit(curModScene, aliveCursor);
		AC_ManagerHolder.SystemAudioManager.OnModInit(curModScene, aliveCursor);
		//#2：调用其他通用组件的OnModInited
		AC_EventCommunication.SendMessage<IAC_ModHandler>((inst) => inst.OnModInit());
	}
	protected virtual void DeInitCursor(AC_AliveCursor aliveCursor)
	{
		//#1.调用Controller的Deinit
		AC_ManagerHolder.CommonSettingManager.OnModDeinit(curModScene, aliveCursor);
		AC_ManagerHolder.EnvironmentManager.OnModDeinit(curModScene, aliveCursor);
		AC_ManagerHolder.PostProcessingManager.OnModDeinit(curModScene, aliveCursor);
		AC_ManagerHolder.TransformManager.OnModDeinit(curModScene, aliveCursor);
		AC_ManagerHolder.StateManager.OnModDeinit(curModScene, aliveCursor);
		AC_ManagerHolder.SystemCursorManager.OnModDeinit(curModScene, aliveCursor);
		AC_ManagerHolder.SystemAudioManager.OnModDeinit(curModScene, aliveCursor);
		//#2：调用其他通用组件的OnModDeinit
		AC_EventCommunication.SendMessage<IAC_ModHandler>((inst) => inst.OnModDeinit());
	}
}