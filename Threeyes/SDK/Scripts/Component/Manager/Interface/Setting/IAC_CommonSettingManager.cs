/// <summary>
/// Common Setting
/// 
/// ToDo:暴露所有设置
/// </summary>
public interface IAC_CommonSettingManager : IAC_Manager_ModInitHandler
{
	/// <summary>
	/// Is hide when the cursor's appearance is input style?
	/// </summary>
	bool IsHideOnTextInput { get; }
	/// <summary>
	/// Cursor size
	/// </summary>
	float CursorSize { get; }

	/// <summary>
	/// Can enter StandBy state
	/// </summary>
	bool IsStandByActive { get; }
	/// <summary>
	/// How long to enter StandBy state
	/// </summary>
	float StandByDelayTime { get; }
	/// <summary>
	/// Can enter Bored state
	/// </summary>
	bool IsBoredActive { get; }
	/// <summary>
	/// How long to enter Bored state
	/// </summary>
	float BoredDelayTime { get; }
	/// <summary>
	/// Cursor max distance relate to camera on bored state
	/// </summary>
	float BoredDepth { get; }

	/// <summary>
	/// Current Localization
	/// </summary>
	string Localization { get; }
	/// <summary>
	/// Current QualitySettings Level
	/// </summary>
	string Quality { get; }
	/// <summary>
	/// Current Process Priority
	/// </summary>
	string ProcessPriority { get; }
	/// <summary>
	/// Is VSync actived?
	/// </summary>
	bool IsVSyncActive { get; }
	/// <summary>
	/// Target frame rate when vSync is off
	/// </summary>
	int TargetFrameRate { get; }

	/// <summary>
	/// Is AliveCursor actived?
	/// </summary>
	bool IsCursorActive { get; }
	/// <summary>
	/// Enable switch between display
	/// </summary>
	bool SupportMultiDisplay { get; }
}

public interface IAC_CommonSetting_IsHideOnTextInputHandler
{
	void OnIsHideOnTextInputChanged(bool isActive);
}
public interface IAC_CommonSetting_CursorSizeHandler
{
	void OnCursorSizeChanged(float value);
}

public interface IAC_CommonSetting_IsStandByActiveHandler
{
	void OnIsStandByActiveChanged(bool isActive);
}
public interface IAC_CommonSetting_StandByDelayTimeHandler
{
	void OnStandByDelayTimeChanged(float value);
}
public interface IAC_CommonSetting_IsBoredActiveHandler
{
	void OnIsBoredActiveChanged(bool isActive);
}
public interface IAC_CommonSetting_BoredDelayTimeHandler
{
	void OnBoredDelayTimeChanged(float value);
}
public interface IAC_CommonSetting_BoredDepthHandler
{
	void OnBoredDepthChanged(float value);
}

public interface IAC_CommonSetting_LocalizationHandler
{
	void OnLocalizationChanged(string value);
}
public interface IAC_CommonSetting_QualityHandler
{
	void OnQualityChanged(string value);
}
public interface IAC_CommonSetting_ProcessPriorityHandler
{
	void OnProcessPriorityChanged(string value);
}
public interface IAC_CommonSetting_IsVSyncActiveHandler
{
	void OnIsVSyncActiveChanged(bool isActive);
}
public interface IAC_CommonSetting_TargetFrameRateHandler
{
	void OnTargetFrameRateChanged(int value);
}

public interface IAC_CommonSetting_IsAliveCursorActiveHandler
{
	void OnIsAliveCursorActiveChanged(bool isActive);
}
public interface IAC_CommonSetting_IsSupportMultiDisplayHandler
{
	void OnIsSupportMultiDisplayChanged(bool isActive);
}