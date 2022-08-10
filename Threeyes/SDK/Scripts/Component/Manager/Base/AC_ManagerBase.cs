using System.Collections;
using System.Reflection;
using System.Linq;
using UnityEngine;
using System;

public class AC_ManagerBase<T> : InstanceBase<T>
	, IAC_CommonSetting_IsAliveCursorActiveHandler
where T : InstanceBase<T>
{
	protected override void SetInstanceFunc()
	{
		base.SetInstanceFunc();

		//通过反射，自动注册到AC_ManagerHolder(Todo:提炼出接口的通用父接口）（ToDelete）
		Type typeACManagerInterface = typeof(T).GetInterfaces().FirstOrDefault(t => t.Name.StartsWith("IAC_") && t.Name.EndsWith("Manager"));
		if (typeACManagerInterface == null)//PS:有些不需要在AC_ManagerHolder中暴露的Manager，可忽略
			return;

		PropertyInfo propertyInfoStatic = typeof(AC_ManagerHolder).GetProperties(BindingFlags.Public | BindingFlags.Static).FirstOrDefault((pI) =>
		{
			return pI.PropertyType == typeACManagerInterface;
		}
		);
		if (propertyInfoStatic != null)
		{
			propertyInfoStatic.SetValue(null, this);
		}
		else
		{
			Debug.LogError($"Can't find Property {typeACManagerInterface.Name} in {nameof(AC_ManagerHolder)}");
		}
	}

	#region NaughtyAttributes
	protected const string foldoutName_Debug = "[Debug]";
	#endregion

	#region Callback
	protected bool isAliveCursorActived = false;//缓存AC的激活状态，便于Manager自行决定是否执行后续任务(不直接读取CommonSettingManager的相关字段，是为了避免Update中频繁访问，降低性能）
	public void OnIsAliveCursorActiveChanged(bool isActive)
	{
		isAliveCursorActived = isActive;
		OnIsAliveCursorActiveChangedFunc(isActive);
	}
	protected virtual void OnIsAliveCursorActiveChangedFunc(bool isActive)
	{
		enabled = isActive;//通过disable，禁止Update等常用方法运行
	}
	#endregion
}

/// <summary>
/// 管理各模块的Manager,当需要在退出前执行事件才需要继承
/// </summary>
/// <typeparam name="T"></typeparam>
public class AC_ManagerWithLifeCycleBase<T> : AC_ManagerBase<T>, IProgramLifeCycle
where T : InstanceBase<T>
{
	public bool CanQuit { get { return canQuit; } set { canQuit = value; } }
	protected bool canQuit = false;

	public virtual int QuitExecuteOrder { get { return 0; } }

	//
	/// <summary>
	/// 退出前需要立即执行的方法（常用于需要立即执行的重要方法（如涉及系统光标、注册表的操作），避免电脑关机后跳过）
	/// </summary>
	public virtual void OnQuitEnter() { }

	/// <summary>
	/// 退出前需要多帧执行的方法
	/// </summary>
	/// <returns></returns>
	public virtual IEnumerator IETryQuit()
	{
		yield return null;
		//注意：完成后要把canQuit设置为true，为了避免继承者忘了设置，默认返回true
		canQuit = true;
	}
}


public interface IManagerWithController<TControllerInterface>
{
	TControllerInterface ActiveController { get; }
}
public class AC_ManagerWithControllerBase<T, TControllerInterface, TController> : AC_ManagerBase<T>, IManagerWithController<TControllerInterface>
	where T : AC_ManagerWithControllerBase<T, TControllerInterface, TController>
	where TController : TControllerInterface
{
	public TControllerInterface ActiveController { get { return modController != null ? modController : defaultController; } }
	protected TControllerInterface modController;//Mod自定义的Controller（可空）
	[SerializeField] protected TController defaultController;//使用具体类型，便于场景引用
}