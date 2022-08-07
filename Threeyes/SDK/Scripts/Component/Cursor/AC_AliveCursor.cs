using UnityEngine;

/// <summary>
/// The Root Manager for mod item
/// </summary>
public class AC_AliveCursor : MonoBehaviour, IAC_CommonSetting_IsAliveCursorActiveHandler
{
	public static AC_AliveCursor Instance;
	public virtual void Init()
	{
		Instance = this;//Update Instance
	}

	public void OnIsAliveCursorActiveChanged(bool isActive)
	{
		SetActive(isActive);//显隐AC
	}

	public void SetActive(bool isActive)
	{
		gameObject.SetActive(isActive);
	}
}