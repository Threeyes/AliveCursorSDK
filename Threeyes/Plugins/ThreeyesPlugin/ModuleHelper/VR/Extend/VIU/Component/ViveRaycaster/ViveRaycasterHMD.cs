#if USE_VIU
using HTC.UnityPlugin.Pointer3D;
using HTC.UnityPlugin.Utility;
using HTC.UnityPlugin.Vive;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using System.Collections;
using Threeyes.Coroutine;

/// <summary>
/// 头显发射射线，并且能与UI进行交互
/// 注意：
/// ——Pico系的设备，头显的Trigger按键无法监听，因此改用返回键KeyCode.Escape作为主按键
/// </summary>
public class ViveRaycasterHMD : ViveRaycaster
{
#region 以下定义无实际作用，仅用于占位

    [SerializeField]
    [FormerlySerializedAs("m_mouseBtnLeft")]
    [CustomOrderedEnum]
    private ControllerButton m_mouseButtonLeftCustom = ControllerButton.Trigger;
    [SerializeField]
    [FormerlySerializedAs("m_mouseBtnMiddle")]
    [CustomOrderedEnum]
    private ControllerButton m_mouseButtonMiddleCustom = ControllerButton.Grip;
    [SerializeField]
    [FormerlySerializedAs("m_mouseBtnRight")]
    [CustomOrderedEnum]
    private ControllerButton m_mouseButtonRightCustom = ControllerButton.Pad;

#endregion

    VivePointerEventDataCommon pedLeft;
    VivePointerEventDataCommon pedRight;
    VivePointerEventDataCommon pedMiddle;

    public KeyCode keyCodeSelect = KeyCode.Escape;//选中键的映射按键
    public KeyCode debugkeyCodeSelect = KeyCode.Return;//Editor下选中键的映射按键
    public BoolEvent onPressDownUp;

    protected override void Start()
    {
        //监听按键事件

        pedLeft = new VivePointerEventDataCommon(this, EventSystem.current, m_mouseButtonLeftCustom, PointerEventData.InputButton.Left);
        pedRight = new VivePointerEventDataCommon(this, EventSystem.current, m_mouseButtonRightCustom, PointerEventData.InputButton.Right);
        pedMiddle = new VivePointerEventDataCommon(this, EventSystem.current, mouseButtonMiddle, PointerEventData.InputButton.Middle);

        buttonEventDataList.AddRange(new List<Pointer3DEventData>() { pedLeft, pedRight, pedMiddle });
    }

#region Hover
    [Header("Hover")]
    public float hoverToClickTime = 2f;

    //Debug
    [Space]
    public bool isStartHoverCountDown = false;
    public bool isClicked = false;
    public GameObject currentTarget;
    public FloatEvent onCoundDownPercent;

    private float hoverUsedTime;
    protected EventSystem cachedEventSystem;
    protected Coroutine cacheEnum;

    public float HoverUsedTime
    {
        get
        {
            return hoverUsedTime;
        }
        set
        {
            hoverUsedTime = Mathf.Clamp(value, 0, hoverToClickTime); ;
            onCoundDownPercent.Invoke(hoverUsedTime / hoverToClickTime);
        }
    }

    protected override void Awake()
    {
        if (!cachedEventSystem)
        {
            cachedEventSystem = FindObjectOfType<EventSystem>();
        }
    }

    protected void TryStopCoroutine()
    {
        if (cacheEnum != null)
        {
            CoroutineManager.StopCoroutineEx(cacheEnum);
        }
    }

    void StartHoverClickCountDown()
    {
        TryStopCoroutine();
        cacheEnum = CoroutineManager.StartCoroutineEx(IEHoverClickCountDown());
    }

    IEnumerator IEHoverClickCountDown()
    {
        isStartHoverCountDown = true;
        float startTime = Time.time;

        while (Time.time - startTime < hoverToClickTime)
        {
            HoverUsedTime = Time.time - startTime;
            yield return null;
        }
        SetPressDown();//模拟点击
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        SetPressUp();
        //Todo：重置选择
        ResetHover();
        isClicked = true;
    }

    private void ResetHover()
    {
        isStartHoverCountDown = false;
        currentTarget = null;
        HoverUsedTime = 0;
    }

    Selectable GetCurrentHoverSelectable()
    {
        foreach (RaycastResult rayCastResult in sortedRaycastResults)
        {
            var go = rayCastResult.gameObject;
            if (go)
            {
                var selectable = go.GetComponentInParent<Selectable>();
                //selectable = go.GetComponent<Selectable>();
                if (selectable && selectable.interactable)
                    return selectable;
            }
        }
        return null;
    }

    private void DetectHoverState()
    {
        //检测是否凝视状态
        EventSystem eventSystem = EventSystem.current;
        BaseInputModule baseInputModule = eventSystem.GetComponent<BaseInputModule>();
        if (eventSystem)
        {
            Selectable selectable = GetCurrentHoverSelectable();
            //是否可选择
            if (selectable)
            {
                GameObject goCurHover = selectable.gameObject;
                //初始化
                if (currentTarget == null)
                {
                    currentTarget = goCurHover;
                    if (!isStartHoverCountDown && !isClicked)
                    {
                        StartHoverClickCountDown();
                    }
                }
                if (goCurHover == currentTarget)
                {

                }
                else
                {
                    TryStopCoroutine();
                    ResetHover();
                    isClicked = false;
                }
            }
            else//没选中
            {
                TryStopCoroutine();
                ResetHover();
                isClicked = false;
            }
        }
    }

#endregion

