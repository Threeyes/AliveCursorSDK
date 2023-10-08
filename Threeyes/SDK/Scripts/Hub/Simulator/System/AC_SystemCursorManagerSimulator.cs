using UnityEngine;
using NaughtyAttributes;
public class AC_SystemCursorManagerSimulator : AC_SystemCursorManagerBase<AC_SystemCursorManagerSimulator>
{
	[Space]
	[InfoBox(
		"-Press Left/Right Arrow key to loop through [No, ArrowCD]\r\n" +
		"-Press DownArrow key to switch between None and last valid type\r\n" +
		"-Press UpArrow key to toggle system cursor activation")]
	[SerializeField] protected bool isSystemCursorShowing_Simulator = true;
	[SerializeField] protected AC_SystemCursorAppearanceType curSCAType_Simulator = AC_SystemCursorAppearanceType.Arrow;//当前模拟的系统光标类型
	protected AC_SystemCursorAppearanceType lastSCAType_Simulator = AC_SystemCursorAppearanceType.Arrow;//上次模拟的系统光标类型（仅保存有效值）

	AC_SystemCursorAppearanceInfo curSystemCursorAppearanceInfo_Simulate;
	void Update()
	{
		//监听用户输入，左右键切换SystemCursorAppearanceType，上下切换Show/Hide
		//通过通过设置App光标，模拟控制SystemCursor的显隐
		//ToUpdate:如果curSystemCursorAppearanceInfo切换到IBeam，则根据设置，自动决定是否设置Texture

		//Press Left/Right Arrow key: loop through [No, ArrowCD]
		if (InputTool.GetKeyDown(KeyCode.RightArrow))//Add
		{
			if (curSCAType_Simulator == AC_SystemCursorAppearanceType.ArrowCD || curSCAType_Simulator == AC_SystemCursorAppearanceType.None)
				curSCAType_Simulator = AC_SystemCursorAppearanceType.No;
			else
				curSCAType_Simulator = (AC_SystemCursorAppearanceType)((int)curSCAType_Simulator << 1);
		}
		if (InputTool.GetKeyDown(KeyCode.LeftArrow))//Subtract
		{
			if (curSCAType_Simulator == AC_SystemCursorAppearanceType.No || curSCAType_Simulator == AC_SystemCursorAppearanceType.None)
				curSCAType_Simulator = AC_SystemCursorAppearanceType.ArrowCD;
			else
				curSCAType_Simulator = (AC_SystemCursorAppearanceType)((int)curSCAType_Simulator >> 1);
		}
		//Press DownArrow key: switch between None and last valid type
		if (InputTool.GetKeyDown(KeyCode.DownArrow))
		{
			curSCAType_Simulator = curSCAType_Simulator == AC_SystemCursorAppearanceType.None ? lastSCAType_Simulator : AC_SystemCursorAppearanceType.None;
		}

		//Press UpArrow key: toggle system cursor activation
		if (InputTool.GetKeyDown(KeyCode.UpArrow))
		{
			isSystemCursorShowing_Simulator = !isSystemCursorShowing_Simulator;
		}

		curSystemCursorAppearanceInfo_Simulate = new AC_SystemCursorAppearanceInfo() { systemCursorAppearanceType = curSCAType_Simulator };
		UpdateAppearance(isSystemCursorShowing_Simulator, curSystemCursorAppearanceInfo_Simulate);

		if (curSCAType_Simulator != AC_SystemCursorAppearanceType.None && curSCAType_Simulator != AC_SystemCursorAppearanceType.All)
			lastSCAType_Simulator = curSCAType_Simulator;


		//Simulate SystemCursor Behaviour
		bool isSystemCursorVisible = isSystemCursorShowing;
		if (!isSystemCursorShowing)
		{
			if (commonSetting_IsHideOnTextInput && curSystemCursorAppearanceInfo.systemCursorAppearanceType == AC_SystemCursorAppearanceType.IBeam)
			{
				isSystemCursorVisible = true;
			}
		}
		Cursor.visible = isSystemCursorVisible;
	}
}

