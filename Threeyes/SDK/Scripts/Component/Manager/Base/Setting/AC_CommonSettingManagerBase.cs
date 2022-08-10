using System;
using Threeyes.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AC_CommonSettingManagerBase<T> : AC_SettingManagerBase<T, AC_SOCommonSettingManagerConfig, AC_CommonSettingConfigInfo>, IAC_CommonSettingManager
	where T : AC_CommonSettingManagerBase<T>
{
	#region Interface
	public bool IsHideOnTextInput { get { return Config.cursorAppearance_IsHideOnTextInput.Value; } }
	public float CursorSize { get { return Config.cursorAppearance_CursorSize.Value; } }

	public bool IsStandByActive { get { return Config.cursorState_StandBy_IsActive.Value; } }
	public float StandByDelayTime { get { return Config.cursorState_StandBy_DelayTime.Value; } }
	public bool IsBoredActive { get { return Config.cursorState_Bored_IsActive.Value; } }
	public float BoredDelayTime { get { return Config.cursorState_Bored_DelayTime.Value; } }
	public float BoredDepth { get { return Config.cursorState_Bored_Depth.Value; } }

	public string Localization { get { return Config.generalSetting_Localization.Value; } }
	public string Quality { get { return Config.generalSetting_Quality.Value; } }
	public string ProcessPriority { get { return Config.generalSetting_ProcessPriority.Value; } }
	public bool IsVSyncActive { get { return Config.generalSetting_IsVSyncActive.Value; } }
	public int TargetFrameRate { get { return Config.generalSetting_TargetFrameRate.Value; } }

	public bool IsCursorActive { get { return Config.generalSetting_IsAliveCursorActive.Value; } }
	public bool SupportMultiDisplay { get { return Config.generalSetting_IsSupportMultiDisplay.Value; } }
	#endregion

	#region Data Events
	public override void InitEvent()
	{
		Config.cursorAppearance_IsHideOnTextInput.actionValueChanged += OnDataIsHideOnTextInputChanged;
		Config.cursorAppearance_CursorSize.actionValueChanged += OnDataCursorSizeChanged;

		Config.cursorState_StandBy_IsActive.actionValueChanged += OnDataIsStandByActiveChanged;
		Config.cursorState_StandBy_DelayTime.actionValueChanged += OnDataStandByDelayTimeChanged;
		Config.cursorState_Bored_IsActive.actionValueChanged += OnDataIsBoredActiveChanged;
		Config.cursorState_Bored_DelayTime.actionValueChanged += OnDataBoredDelayTimeChanged;
		Config.cursorState_Bored_Depth.actionValueChanged += OnDataBoredDepthChanged;

		Config.generalSetting_IsAliveCursorActive.actionValueChanged += OnDataIsAliveCursorActiveChanged;
		Config.generalSetting_IsRunAtStartUp.actionValueChanged += OnDataIsRunAtStartUpChanged;
		Config.generalSetting_IsSupportMultiDisplay.actionValueChanged += OnDataIsSupportMultiDisplayChanged;
		Config.generalSetting_IsVSyncActive.actionValueChanged += OnDataIsVSyncActiveChanged;
		Config.generalSetting_TargetFrameRate.actionValueChanged += OnDataTargetFrameRateChanged;
		Config.generalSetting_Localization.actionValueChanged += OnDataLocalizationChanged;
		Config.generalSetting_Quality.actionValueChanged += OnDataQualityChanged;
		Config.generalSetting_ProcessPriority.actionValueChanged += OnDataProcessPriorityChanged;
	}

	protected virtual void OnDataIsHideOnTextInputChanged(bool value)
	{
		AC_EventCommunication.SendMessage<IAC_CommonSetting_IsHideOnTextInputHandler>(inst => inst.OnIsHideOnTextInputChanged(value), includeHubScene: true);
	}
	protected virtual void OnDataCursorSizeChanged(float value)
	{
		AC_EventCommunication.SendMessage<IAC_CommonSetting_CursorSizeHandler>(inst => inst.OnCursorSizeChanged(value), includeHubScene: true);
	}

	protected virtual void OnDataIsStandByActiveChanged(bool value)
	{
		AC_EventCommunication.SendMessage<IAC_CommonSetting_IsStandByActiveHandler>(inst => inst.OnIsStandByActiveChanged(value), includeHubScene: true);
	}
	protected virtual void OnDataStandByDelayTimeChanged(float value)
	{
		AC_EventCommunication.SendMessage<IAC_CommonSetting_StandByDelayTimeHandler>(inst => inst.OnStandByDelayTimeChanged(value), includeHubScene: true);
	}
	protected virtual void OnDataIsBoredActiveChanged(bool value)
	{
		AC_EventCommunication.SendMessage<IAC_CommonSetting_IsBoredActiveHandler>(inst => inst.OnIsBoredActiveChanged(value), includeHubScene: true);
	}
	protected virtual void OnDataBoredDelayTimeChanged(float value)
	{
		AC_EventCommunication.SendMessage<IAC_CommonSetting_BoredDelayTimeHandler>(inst => inst.OnBoredDelayTimeChanged(value), includeHubScene: true);
	}
	protected virtual void OnDataBoredDepthChanged(float value)
	{
		AC_EventCommunication.SendMessage<IAC_CommonSetting_BoredDepthHandler>(inst => inst.OnBoredDepthChanged(value), includeHubScene: true);
	}

	protected virtual void OnDataIsAliveCursorActiveChanged(bool value)
	{
		AC_EventCommunication.SendMessage<IAC_CommonSetting_IsAliveCursorActiveHandler>(inst => inst.OnIsAliveCursorActiveChanged(value), includeHubScene: true);
	}
	protected virtual void OnDataIsRunAtStartUpChanged(bool value)
	{
		AC_EventCommunication.SendMessage<IAC_CommonSetting_IsRunAtStartUpHandler>(inst => inst.OnIsRunAtStartUpChanged(value), includeHubScene: true);
	}
	protected virtual void OnDataIsSupportMultiDisplayChanged(bool value)
	{
		AC_EventCommunication.SendMessage<IAC_CommonSetting_IsSupportMultiDisplayHandler>(inst => inst.OnIsSupportMultiDisplayChanged(value), includeHubScene: true);
	}
	protected virtual void OnDataIsVSyncActiveChanged(bool isActive)
	{
		AC_EventCommunication.SendMessage<IAC_CommonSetting_IsVSyncActiveHandler>(inst => inst.OnIsVSyncActiveChanged(isActive), includeHubScene: true);
	}
	protected virtual void OnDataTargetFrameRateChanged(int value)
	{
		AC_EventCommunication.SendMessage<IAC_CommonSetting_TargetFrameRateHandler>(inst => inst.OnTargetFrameRateChanged(value), includeHubScene: true);
	}
	protected virtual void OnDataLocalizationChanged(string value)
	{
		AC_EventCommunication.SendMessage<IAC_CommonSetting_LocalizationHandler>(inst => inst.OnLocalizationChanged(value), includeHubScene: true);
	}
	protected virtual void OnDataQualityChanged(string value)
	{
		AC_EventCommunication.SendMessage<IAC_CommonSetting_QualityHandler>(inst => inst.OnQualityChanged(value), includeHubScene: true);
	}
	protected virtual void OnDataProcessPriorityChanged(string value)
	{
		AC_EventCommunication.SendMessage<IAC_CommonSetting_ProcessPriorityHandler>(inst => inst.OnProcessPriorityChanged(value), includeHubScene: true);
	}
	#endregion

	#region Callback
	public virtual void OnModInit(Scene scene, AC_AliveCursor aliveCursor)
	{
		//通知AC是否激活（PS：在程序启动且被禁用时会被调用；运行期间不会无法切换Item）
		AC_EventCommunication.SendMessage<IAC_CommonSetting_IsAliveCursorActiveHandler>(inst => inst.OnIsAliveCursorActiveChanged(Config.generalSetting_IsAliveCursorActive.Value), includeHubScene: true);
	}

	public virtual void OnModDeinit(Scene scene, AC_AliveCursor aliveCursor)
	{
	}
	#endregion
}

