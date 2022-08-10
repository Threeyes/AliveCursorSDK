using System;
using UnityEngine;
using Threeyes.Data;
using NaughtyAttributes;
using System.Reflection;
using System.Collections.Generic;

public abstract class AC_SettingManagerBase<T, TSOConfig, TConfig> : AC_ManagerBase<T>
	where T : AC_SettingManagerBase<T, TSOConfig, TConfig>
	where TSOConfig : AC_SOConfigBase<TConfig>
	where TConfig : AC_SettingConfigInfoBase<TConfig>
{
	/// <summary>
	/// 当前App的版本
	/// </summary>
	public string strAppVersion { get { return Application.version; } }//App版本，格式必须是A.B
	public static TConfig Config
	{
		get
		{
			//if (Instance.config == null)
			return /*Instance.config =*/ Instance.soOverrideConfig != null ? Instance.soOverrideConfig.config : Instance.defaultConfig;
			//return Instance.config;
		}
	}
	//TConfig config;//PS:不能是state，便于程序重置时自动清空(Bug:不能使用，因为程序调用问题可能会导致提前销毁而无法保存）

	[SerializeField] protected TConfig defaultConfig;//Default config
	[Expandable] [SerializeField] protected TSOConfig soOverrideConfig;//Override config


	public bool HasInit { get { return hasInit; } set { hasInit = value; } }
	protected bool hasInit;
	public virtual void Init(bool isFirstInit)
	{
		UpdateConfig(isFirstInit);//更新已读取Config的特殊字段（如Version）

		InitEvent();//监听Config事件
		InitUI();//基于Config初始化UI

		Config.NotifyAllDataEvent(BasicDataState.Init);//#外部已经监听完毕，且值已经设置完毕，通知所有监听BaseData的方法，从而进行初始化
		hasInit = true;
	}
	public virtual void DeInit()
	{
		//PS：【Editor】模式下，BasicData中的Action会在Replay后自动清空。如果需要实现Reload HubScene的功能，可以增加Config.ClearAllDataEvent并在此调用
		Config.ClearAllDataEvent();
	}

	protected virtual void UpdateConfig(bool isFirstInit)
	{
		try
		{
			//PS:可以在这里针对旧版本的配置属性进行修改

			//更新本地配置的版本值
			Version curVersion = new Version(strAppVersion);
			if (curVersion != Config.version)
				Config.version = curVersion;
		}
		catch (Exception e)
		{
			Debug.LogError("UpdateConfig failed: " + e);
		}
	}
	protected virtual void InitUI()
	{
		//调用对应的UIManager
	}

	public virtual void InitEvent()
	{
	}
}

#region Define
public abstract class AC_SettingConfigInfoBase<TRealType> : AC_SerializableDataBase
 where TRealType : AC_SettingConfigInfoBase<TRealType>
{
	public Version version = new Version("3.0");//Warning：格式必须是A.B，否则报错！（The major and minor components are required; the build and revision components are optional）

	public void ClearAllDataEvent()
	{
		try
		{
			GetListBaseData().ForEach((bd) => bd.ClearEvent());
		}
		catch (Exception e)
		{
			Debug.LogError("ClearAllDataEvent with error:\r\n" + e);
		}
	}
	public void NotifyAllDataEvent(BasicDataState state = BasicDataState.Update)
	{
		{
			GetListBaseData().ForEach((bd) =>
			{
				try
				{
					bd.NotifyValueChanged(state);
				}
				catch (Exception e)
				{
					Debug.LogError($"NotifyAllDataEvent {bd} on state {state} with error:\r\n" + e);
				}
			});
		}
	}

	/// <summary>
	/// 获取所有的数据类实例
	/// </summary>
	/// <returns></returns>
	public List<BasicData> GetListBaseData()
	{
		List<BasicData> listBD = new List<BasicData>();

		//复制对应的值
		foreach (FieldInfo fieldInfo in typeof(TRealType).GetFields())
		{
			var fieldValue = fieldInfo.GetValue(this);//真实值
			if (fieldValue.GetType().IsSubclassOf(typeof(BasicData)))
			{
				BasicData inst = fieldValue as BasicData;
				listBD.Add(inst);
			}
		}
		return listBD;
	}

	public virtual void CloneTo(ref TRealType other)
	{
		try
		{
			//复制对应的值
			foreach (FieldInfo fieldInfo in typeof(TRealType).GetFields())
			{
				object fieldValue = fieldInfo.GetValue(this);//真实值

				if (fieldValue.GetType().IsSubclassOf(typeof(BasicData)))
				{
					BasicData bdThis = fieldValue as BasicData;
					var fieldValueOthers = fieldInfo.GetValue(other);
					bdThis.CloneTo(ref fieldValueOthers);
				}
			}
		}
		catch (Exception e)
		{
			Debug.LogError("Clone error:\r\n" + e);
		}
	}
}
#endregion
