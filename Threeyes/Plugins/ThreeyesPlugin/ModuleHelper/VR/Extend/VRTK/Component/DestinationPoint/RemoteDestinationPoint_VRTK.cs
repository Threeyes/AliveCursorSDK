#if USE_VRTK
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRTK;
using Threeyes.Coroutine;

/// <summary>
/// 可以远程设置 VRRig 的位置，相比默认的VRTK_DestinationPoint，忽略了Raycast
/// 注意：为了避免传送失败，要WayPoint里设置：isShowOnAwake，让该物体和目标物体激活，还需要提前把两点之间的碰撞体去掉
/// 
/// 场景运行时自动传送的路标点的相应设置：
/// 1.isTeleportOnGameStart=true
/// 2.isAlignToDP=true;
/// 5.该路标朝向指定的目标
/// </summary>
public partial class RemoteDestinationPoint : VRTK_DestinationPoint
{
    #region  Partial Method

    protected void AddListenerFunc()
    {
        //DestinationMarkerSet += DoDestinationMarkerSet;//VRTK已自行监听并调用，因此不需要监听
    }
    protected void RemoveListenerFunc()
    {
        //DestinationMarkerSet -= DoDestinationMarkerSet;//VRTK已自行监听并调用，因此不需要监听
    }

    static void TeleportFunc(Transform target, Vector3 destinationPosition, Quaternion? destinationRotation = null)
    {
        VRTK_BasicTeleport basicTeleport = VRTK_ObjectCache.registeredTeleporters.FirstOrDefault();
        if (basicTeleport)
            basicTeleport.Teleport(target, destinationPosition, destinationRotation, true);
    }

    #endregion

    protected override void DisablePoint()
    {
        //修改：避免出现未初始化的问题
        if (pointCollider)
        {
            pointCollider.enabled = false;
        }
        else
        {
            CreateColliderIfRequired();
        }

        ToggleObject(lockedCursorObject, false);
        ToggleObject(defaultCursorObject, false);
        ToggleObject(hoverCursorObject, false);
        OnDestinationPointDisabled();
    }

    //ps:这些涉及不通用类型的方法声明在各自的分部类中
    protected override void DoDestinationMarkerSet(object sender, DestinationMarkerEventArgs e)
    {
        DoDestinationMarkerSetFunc(e.raycastHit.transform, e.raycastHit.point, () => BeginSetPos(this, e.raycastHit.point));
        //base.DoDestinationMarkerSet(sender, e);//使用上面的方法替代传送功能
    }

    #region Wasted
    ///// <summary>
    ///// 针对使用VRTK自带路标的传送组件（淘汰，建议统一使用RemoteDestinationPoint代替）
    ///// </summary>
    ///// <param name="destinationPoint"></param>
    //public static void ManualTeleport_VRTK(object destinationPoint)
    //{
    //    //Ps:如果路标点未首次激活，调用DisablePoint时会报错，只要先激活（按F调用FinishMission）即可，不是大问题。
    //    //原因如下：
    //    //①：VRTK_DestinationPoint组件 -Awake函数会调用VRTK_SDKManager.instance.AddBehaviourToToggleOnLoadedSetupChange(this);
    //    //②该方法会在场景变换的时候将VRTK_DestinationPoint.enable=false，导致OnEnable没有调用。只有在VRTK用协程初始化完毕后才重新激活： VRTK_SDKManager-（FinishSDKSetupLoading）-ToggleBehaviours
    //    VRTK_DestinationPoint vRTK_DestinationPoint = destinationPoint as VRTK_DestinationPoint;
    //    if (!vRTK_DestinationPoint)
    //    {
    //        Debug.LogError("类型错误！");
    //        return;
    //    }
    //    Debug.LogError("请使用 RemoteDestinationPoint 代替该组件！");
    //    currentDestinationPoint = vRTK_DestinationPoint;
    //    if (vRTK_DestinationPoint.snapToPoint)
    //    {
    //        if (!vRTK_DestinationPoint.destinationLocation)
    //        {
    //            Debug.LogWarning("destinationLocation 为空！可能是路标点还没激活并初始化！");
    //            return;
    //        }
    //        Teleport(vRTK_DestinationPoint.destinationLocation.position, target: vRTK_DestinationPoint.destinationLocation);//不使用协程传送
    //        vRTK_DestinationPoint.Invoke("DisablePoint", 0);//隐藏该节点
    //    }
    //}
    #endregion
}
#endif