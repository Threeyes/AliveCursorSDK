#if USE_VRTK
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using VRTK;
[DefaultExecutionOrder(-400)]
public class SDK_InputSimulatorEx : SDK_InputSimulator
{
    //修改：
    [Header("自定义")]
    [Header("Movement")]
    public bool isFPSMode = false;//变为FPS模式
    public bool isMoveByInput = false;//使用Input进行位移输入(常用于安卓端的输入）
    public KeyCode moveUpward = KeyCode.Q;
    public KeyCode moveDownward = KeyCode.E;


    public bool isActiveOnPressing = false;//常用于UI拖拽

#if UNITY_2019_1_OR_NEWER

    Transform tfRightHand;
    VRTK_InteractUse rightInteractUse;

    /// <summary>
    /// 是否允许点击使用
    /// </summary>
    public bool IsActiveClickToUse
    {
        get
        {
            return isActiveClickToUse;
        }
        set
        {
            isActiveClickToUse = value;
        }
    }
    [SerializeField] protected bool isActiveClickToUse = true;
    private void Start()
    {
        rightInteractUse = VRTK_SDKManager.instance.scriptAliasRightController.GetComponent<VRTK_InteractUse>();

        if (isFPSMode)
        {
            SetMove();//设置为移动模式 

            leftHand.gameObject.SetActive(false);//隐藏左手
            rightHand.gameObject.SetActive(true);//保持右手显示

            //禁用右手的相关Renderer
            rightHand.ForEachChildTransform(
                (tfChild) =>
                {
                    MeshRenderer meshRenderer = tfChild.GetComponent<MeshRenderer>();
                    if (meshRenderer)
                        meshRenderer.enabled = false;
                }, isRecursive: true);
            tfRightHand = rightHand;

            var rightControllerEvent = VRTK_SDKManager.instance.scriptAliasRightController.GetComponent<VRTK_ControllerEvents>();


            //隐藏射线选择
            VRTK_StraightPointerRenderer vRTK_StraightPointerRenderer = rightControllerEvent.GetComponent<VRTK_StraightPointerRenderer>();
            if (vRTK_StraightPointerRenderer)
            {
                vRTK_StraightPointerRenderer.tracerVisibility = VRTK_BasePointerRenderer.VisibilityStates.AlwaysOff;
                vRTK_StraightPointerRenderer.cursorVisibility = VRTK_BasePointerRenderer.VisibilityStates.AlwaysOff;
            }


            VRTK_UIPointer vRTK_UIPointer = rightControllerEvent.GetComponent<VRTK_UIPointer>();
            if (vRTK_UIPointer)
            {
                vRTK_UIPointer.activationMode = VRTK_UIPointer.ActivationMethods.HoldButton;//按下才激活
                vRTK_UIPointer.clickMethod = VRTK_UIPointer.ClickMethods.ClickOnButtonDown;
            }
        }
    }

    protected override void UpdatePosition()
    {
        base.UpdatePosition();

        //ToAdd:上和下
        float moveMod = Time.deltaTime * playerMoveMultiplier * sprintMultiplier;
        if (Input.GetKey(moveUpward))
        {
            transform.Translate(transform.up * moveMod, Space.World);
        }
        if (Input.GetKey(moveDownward))
        {
            transform.Translate(-transform.up * moveMod, Space.World);
        }

        if (isMoveByInput)
            UpdatePositionByInput();
    }

    void UpdatePositionByInput()
    {
        //PS:Unity的CrossPlatformInputManager是个垃圾，只能用第三方插件实现
        float inputHorizontal = SimpleInput.GetAxis("Horizontal");
        float inputVertical = SimpleInput.GetAxis("Vertical");
        transform.Translate(transform.forward * inputVertical, Space.World);
        transform.Translate(transform.right * inputHorizontal, Space.World);
    }


    /// <summary>
    /// 重置Rig及相机的旋转及位置
    /// </summary>
    public void ResetAllRotation()
    {
        transform.localEulerAngles = Vector3.zero;
        transform.Find("Neck").localEulerAngles = Vector3.zero;
    }
    public void ResetCamRotation()
    {
        transform.Find("Neck").localEulerAngles = Vector3.zero;
    }


    [SerializeField] VRTK_InteractableObject curIO;


    new void Update()
    {
        base.Update();

        if (!IsActiveClickToUse)
            return;


        bool isHoveringUI = false;
        //检测PC模式下的StandaloneInputModule是否在UI上。该方法有效，但是可能会因为有多余背景UI导致误触发，暂时隐藏
        //if (EventSystem.current)
        //{
        //    StandaloneInputModule standaloneInputModule = EventSystem.current.GetComponent<StandaloneInputModule>();
        //    if (standaloneInputModule)
        //    {
        //        isHoveringUI = standaloneInputModule.IsPointerOverGameObject(PointerInputModule.kMouseLeftId);
        //        Debug.Log("isHoveringUI： " + isHoveringUI);
        //    }
        //}

        if (!isHoveringUI &&
            (Input.GetKeyDown(triggerAlias) || Input.GetKey(triggerAlias) && isActiveOnPressing))
        {
            Camera cameraMain = VRInterface.vrCamera;
            Ray screenRay = cameraMain.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            RaycastHit[] raycastHits = Physics.RaycastAll(screenRay);
            if (Physics.Raycast(screenRay, out hit))
            {
                tfRightHand.position = cameraMain.transform.position;//放在相机原点，避免手的偏移导致误触
                tfRightHand.LookAt(hit.point);


                if (Input.GetKeyDown(triggerAlias))//只有按下才调用
                {
                    //Debug.Log(hit.collider.gameObject.name + " " + hit.point);
                    VRTK_InteractableObject io = hit.collider.gameObject.GetComponent<VRTK_InteractableObject>();
                    if (io != null)
                    {
                        //Todo:改为主动调用
                        io.StartUsing(rightInteractUse);

                        //io.pointerActivatesUseAction = true;//只有点击的一瞬间才激活碰到的物体(Bug:会导致多次触发）
                        curIO = io;
                    }
                }
            }
        }

        if (Input.GetKeyUp(triggerAlias)/* && !UITool.IsHoveringUI()*/)//PS：检测UI无效，因为有多个EventSystem作用
        {
            if (curIO)
            {
                curIO.StopUsing(rightInteractUse);
                curIO = null;
            }
        }
    }
#endif

}
#endif