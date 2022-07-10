#if USE_VRTK
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using VRTK.Highlighters;

public class SetTouchHighlightColor : MonoBehaviour 
{
    public Transform target;//Default is itself
    public Color touchHighlightColor;
    public bool isIncludeChild = false;

    [ContextMenu("SetColor")]
    public void SetColor()
    {
        if (!target)
            target = transform;
        if (isIncludeChild)
            target.Recursive(SetColor);
        else
            SetColor(target);
    }

    void SetColor(Component comp)
    {
        if (comp.GetComponent<VRTK_InteractableObject>())
        {
                comp.GetComponent<VRTK_InteractableObject>().touchHighlightColor = touchHighlightColor;
            if (comp.GetComponent<VRTK_BaseHighlighter>())
                comp.GetComponent<VRTK_BaseHighlighter>().ResetHighlighter();//刷新
        }
    }
}
#endif