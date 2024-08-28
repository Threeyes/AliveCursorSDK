using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Threeyes.Core
{
    public static class UITool
    {
        static EventSystem curEventSystem { get { if (!_curEventSystem) _curEventSystem = EventSystem.current; return _curEventSystem; } }
        static EventSystem _curEventSystem;
        public static System.Func<bool> OverrideIsHoveringUI;

        /// <summary>
        /// Warning:
        /// -如果相机有 PhysicsRaycaster 组件，那么光标移动到Collider时，IsPointerOverGameObject会返回true。解决办法是为PhysicsRaycaster设置特定的EventMask（https://forum.unity.com/threads/how-to-detect-if-mouse-is-over-ui.1025533/#post-7616668）
        /// </summary>
        /// <returns></returns>
        public static bool IsHoveringUI()
        {
            if (OverrideIsHoveringUI != null)//适用于：屏幕有需要忽略的OnScreenUI（如虚拟摇杆） 。后续还可以通过提供需要忽略UI的特征来进行排除
                return OverrideIsHoveringUI.Invoke();

            if (curEventSystem)
            {
                return curEventSystem.IsPointerOverGameObject();
            }
            return false;
        }

        /// <summary>
        /// 是否正在选中特定UI
        /// 
        /// ToUpdate：
        /// -提供可选参数（是否判断Layer，可以保证与PhysicsRaycaster共存）。参考：https://forum.unity.com/threads/how-to-detect-if-mouse-is-over-ui.1025533/#post-8227341
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


        /// <summary>
        /// 获取某一点的顶层UI
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool GetFirstUIHit(Vector3 screenPosition, out GameObject result)
        {
            if (!curEventSystem)
            {
                result = null;
                return false;
            }
            List<RaycastResult> results = new List<RaycastResult>();
            curEventSystem.RaycastAll(new PointerEventData(curEventSystem) { position = screenPosition }, results);//Warning：EventSystem.current.currentSelectedGameObject无法正常返回当前UI，因此要通过RaycastAll检测

            if (results.Count > 0)
            {
                result = results[0].gameObject;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }
    }
}
