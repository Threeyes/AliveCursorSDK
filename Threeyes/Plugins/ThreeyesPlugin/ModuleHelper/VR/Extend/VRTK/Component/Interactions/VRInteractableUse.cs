using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if USE_VRTK
using VRTK;

/// <summary>
/// PS： 因为直接监听VRTK_InteractableObject的use事件没用，所以只能继承
/// </summary>
[System.Obsolete("UseVRTK_InteractableObject_UnityEvent instead")]
public class VRInteractableUse : VRTK_InteractableObject
#else
class VRInteractableUse : MonoBehaviour
#endif
{
    public UnityEvent onUse;

#if USE_VRTK
    public override void StartUsing(VRTK_InteractUse usingObject)
    {
        base.StartUsing(usingObject);
        Use();
    }
    private void Use()
    {
        onUse.Invoke();
    }

    //Debug
#if UNITY_EDITOR
    bool isToggleHighLight = false;

#if UNITY_2018
    public override void ToggleHighlight(bool toggle)
    {
        isToggleHighLight = toggle;
        base.ToggleHighlight(toggle);
    }
#endif

    protected override void Update()
    {
        base.Update();
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (isToggleHighLight)//高亮状态(即该物体当前需要操作），点击按键使用
            {
                print("Use!");
                Use();
                ToggleHighlight(false);//防止重复使用
            }
        }
    }

#endif

#endif

}
//#endif