#region Define
/// <summary>
/// 通用设置
///
/// PS：
/// 1.通过存储BasicData的类结构，用户可在程序运行前，修改字段的范围
/// </summary>
[System.Serializable]
public class AC_CommonSettingConfigInfo : AC_SettingConfigInfoBase<AC_CommonSettingConfigInfo>
{
	[Header("Cursor Appearance")]
	public BoolData cursorAppearance_IsHideOnTextInput = new BoolData(false);//输入模式时隐藏(适用于文字编辑、代码编辑等人群)
	public FloatData cursorAppearance_CursorSize = new FloatData(1, new DataOption_Float(true, 0.1f, 10f));

	[Header("Cursor State")]
	public BoolData cursorState_StandBy_IsActive = new BoolData(true);//待命：如果超过一定时间没有鼠标点击事件则隐藏（便于用户看电影或滚轮浏览网页）
	public FloatData cursorState_StandBy_DelayTime = new FloatData(3, new DataOption_Float(true, 1, 120));
	public BoolData cursorState_Bored_IsActive = new BoolData(true);//闲逛
	public FloatData cursorState_Bored_DelayTime = new FloatData(60, new DataOption_Float(true, 3, 3600));
	public FloatData cursorState_Bored_Depth = new FloatData(5, new DataOption_Float(true, 0, 10));

	[Header("General Setting")]//PS:(以下Option不能用枚举代替，因为可能会有变化（如多语言））

	//Todo:将General Setting放到不受IsAliveCursorActive影响的UI区域中
	public BoolData generalSetting_IsAliveCursorActive = new BoolData(true);//启用AC
	public BoolData generalSetting_IsRunAtStartUp = new BoolData(false);//系统运行时自动启动
	public BoolData generalSetting_IsSupportMultiDisplay = new BoolData(true);//支持多屏幕
	public BoolData generalSetting_IsVSyncActive = new BoolData(false);//垂直同步（打开可以减少电脑发热现象；低刷新率的用户关闭以增加流畅度 ）
	public IntData generalSetting_TargetFrameRate = new IntData(90, new DataOption_Int(true, 60, 360));//垂直同步关闭后的默认帧率（设置为120可以增加流畅度，有需要的可以自行设置）（Todo：暴露在UI中）
	public StringData generalSetting_Localization = new StringData("English");
	public StringData generalSetting_Quality = new StringData("Ultra");
	public StringData generalSetting_ProcessPriority = new StringData("High");

	public AC_CommonSettingConfigInfo() { }
}
#endregion