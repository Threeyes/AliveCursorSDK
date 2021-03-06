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
/// ????????????
///
/// PS???
/// 1.????????????BasicData??????????????????????????????????????????????????????????????????
/// </summary>
[System.Serializable]
public class AC_CommonSettingConfigInfo : AC_SettingConfigInfoBase<AC_CommonSettingConfigInfo>
{
	[Header("Cursor Appearance")]
	public BoolData cursorAppearance_IsHideOnTextInput = new BoolData(false);//?????????????????????(?????????????????????????????????????????????)
	public FloatData cursorAppearance_CursorSize = new FloatData(1, new DataOption_Float(true, 0.1f, 10f));

	[Header("Cursor State")]
	public BoolData cursorState_StandBy_IsActive = new BoolData(true);//??????????????????????????????????????????????????????????????????????????????????????????????????????????????????
	public FloatData cursorState_StandBy_DelayTime = new FloatData(3, new DataOption_Float(true, 1, 120));
	public BoolData cursorState_Bored_IsActive = new BoolData(true);//??????
	public FloatData cursorState_Bored_DelayTime = new FloatData(60, new DataOption_Float(true, 3, 3600));
	public FloatData cursorState_Bored_Depth = new FloatData(5, new DataOption_Float(true, 0, 10));

	[Header("General Setting")]//PS:(??????Option?????????????????????????????????????????????????????????????????????
	public StringData generalSetting_Localization = new StringData("English");
	public StringData generalSetting_Quality = new StringData("Ultra");
	public StringData generalSetting_ProcessPriority = new StringData("High");
	public BoolData generalSetting_IsActiveVSync = new BoolData(false);//?????????????????????????????????????????????????????????????????????????????????????????????????????? ???
	public IntData generalSetting_TargetFrameRate = new IntData(90, new DataOption_Int(true, 60, 360));//????????????????????????????????????????????????120????????????????????????????????????????????????????????????Todo????????????UI??????

	[Header("Notify Setting")]
	public BoolData notifySetting_IsActiveAliveCursor = new BoolData(true);//??????AC
	public BoolData notifySetting_IsSupportMultiDisplay = new BoolData(true);//???????????????

	public AC_CommonSettingConfigInfo() { }
}
#endregion