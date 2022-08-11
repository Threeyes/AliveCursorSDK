using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public static class AC_EventCommunication
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TInterface"></typeparam>
	/// <param name="act"></param>
	/// <param name="includeInactive">为了避免物体为了节省性能临时隐藏而收不到Event，统一包含未激活物体</param>
	/// <param name="includeHubScene"></param>
	public static void SendMessage<TInterface>(UnityAction<TInterface> act, bool includeInactive = true, bool includeHubScene = false)
	{
		List<TInterface> result = FindClassesFromModScene<TInterface>(includeInactive).ToList();
		//UnityEngine.Debug.LogError(typeof(TInterface) + " Count: " + result.Count);
		if (includeHubScene)
		{
			result.AddRange(AC_ManagerHolder.SceneManager.HubScene.GetComponents<TInterface>(includeInactive));
		}
		for (int i = 0; i != result.Count; i++)
		{
			try
			{
				act(result[i]);//为节省资源，不判断是否为空，调用方应确保不为空
			}
			catch (System.Exception e)
			{
				//将错误写入到Item对应目录中便于制作者查看
				AC_ManagerHolder.SystemLogManager.LogError("SendMessage with error:" + e);
			}
		}
	}

	readonly static Scene EmptyScene = default(Scene);
	static IEnumerable<TInterface> FindClassesFromModScene<TInterface>(bool includeInactive = false)
	{
		Scene targetScene = EmptyScene;
#if UNITY_EDITOR
		//调试模式：如果是调试场景(而不是AliveCursorHub)，则使用当前场景(ToUpdate)
		if (AC_ManagerHolder.SceneManager == null)
		{
			targetScene = SceneManager.GetActiveScene();
		}
		else
#endif
		{
			targetScene = AC_ManagerHolder.SceneManager.CurModScene;
		}
		//每次调用都需要重新搜寻的原因是，避免有动态生成/卸载的物体包含对应接口
		//针对实现接口的内置脚本（如CursorInputBehaviourCollection）
		//PS!:以下接口也会找到Mod的自定义脚本！所以接下来的UMod调用接口的方法不是必要
		if (targetScene.IsValid())
		{
			return targetScene.GetComponents<TInterface>(includeInactive);//查找场景中所有实现接口的实例
		}
		else//可能是Mod卸载导致无法找到场景，不算报错
		{
			//Debug.LogError("Scene not load!");
			return new List<TInterface>();
		}
	}
}