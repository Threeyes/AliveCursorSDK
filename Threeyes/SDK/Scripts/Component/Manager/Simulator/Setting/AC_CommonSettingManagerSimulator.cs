using System.Collections;
using System.Collections.Generic;
using Threeyes.Data;
using UnityEngine;
public class AC_CommonSettingManagerSimulator : AC_CommonSettingManagerBase<AC_CommonSettingManagerSimulator>
{
	void Start()
	{
		Init(false);//模拟被调用
	}

	bool HideOnTextInput;
	float lastCursorSize;

	bool StandBy_Active;
	float StandBy_DelayTime;

	bool Bored_Active;
	float Bored_DelayTime;
	float Bored_Depth;

	bool lastActiveAliveCursor;
	public override void Init(bool isFirstInit)
	{
		base.Init(isFirstInit);
		lastCursorSize = Config.cursorAppearance_CursorSize.Value;
		lastActiveAliveCursor = Config.notifySetting_IsActiveAliveCursor.Value;
	}

	private void Update()
	{
		if (!hasInit)
			return;

		NotifyValueIfChanged(Config.cursorAppearance_IsHideOnTextInput, ref HideOnTextInput);
		NotifyValueIfChanged(Config.cursorAppearance_CursorSize, ref lastCursorSize);

		NotifyValueIfChanged(Config.cursorState_StandBy_IsActive, ref StandBy_Active);
		NotifyValueIfChanged(Config.cursorState_StandBy_DelayTime, ref StandBy_DelayTime);

		NotifyValueIfChanged(Config.cursorState_Bored_IsActive, ref Bored_Active);
		NotifyValueIfChanged(Config.cursorState_Bored_DelayTime, ref Bored_DelayTime);
		NotifyValueIfChanged(Config.cursorState_Bored_Depth, ref Bored_Depth);

		NotifyValueIfChanged(Config.notifySetting_IsActiveAliveCursor, ref lastActiveAliveCursor);
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