    float debugLastPressTime;
    private void Update()
    {
        DetectHoverState();

        //头显选中键
        if (Input.GetKeyDown(keyCodeSelect))
        {
            Debug.Log("Down 1");
            //停止UI凝视倒计时
            TryStopCoroutine();
            ResetHover();
            isClicked = true;
            SetPressDown();
        }
        if (Input.GetKey(keyCodeSelect))
            SetPress();

        if (Input.GetKeyUp(keyCodeSelect))
            SetPressUp();

#if UNITY_EDITOR

        //编辑器选中键
        if (Input.GetKeyDown(debugkeyCodeSelect) || Input.GetMouseButtonDown(0))
        {
            if (Time.time - debugLastPressTime > 0.5f)
            {
                Debug.Log("Down 1");
                //停止UI凝视倒计时
                TryStopCoroutine();
                ResetHover();
                isClicked = true;

                debugLastPressTime = Time.time;
                SetPressDown();
            }
        }

        if (Input.GetKey(debugkeyCodeSelect) || Input.GetMouseButton(0))
            SetPress();

        if (Input.GetKeyUp(debugkeyCodeSelect) || Input.GetMouseButtonUp(0))
            SetPressUp();

        ////以下用于测试头盔的有效按键
        //if (Input.anyKeyDown)
        //{
        //    foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
        //    {
        //        if (Input.GetKeyDown(keyCode))
        //        {
        //            foreach (string name in Input.GetJoystickNames())
        //            {
        //                Debug.LogError("Joystickname " + name);
        //            }

        //            Debug.Log("AnyKeyDown !" + keyCode.ToString());
        //            //SetPressDown();
        //            return;
        //        }
        //    }
        //}

        //for (int i = 0; i != 10; i++)
        //{
        //    if (HTC.UnityPlugin.VRModuleManagement.UnityEngineVRModule.GetUnityButton(i))
        //    {
        //        Debug.LogError("AnyKeyDown " + i);
        //    }
        //}


        //foreach (DeviceRole deviceRole in System.Enum.GetValues(typeof(DeviceRole)))
        //    foreach (ControllerButton keyCode in System.Enum.GetValues(typeof(ControllerButton)))
        //    {
        //        if (ViveInput.GetPressEx(deviceRole, keyCode))
        //        {
        //            Debug.LogError("ViveInput.GetPressEx " + keyCode);
        //        }
        //    }

        //if (Input.anyKeyDown)
        //{
        //    Debug.LogError("AnyKeyDown !" + Input.inputString);
        //}


        //原因：安卓屏蔽了事件在UnityPlayerActivity.java里面的onKeyDown和一些按键按下的事件无法监听到摇杆的轴键按下，
        ///https://blog.csdn.net/weixin_33712881/article/details/94499607
        //if (Input.GetKeyDown(KeyCode.JoystickButton0))//
        //{
        //    SetPressDown();
        //}
        //if (Input.GetKeyDown(KeyCode.JoystickButton0))//
        //{
        //    SetPressDown();
        //}

        //if (Input.GetKey(KeyCode.JoystickButton0))//
        //{
        //    SetPress();
        //}

        //if (Input.GetKeyUp(KeyCode.JoystickButton0))
        //    SetPressUp();


        //if (Input.GetMouseButtonDown(0))
        //{
        //    SetPressDown();
        //}
        //if (Input.GetMouseButton(0))
        //{
        //    SetPress();
        //}
        //if (Input.GetMouseButtonUp(0))
        //{
        //    SetPressUp();
        //}
#endif
    }

    /// <summary>
    /// PS:貌似只调用该方法无效,改为调用SetPress
    /// </summary>
    public void SetPressDown()
    {
        Debug.Log("SetPressDown");
        pedLeft.SetPressDown();
        onPressDownUp.Invoke(true);
    }

    public void SetPress()
    {
        pedLeft.SetPress();
    }
    public void SetPressUp()
    {
        pedLeft.SetPressUp();
        onPressDownUp.Invoke(false);
    }


    /// <summary>
    /// 便于自定义触发条件
    /// </summary>
    [System.Serializable]
    public class VivePointerEventDataCommon : VivePointerEventData
    {
        public VivePointerEventDataCommon(ViveRaycaster ownerRaycaster, EventSystem eventSystem, ControllerButton viveButton, InputButton mouseButton) : base(ownerRaycaster, eventSystem, viveButton, mouseButton)
        {

        }

        public bool IsPress = false;
        public bool IsPressDown = false;
        public bool IsPressUp = false;
        //Todo:离开一个物体后,ispressdown为false

#region Set

        public void SetPressDown()
        {
            IsPressDown = true;
            IsPress = true;
            IsPressUp = false;
        }

        public void SetPress()
        {
            IsPress = true;
            IsPressDown = true;
            IsPressUp = false;
        }

        public void SetPressUp()
        {
            IsPress = false;
            IsPressDown = false;
            IsPressUp = true;
        }

#endregion

#region Get

        public override bool GetPress()
        {
            return IsPress;
        }
        public override bool GetPressDown()
        {
            return IsPressDown;
        }
        public override bool GetPressUp()
        {
            return IsPressUp;
        }
#endregion
    }
}
#endif