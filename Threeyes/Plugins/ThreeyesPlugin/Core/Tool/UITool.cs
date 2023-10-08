using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public static class UITool
{
  static  EventSystem curEventSystem { get { return EventSystem.current; } }

    /// <summary>
    /// Warning:
    /// -如果相机有PhysicsRaycaster组件，那么光标移动到Collider时，IsPointerOverGameObject会返回true。解决办法是为PhysicsRaycaster设置特定的EventMask（https://forum.unity.com/threads/how-to-detect-if-mouse-is-over-ui.1025533/#post-7616668）
    /// </summary>
    /// <returns></returns>
    public static bool IsHoveringUI()
    {
        if (curEventSystem)
        {
            return curEventSystem.IsPointerOverGameObject();
        }
        return false;
    }

    /// <summary>
    /// 是否正在选中特定UI
    /// </summary>
    /// <param name="goUIElement"></param>
    /// <returns></returns>
    public static bool IsHoveringUI(GameObject goUIElement)
    {
        if (curEventSystem)
        {
            return curEventSystem.currentSelectedGameObject == goUIElement;
        }
        return false;
    }

    /// <summary>
    /// 是否选中了InputField（输入状态）
    /// 
    /// PS:
    /// -InputSystem有效
    /// </summary>
    /// <param name="goUIElement"></param>
    /// <returns></returns>
    public static bool IsFocusingInputfield()
    {
        if (IsHoveringUI())
        {
            GameObject curGO = curEventSystem.currentSelectedGameObject;
            if (!curGO)
                return false;
            InputField inputField = curGO.GetComponent<InputField>();
            return inputField && inputField.isFocused;
        }
        return false;
    }
}