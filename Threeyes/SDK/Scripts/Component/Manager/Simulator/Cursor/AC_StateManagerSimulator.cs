using UnityEngine;
using NaughtyAttributes;

public class AC_StateManagerSimulator : AC_StateManagerBase<AC_StateManagerSimulator>
{
	[Space]
	[InfoBox(
		"-Set [isDebugXXXNumberKeysChangeState] to true then Press the following number key to change state:\r\n" +
		"0->Toggle [isDebugIgnoreInput]\r\n" +
		"1->Enter	 2->Exit 3->Show 4->Hide 5->Working 6->StandBy 7->Bored")]
	public string dummyString;//Use this to make NaughtyAttributes work

	private void Update()
	{
		//PS：因为OnGUI同一帧会调用多次，而切换State只需要按下一瞬间，因此需要用Update
		//Number Keys change state
		if (!Input.anyKeyDown)
			return;

		if (isDebugTopNumberKeysChangeState)
		{
			foreach (var keyCode in debugDicTopNumberKey2State.Keys)
			{
				if (Input.GetKeyDown(keyCode))
				{
					DebugInputChangeStateFunc(keyCode, AC_KeyState.Down, debugDicTopNumberKey2State);
				}
			}
		}
		if (isDebugPadNumberKeysChangeState)
		{
			foreach (var keyCode in debugDicPadNumberKey2State.Keys)
			{
				if (Input.GetKeyDown(keyCode))
				{
					DebugInputChangeStateFunc(keyCode, AC_KeyState.Down, debugDicPadNumberKey2State);
				}
			}
		}
	}
}
