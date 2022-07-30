using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using Threeyes.Data;
using UnityEngine;
public class AC_CommonSettingManagerSimulator : AC_CommonSettingManagerBase<AC_CommonSettingManagerSimulator>
{
	[Space]
	[InfoBox(
	"-Press the following key to change setting value:\r\n" +
	"'-' -> Shrink cursor\r\n" +
	"'=' ->Enlarge cursor")]
	public string dummyString;//Use this to make NaughtyAttributes work

	void Start()
	{
		Init(false);//模拟被调用
	}

	bool lastIsHideOnTextInput;
	float lastCursorSize;

	bool lastStandBy_Active;
	float lastStandBy_DelayTime;

	bool lastBored_Active;
	float lastBored_DelayTime;
	float lastBored_Depth;

	bool lastIsActiveAliveCursor;
	public override void Init(bool isFirstInit)
	{
		base.Init(isFirstInit);
		lastCursorSize = Config.cursorAppearance_CursorSize.Value;
		lastIsActiveAliveCursor = Config.notifySetting_IsActiveAliveCursor.Value;
	}

	private void Update()
	{
		if (!hasInit)
			return;

		///KeyChangeValue:
		///#Change cursor size by -/=
		if (Input.GetKeyDown(KeyCode.Minus))
		{
			Config.cursorAppearance_CursorSize.Value -= 0.5f;
		}
		if (Input.GetKeyDown(KeyCode.Equals))
		{
			Config.cursorAppearance_CursorSize.Value += 0.5f;
		}


		NotifyValueIfChanged(Config.cursorAppearance_IsHideOnTextInput, ref lastIsHideOnTextInput);
		NotifyValueIfChanged(Config.cursorAppearance_CursorSize, ref lastCursorSize);

		NotifyValueIfChanged(Config.cursorState_StandBy_IsActive, ref lastStandBy_Active);
		NotifyValueIfChanged(Config.cursorState_StandBy_DelayTime, ref lastStandBy_DelayTime);

		NotifyValueIfChanged(Config.cursorState_Bored_IsActive, ref lastBored_Active);
		NotifyValueIfChanged(Config.cursorState_Bored_DelayTime, ref lastBored_DelayTime);
		NotifyValueIfChanged(Config.cursorState_Bored_Depth, ref lastBored_Depth);

		NotifyValueIfChanged(Config.notifySetting_IsActiveAliveCursor, ref lastIsActiveAliveCursor);
	}

	static void NotifyValueIfChanged<TValue>(BasicData<TValue> basicData, ref TValue lastValue)
	{
		if (!lastValue.Equals(basicData.Value))
		{
			basicData.NotifyValueChanged();
			lastValue = basicData.Value;
		}
	}
}
