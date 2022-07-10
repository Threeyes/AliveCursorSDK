using System;
using Threeyes.Data;
using UnityEngine;

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
	public bool IsVSyncActive { get { return Config.generalSetting_IsActiveVSync.Value; } }
	public int TargetFrameRate { get { return Config.generalSetting_TargetFrameRate.Value; } }

	public bool IsCursorActive { get { return Config.notifySetting_IsActiveAliveCursor.Value; } }
	public bool SupportMultiDisplay { get { return Config.notifySetting_IsSupportMultiDisplay.Value; } }
	#endregion

	public override void InitEvent()
	{
		Config.cursorAppearance_IsHideOnTextInput.actionValueChanged += OnIsHideOnTextInputChanged;
		Config.cursorAppearance_CursorSize.actionValueChanged += OnCursorSizeChanged;

		Config.cursorState_StandBy_IsActive.actionValueChanged += OnIsStandByActiveChanged;
		Config.cursorState_StandBy_DelayTime.actionValueChanged += OnStandByDelayTimeChanged;
		Config.cursorState_Bored_IsActive.actionValueChanged += OnIsBoredActiveChanged;
		Config.cursorState_Bored_DelayTime.actionValueChanged += OnBoredDelayTimeChanged;
		Config.cursorState_Bored_Depth.actionValueChanged += OnBoredDepthChanged;

		Config.generalSetting_Localization.actionValueChanged += OnLocalizationChanged;
		Config.generalSetting_Quality.actionValueChanged += OnQualityChanged;
		Config.generalSetting_ProcessPriority.actionValueChanged += OnProcessPriorityChanged;
		Config.generalSetting_IsActiveVSync.actionValueChanged += OnIsVSyncChanged;
		Config.generalSetting_TargetFrameRate.actionValueChanged += OnTargetFrameRateChanged;

		Config.notifySetting_IsActiveAliveCursor.actionValueChanged += OnActiveAliveCursorChanged;
		Config.notifySetting_IsSupportMultiDisplay.actionValueChanged += OnIsSupportMultiDisplayChanged;
	}

	protected virtual void OnIsHideOnTextInputChanged(bool value)
	{
		AC_EventCommunication.SendMessage<IAC_CommonSetting_IsHideOnTextInputHandler>(inst => inst.OnIsHideOnTextInputChanged(value), includeHubScene: true);
	}
	protected virtual void OnCursorSizeChanged(float value)
	{
		AC_EventCommunication.SendMessage<IAC_CommonSetting_CursorSizeHandler>(inst => inst.OnCursorSizeChanged(value), includeHubScene: true);
	}

	protected virtual void OnIsStandByActiveChanged(bool value)
	{
		AC_EventCommunication.SendMessage<IAC_CommonSetting_IsStandByActiveHandler>(inst => inst.OnIsStandByActiveChanged(value), includeHubScene: true);
	}
	protected virtual void OnStandByDelayTimeChanged(float value)
	{
		AC_EventCommunication.SendMessage<IAC_CommonSetting_StandByDelayTimeHandler>(inst => inst.OnStandByDelayTimeChanged(value), includeHubScene: true);
	}
	protected virtual void OnIsBoredActiveChanged(bool value)
	{
		AC_EventCommunication.SendMessage<IAC_CommonSetting_IsBoredActiveHandler>(inst => inst.OnIsBoredActiveChanged(value), includeHubScene: true);
	}
	protected virtual void OnBoredDelayTimeChanged(float value)
	{
		AC_EventCommunication.SendMessage<IAC_CommonSetting_BoredDelayTimeHandler>(inst => inst.OnBoredDelayTimeChanged(value), includeHubScene: true);
	}
	protected virtual void OnBoredDepthChanged(float value)
	{
		AC_EventCommunication.SendMessage<IAC_CommonSetting_BoredDepthHandler>(inst => inst.OnBoredDepthChanged(value), includeHubScene: true);
	}

	protected virtual void OnLocalizationChanged(string value)
	{
		AC_EventCommunication.SendMessage<IAC_CommonSetting_LocalizationHandler>(inst => inst.OnLocalizationChanged(value), includeHubScene: true);
	}
	protected virtual void OnQualityChanged(string value)
	{
		AC_EventCommunication.SendMessage<IAC_CommonSetting_QualityHandler>(inst => inst.OnQualityChanged(value), includeHubScene: true);
	}
	protected virtual void OnProcessPriorityChanged(string value)
	{
		AC_EventCommunication.SendMessage<IAC_CommonSetting_ProcessPriorityHandler>(inst => inst.OnProcessPriorityChanged(value), includeHubScene: true);
	}
	protected virtual void OnIsVSyncChanged(bool isActive)
	{
		AC_EventCommunication.SendMessage<IAC_CommonSetting_IsVSyncActiveHandler>(inst => inst.OnIsVSyncActiveChanged(isActive), includeHubScene: true);
	}
	protected virtual void OnTargetFrameRateChanged(int value)
	{
		AC_EventCommunication.SendMessage<IAC_CommonSetting_TargetFrameRateHandler>(inst => inst.OnTargetFrameRateChanged(value), includeHubScene: true);
	}

	protected virtual void OnActiveAliveCursorChanged(bool value)
	{
		AC_EventCommunication.SendMessage<IAC_CommonSetting_IsCursorActiveHandler>(inst => inst.OnIsCursorActiveChanged(value), includeHubScene: true);
	}
	protected virtual void OnIsSupportMultiDisplayChanged(bool value)
	{
		AC_EventCommunication.SendMessage<IAC_CommonSetting_IsSupportMultiDisplayHandler>(inst => inst.OnIsSupportMultiDisplayChanged(value), includeHubScene: true);
	}
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
	public FloatData cursorAppearance_CursorSize = new FloatData(1, new DataOption_Float(true, 0.1f, 5f));

	[Header("Cursor State")]
	public BoolData cursorState_StandBy_IsActive = new BoolData(true);//待命：如果超过一定时间没有鼠标点击事件则隐藏（便于用户看电影或滚轮浏览网页）
	public FloatData cursorState_StandBy_DelayTime = new FloatData(3, new DataOption_Float(true, 1, 120));
	public BoolData cursorState_Bored_IsActive = new BoolData(true);//闲逛
	public FloatData cursorState_Bored_DelayTime = new FloatData(60, new DataOption_Float(true, 3, 3600));
	public FloatData cursorState_Bored_Depth = new FloatData(5, new DataOption_Float(true, 0, 10));

	[Header("General Setting")]
	public StringData generalSetting_Localization = new StringData("English");
	public StringData generalSetting_Quality = new StringData("Ultra");
	public StringData generalSetting_ProcessPriority = new StringData("High");
	public BoolData generalSetting_IsActiveVSync = new BoolData(false);//垂直同步（打开可以减少电脑发热现象。；低刷新率的用户关闭以增加流畅度 ）
	public IntData generalSetting_TargetFrameRate = new IntData(90, new DataOption_Int(true, 60, 360));//垂直同步关闭后的默认帧率（设置为120可以增加流畅度，有需要的可以自行设置）（Todo：暴露在UI中）

	[Header("Notify Setting")]
	public BoolData notifySetting_IsActiveAliveCursor = new BoolData(true);//启用AC
	public BoolData notifySetting_IsSupportMultiDisplay = new BoolData(true);//支持多屏幕

	public AC_CommonSettingConfigInfo() { }
}
#endregion