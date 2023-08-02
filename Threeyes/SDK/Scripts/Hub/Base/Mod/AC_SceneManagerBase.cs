using System.Collections;
using System.Collections.Generic;
using Threeyes.Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AC_SceneManagerBase<T> : HubSceneManagerBase<T>
	where T : AC_SceneManagerBase<T>
{
	//ToUpdate:将以下代码放到AC相关Tool方法中，删掉该不通用类，方便提炼出通用基类
    #region ModInit
    protected virtual void InitCursor(AC_AliveCursor aliveCursor)
	{
		//#1.调用Controller的Init       
		aliveCursor.Init();//优先初始化AC单例

		//#1.按顺序调用各Manager的OnModInit
		AC_ManagerHolder.CommonSettingManager.OnModInit(curModScene, aliveCursor);
		AC_ManagerHolder.EnvironmentManager.OnModInit(curModScene, aliveCursor);
		AC_ManagerHolder.PostProcessingManager.OnModInit(curModScene, aliveCursor);
		AC_ManagerHolder.TransformManager.OnModInit(curModScene, aliveCursor);
		AC_ManagerHolder.StateManager.OnModInit(curModScene, aliveCursor);
		AC_ManagerHolder.SystemCursorManager.OnModInit(curModScene, aliveCursor);
		AC_ManagerHolder.SystemAudioManager.OnModInit(curModScene, aliveCursor);
		//#2：调用其他通用组件的OnModInited
		EventCommunication.SendMessage<IModHandler>((inst) => inst.OnModInit());
	}
	protected virtual void DeInitCursor(AC_AliveCursor aliveCursor)
	{
		//#1.调用各Manager的Deinit
		AC_ManagerHolder.CommonSettingManager.OnModDeinit(curModScene, aliveCursor);
		AC_ManagerHolder.EnvironmentManager.OnModDeinit(curModScene, aliveCursor);
		AC_ManagerHolder.PostProcessingManager.OnModDeinit(curModScene, aliveCursor);
		AC_ManagerHolder.TransformManager.OnModDeinit(curModScene, aliveCursor);
		AC_ManagerHolder.StateManager.OnModDeinit(curModScene, aliveCursor);
		AC_ManagerHolder.SystemCursorManager.OnModDeinit(curModScene, aliveCursor);
		AC_ManagerHolder.SystemAudioManager.OnModDeinit(curModScene, aliveCursor);
		//#2：调用其他通用组件的OnModDeinit
		EventCommunication.SendMessage<IModHandler>((inst) => inst.OnModDeinit());
	}
    #endregion
}