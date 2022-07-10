using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class InputFieldHelper : ComponentHelperBase<InputField>
{
    public KeyCode keyCodeSubmit = KeyCode.Return;

    public BoolEvent onDetectHasContent;//检测是否有内容
    private void Awake()
    {
        Comp.onValueChanged.AddListener(OnValueChanged);
        Comp.onEndEdit.AddListener(OnEndEdit);
    }

    private void OnValueChanged(string arg0)
    {
    }

    private void OnEndEdit(string arg0)
    {
        onDetectHasContent.Invoke(arg0.Length > 0);
    }
}
