#if USE_VIU
using UnityEngine;
using HTC.UnityPlugin.Pointer3D;
using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.VRModuleManagement;
using System;
using System.Collections;
using UnityEngine.Events;
using Threeyes.Coroutine;

public partial class RemoteDestinationPoint : MonoBehaviour
     , IPointer3DPressEnterHandler
    , IPointer3DPressExitHandler
{
    public static event UnityAction<Transform, Pointer3DEventData> DestinationMarkerSet;
    static Coroutine coroutineTeleport;//当前缓存的传送协程

#region  Partial Method

    protected void AddListenerFunc()
    {
        DestinationMarkerSet += DoDestinationMarkerSet;
        actionBeforeTeleport += () => HeadsetFade(true);
        actionAfterTeleport += () => HeadsetFade(false);
    }
    protected void RemoveListenerFunc()
    {
        DestinationMarkerSet -= DoDestinationMarkerSet;
        actionBeforeTeleport -= () => HeadsetFade(true);
        actionAfterTeleport -= () => HeadsetFade(false);
    }

    protected void AlterSetPosFunc(bool isInvokeEvent = true)
    {
        //避免多次传送
        if (coroutineTeleport.NotNull())
        {
            CoroutineManager.StopCoroutineEx(coroutineTeleport);
        }
        coroutineTeleport = CoroutineManager.StartCoroutineEx(IEAlterSetPos(isInvokeEvent));
    }

    IEnumerator IEAlterSetPos(bool isInvokeEvent = true)
    {
        //等待硬件更新位置
        yield return new WaitForSeconds(0.1f);
        if (this)//防止场景切换导致物体被销毁
            AlterSetPos(isInvokeEvent);
    }

    void HeadsetFade(bool isFade)
    {
        VRInterface.Instance.HeadsetFadeFunc(isFade);//屏幕变暗
    }

#endregion


    public void OnPointer3DPressEnter(Pointer3DEventData eventData)
    {
        Debug.LogWarning("OnPointer3DPressEnter");
        VivePointerEventData viveEventData;
        if (eventData.TryGetViveButtonEventData(out viveEventData))
        {

            //if (viveEventData.GetPress() && viveEventData.viveButton == ControllerButton.Pad)//如果点击TouchPad，就执行传送操作
            if (viveEventData.GetPress())//如果按下任意按键，就激活Point
            {
                //ToggleCursor(sender, false);
                EnablePoint();
            }
        }
    }

    public void OnPointer3DPressExit(Pointer3DEventData eventData)
    {
        Debug.LogWarning("OnPointer3DPressExit");
        VivePointerEventData viveEventData;
        if (eventData.TryGetViveButtonEventData(out viveEventData))
        {
            //if (viveEventData.GetPressUp() && viveEventData.viveButton == ControllerButton.Pad)//如果点击TouchPad，就执行传送操作
            if (viveEventData.GetPressUp())//如果抬起任意按键，就执行传送操作
            {
                DestinationMarkerSet.Invoke(transform, eventData);
            }
            else
            {
                ResetPoint();
            }
        }
    }

    void DoDestinationMarkerSet(Transform tfTeleportTo, Pointer3DEventData eventData)//类似VRTK的DoDestinationMarkerSet
    {
        UnityAction actSetPos =
            () =>
            {
                Vector3? positionOverride = null;
                if (!snapToPoint)//使用射线指定的点作为传送终点
                {
                    var hitResult = eventData.pointerCurrentRaycast;
                    if (!hitResult.isValid) { return; }// check if hit something

                    positionOverride = hitResult.worldPosition;
                }
                SetPos(positionOverride);
            };

        DoDestinationMarkerSetFunc(tfTeleportTo, actionOnSetPosOverride: actSetPos);
    }
}

#endif
