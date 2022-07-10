using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// 检测输入，然后调用相应事件
/// </summary>
public class InputEventListener : MonoBehaviour
{
    public List<KeyEventPair> listKeyEventPair = new List<KeyEventPair>();

    public static float ScrollWheel { get { return Input.mouseScrollDelta.y / 10; } }
    private void Update()
    {
        for (int i = 0; i < listKeyEventPair.Count; i++)
        {
            KeyEventPair keyEventPair = listKeyEventPair[i];
            if (keyEventPair.checkType == KeyEventPair.CheckType.Default)
            {
                if (keyEventPair.isCheckMouseScroll)
                {
                    if (Input.mouseScrollDelta != default(Vector2))
                    {
                        float vValue = ScrollWheel;
                        if (vValue > 0)
                            keyEventPair.onMouseScrollPositive.Invoke(vValue);
                        else
                            keyEventPair.onMouseScrollNegative.Invoke(vValue);
                        keyEventPair.onMouseScroll.Invoke(vValue);
                    }
                }
                keyEventPair.CheckKeyState();
            }
        }
    }

    public void SetKeyDown(int intKeyCode)
    {
        for (int i = 0; i < listKeyEventPair.Count; i++)
        {
            KeyEventPair keyEventPair = listKeyEventPair[i];
            if (keyEventPair.checkType == KeyEventPair.CheckType.Manual)
            {
                if (intKeyCode == (int)keyEventPair.keyCode)
                {
                    keyEventPair.OnKeyDown();
                }
            }
        }
    }
}

/// <summary>
/// 按键的状态
/// </summary>
public enum KeyInputState
{
    Null = -1,//无
    Down = 0,//刚按下
    Pressing,//按住
    Up,//抬起
}

[System.Serializable]
public class KeyEventPair
{
    public KeyEventPair()
    {
        //Bug:以下设置在List中无效，应该改为点击自己的按钮然后自动添加类实例，同时调用初始化代码
        mouseButtonIndex = -1;//设置默认值，避免默认监听鼠标事件
    }

    //Todo:增加一个监听类型：后台还是本地

    public CheckType checkType = CheckType.Default;
    public string description;

    [Header("Mouse")]
    [SerializeField]
    public int mouseButtonIndex = -1;
    public bool isCheckMouseScroll = false;

    [Header("Key")]
    public bool isDetectAnyKey = false;//检测任意鼠标或按键的状态（适合检测任意键）
    public KeyCode keyCode = KeyCode.None;
    public KeyCode debugKeyCode = KeyCode.None;//Editor下的替代键
    KeyCode ActuallyKeyCode
    {
        get
        {
#if UNITY_EDITOR
            if (Application.isEditor && debugKeyCode != KeyCode.None)
            {
                if (Input.GetKeyDown(debugKeyCode) || Input.GetKey(debugKeyCode) || Input.GetKeyUp(debugKeyCode))
                    Debug.Log("编辑器模式下使用" + debugKeyCode + "键 替代 " + keyCode + "键");
                return debugKeyCode;
            }
#endif

            return keyCode;
        }
    }

    [Header("Runtime")]
    public KeyInputState keyInputState = KeyInputState.Null;
    public float lastPressDownTime = 0;//按下瞬间的时间

    //按键或鼠标的 按下/持续/抬起 事件
    public BoolEvent onKeyDownUp;
    public UnityEvent onKeyDown;
    public UnityEvent onKey;
    public FloatEvent onKeyPressing;
    public UnityEvent onKeyUp;

    public FloatEvent onMouseScroll;
    public FloatEvent onMouseScrollPositive;
    public FloatEvent onMouseScrollNegative;



    public void CheckKeyState()
    {
        keyInputState = KeyInputState.Null;

        if (Input.GetKeyDown(ActuallyKeyCode) || GetMouseButton(mouseButtonIndex, KeyInputState.Down) || isDetectAnyKey && Input.anyKeyDown)
        {
            OnKeyDown();
            keyInputState = KeyInputState.Down;
        }
        if (Input.GetKey(ActuallyKeyCode) || GetMouseButton(mouseButtonIndex, KeyInputState.Pressing) || isDetectAnyKey && Input.anyKey)
        {
            keyInputState = KeyInputState.Pressing;
            OnKeyPressing();
        }
        if (Input.GetKeyUp(ActuallyKeyCode) || GetMouseButton(mouseButtonIndex, KeyInputState.Up))
        {
            keyInputState = KeyInputState.Up;
            OnKeyUp();
        }
    }



    public void OnKeyDown()
    {
        onKeyDownUp.Invoke(true);
        onKeyDown.Invoke();
        lastPressDownTime = Time.time;
    }
    public void OnKeyPressing()
    {
        onKey.Invoke();
        float pressedTime = Time.time - lastPressDownTime;
        onKeyPressing.Invoke(pressedTime);
    }
    public void OnKeyUp()
    {
        onKeyDownUp.Invoke(false);
        onKeyUp.Invoke();
    }

    bool GetMouseButton(int buttonIndex, KeyInputState inputState)
    {
        if (buttonIndex >= 0)
        {
            switch (inputState)
            {
                case KeyInputState.Down:
                    return Input.GetMouseButtonDown(buttonIndex);
                case KeyInputState.Pressing:
                    return Input.GetMouseButton(buttonIndex);
                case KeyInputState.Up:
                    return Input.GetMouseButtonUp(buttonIndex);
            }
        }
        return false;
    }

    /// <summary>
    /// 事件的调用方式
    /// </summary>
    [System.Serializable]
    public enum CheckType
    {
        Default,//默认的事件监听
        Manual,//手动调用方法
    }

}