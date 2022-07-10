#if USE_VRTK
using VRTK;
#endif

using UnityEngine;
using System.Collections;
using System;
using Threeyes.Coroutine;

/// <summary>
/// PS：参考032_Controller_CustomControllerModel 中的Controller_Hand，原Demo是直接禁用了  SteamVR-Controller (right)-Model 物体
/// PS2:对应物体需要放在 LeftController/RightController 下
/// 使用手部的碰撞体：
/// 1、碰撞体设置为Trigger，Layer为IgnoreRaycase。注意父物体不能自己加Rigidbody，因为Left/RightController自己会加
/// 2、isUseCustomCollider为True
/// </summary>
public class VRHand : MonoBehaviour
{
    public static VRHand leftHand;
    public static VRHand rightHand;

    public BoolEvent onSwitchMode;
    public BoolEvent onGrab;
    public Hands hand = Hands.Right;
    public Rigidbody rigGrabAttachPoint;//手的抓取点
    public bool isDefaultShowCustom = false;//默认显示自定义模型
    public bool isUseCustomCollider = false;//使用自己的碰撞体
    bool isUsingCustom = false;

    private void Awake()
    {
        if (hand == Hands.Left)
            leftHand = this;
        else
            rightHand = this;
    }

#if USE_VRTK

    //Cache
    VRTK_ControllerReference controllerReference { get { return VRTK_ControllerReference.GetControllerReference(transform.parent.gameObject); } }
    public VRTK_ControllerEvents controllerEvents;
    VRTK_InteractGrab vRTK_InteractGrab;
    Rigidbody rigOriginGrabAttachPoint;//Stream VR 自带模型的抓取点
    GameObject gosteamVR_RenderModel;

    protected GameObject RenderModelCollider
    {
        get
        {
            if (!renderModelCollider)
            {
                Transform tfControllerCollider = transform.parent.Find("[VRTK][AUTOGEN][Controller][CollidersContainer]");
                if (tfControllerCollider)
                    renderModelCollider = tfControllerCollider.gameObject;
            }
            return renderModelCollider;
        }
    }

    private GameObject renderModelCollider;


    private void Start()
    {
        vRTK_InteractGrab = GetComponentInParent<VRTK_InteractGrab>();
        if (vRTK_InteractGrab)
        {
            vRTK_InteractGrab.GrabButtonPressed += DoGrabOn;
            vRTK_InteractGrab.GrabButtonReleased += DoGrabOff;
            rigOriginGrabAttachPoint = vRTK_InteractGrab.controllerAttachPoint;
        }

        if (GetComponentInParent<VRTK_InteractUse>())
        {
            GetComponentInParent<VRTK_InteractUse>().UseButtonPressed += DoUseOn;
            GetComponentInParent<VRTK_InteractUse>().UseButtonReleased += DoUseOff;
        }

        //#检测方法1
        ////参考VRTK_ControllerTooltips，监听Controller的显示/隐藏事件
        ////Bug:只能进入一次，在重置场景后不调用
        //controllerEvents = (controllerEvents != null ? controllerEvents : GetComponentInParent<VRTK_ControllerEvents>());
        //if (controllerEvents != null)
        //{
        //    controllerEvents.ControllerEnabled += DoControllerEnabled;
        //    controllerEvents.ControllerVisible += DoControllerVisible;
        //    controllerEvents.ControllerHidden += DoControllerInvisible;
        //}

        //#检测方法2
        ShowThis(false);
        coroutineSearchControllerModel = CoroutineManager.StartCoroutineEx(IEDetectModelLoadState());//检测模型是否被加载
    }

    Coroutine coroutineSearchControllerModel;
    protected virtual void OnDestroy()
    {
        if (coroutineSearchControllerModel != null)
            CoroutineManager.StopCoroutineEx(coroutineSearchControllerModel);
        //if (controllerEvents != null)
        //{
        //    controllerEvents.ControllerEnabled -= DoControllerEnabled;
        //    controllerEvents.ControllerVisible -= DoControllerVisible;
        //    controllerEvents.ControllerHidden -= DoControllerInvisible;
        //}
    }

