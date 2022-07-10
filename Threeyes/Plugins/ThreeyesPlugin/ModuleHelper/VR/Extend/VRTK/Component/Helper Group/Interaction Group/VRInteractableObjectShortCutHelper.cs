#if USE_VRTK

#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Events;
using VRTK;
using VRTK.Highlighters;
/// <summary>
/// 通过快捷键调用Use等方法，便于调试
/// 键位：方法
/// U:Use
/// </summary>
public class VRInteractableObjectShortCutHelper : MonoBehaviour
{
    public UnityEvent onUse;
    public CheckAvaliableType checkAvaliable = CheckAvaliableType.HightLight;//该快捷键可用的条件

    VRTK_InteractableObject interactableObject;

    private void OnEnable()
    {
        interactableObject = GetComponent<VRTK_InteractableObject>();
    }

    private void Update()
    {
        //Use
        if (Input.GetKeyDown(KeyCode.U))
        {
            if (IsObjActive)
            {
                onUse.Invoke();
                TryExecute<VRTK_InteractUse>(
                       (interactUse) =>
                       {
                           interactableObject.StartUsing(interactUse);
                       }
                       );
            }
        }

        if (Input.GetKeyUp(KeyCode.U))
        {
            if (IsObjActive)
            {
                TryExecute<VRTK_InteractUse>(
                    (interactUse) =>
                    {
                        interactableObject.StopUsing(interactUse);
                    }
                    );
            }
        }

        //Todo:Grab还有Bug
        ////Grab
        //if (Input.GetKeyDown(KeyCode.G))
        //{
        //    TryExecute<VRTK_InteractGrab>(
        //        (interactGrab) =>
        //        {
        //            interactableObject.Grabbed(interactGrab);
        //        }
        //        );
        //}
        //if (Input.GetKeyUp(KeyCode.G))
        //{
        //    TryExecute<VRTK_InteractGrab>(
        //        (interactGrab) =>
        //        {
        //            interactableObject.Ungrabbed(interactGrab);
        //        }
        //        );
        //}

    }

    /// <summary>
    /// 检测该物体是否已激活，即能够使用
    /// </summary>
    /// <returns></returns>
    public bool IsObjActive
    {
        get
        {
            bool isActive = false;

            switch (checkAvaliable)
            {
                case CheckAvaliableType.Null:
                    isActive = true; break;
                case CheckAvaliableType.HightLight:
                    VRTK_BaseHighlighter baseHighlighter = GetComponent<VRTK_BaseHighlighter>();//因为首次调用VRTK_InteractableObject的ToggleHighlight方法后，才会初始化并增加VRTK_BaseHighlighter的子类，因此可以通过有无该脚本，判断是否激活
                    isActive = baseHighlighter != null;
                    break;
                case CheckAvaliableType.Useable:
                    isActive = interactableObject.isUsable;
                    break;
            }
            return isActive;
        }
    }
    public void TryExecute<T>(UnityAction<T> func) where T : Component
    {
        if (VRInterface.defaultControllerEvent)
        {
            VRInterface.defaultControllerEvent.TryGetComponentAndExecute<T>(func);
        }

    }

    public enum CheckAvaliableType
    {
        Null = -1,//不加检测，适合于物体激活时使用
        HightLight,
        Useable = 10,//是否可用
    }

}

#endif

#endif