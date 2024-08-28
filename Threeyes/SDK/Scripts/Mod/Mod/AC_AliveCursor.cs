using UnityEngine;
using Threeyes.GameFramework;
/// <summary>
/// The Root Manager for mod item
/// </summary>
public class AC_AliveCursor : ModEntry<AC_AliveCursor>
    , IAC_CommonSetting_IsAliveCursorActiveHandler
{
    #region Callback
    public void OnIsAliveCursorActiveChanged(bool isActive)
    {
        SetActive(isActive);//显隐AC
    }
    #endregion

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }
}