    protected virtual void DoControllerEnabled(object sender, ControllerInteractionEventArgs e)
    {
        StartCoroutine(IEDetectModelLoadState());
        Debug.Log(hand + " DoControllerEnabled!");
    }
    protected virtual void DoControllerVisible(object sender, ControllerInteractionEventArgs e)
    {
        //e.controllerReference
        Debug.Log(hand + " DoControllerVisible!");
    }

    protected virtual void DoControllerInvisible(object sender, ControllerInteractionEventArgs e)
    {
        //Hide
        Debug.Log(hand + " DoControllerInvisible!");
    }

    IEnumerator IEDetectModelLoadState()
    {
        //找到手柄对应的根物体，并等待其加载模型完毕
        //try
        //{
        while (true)
        {
            //找到手柄对应的根物体，并等待其加载模型完毕
            if (transform.parent && transform.parent.parent && transform.parent.parent.Find("Model"))
            {
                gosteamVR_RenderModel = transform.parent.parent.Find("Model").gameObject;
                break;
            }
            else
                yield return null;
        }
        //}
        //catch (Exception e)
        //{
        //    yield break;
        //}

        if (!gosteamVR_RenderModel)
        {
            Debug.LogError(transform.name + " 找不到对应的Model!");
            yield break;
        }

        Transform tfModel = gosteamVR_RenderModel.transform;
        int waitCount = 0;
        if (tfModel)
        {
            while (tfModel.childCount == 0)
            {
                yield return null;
                waitCount++;
            }
        }

        //初始化显隐
        if (isDefaultShowCustom)
            SwitchHandMode(true);
        else
            ShowThis(false);
    }

    [ContextMenu("SwitchHandMode")]
    public void SwitchHandMode()
    {
        SwitchHandMode(!isUsingCustom);
    }

    public void SwitchHandMode(bool isUseCutom)
    {
        isUsingCustom = isUseCutom;
        GameObject activeObj = isUseCutom ? gameObject : gosteamVR_RenderModel;
        if (gosteamVR_RenderModel)
        {
            //如果使用自定义碰撞体，则隐藏默认的碰撞体
            if (isUseCustomCollider)
                if (RenderModelCollider)
                    RenderModelCollider.SetActive(!isUseCutom);

            gosteamVR_RenderModel.SetActive(!isUseCutom);
            //更新引用，让Controller Model支持隐藏等功能
            if (hand == Hands.Left)
                VRTK_SDKManager.instance.loadedSetup.modelAliasLeftController = activeObj;
            else
                VRTK_SDKManager.instance.loadedSetup.modelAliasRightController = activeObj;
        }
        ShowThis(isUseCutom);

        if (vRTK_InteractGrab)
            vRTK_InteractGrab.controllerAttachPoint = isUseCutom ? rigGrabAttachPoint : rigOriginGrabAttachPoint;

        onSwitchMode.Invoke(isUseCutom);
    }

    void ShowThis(bool isShow)
    {
        gameObject.SetActive(isShow);
    }

    /// <summary>
    /// 显示可视部分，适用于抓取物体
    /// </summary>
    /// <param name="isShow"></param>
    public void ShowRenderer(bool isShow)
    {
        if (isUsingCustom)
        {
            transform.ForEachChildComponent<Renderer>(r => r.enabled = isShow);
        }
        else
        {
            if (gosteamVR_RenderModel)
                gosteamVR_RenderModel.SetActive(isShow);
            //gosteamVR_RenderModel.transform.ForEachChildComponent<Renderer>(r => r.enabled = isShow);
        }
    }

    private void InversePosition(Transform givenTransform)
    {
        givenTransform.localPosition = new Vector3(givenTransform.localPosition.x * -1, givenTransform.localPosition.y, givenTransform.localPosition.z);
        givenTransform.localEulerAngles = new Vector3(givenTransform.localEulerAngles.x, givenTransform.localEulerAngles.y * -1, givenTransform.localEulerAngles.z);
    }

    private void DoGrabOn(object sender, ControllerInteractionEventArgs e)
    {
        onGrab.Invoke(true);
    }

    private void DoGrabOff(object sender, ControllerInteractionEventArgs e)
    {
        onGrab.Invoke(false);
    }

    private void DoUseOn(object sender, ControllerInteractionEventArgs e)
    {

    }

    private void DoUseOff(object sender, ControllerInteractionEventArgs e)
    {

    }

#endif

    public enum Hands
    {
        Right,
        Left
    }
}
