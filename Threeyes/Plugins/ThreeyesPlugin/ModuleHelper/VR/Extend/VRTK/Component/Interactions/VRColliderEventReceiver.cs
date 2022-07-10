using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 用于静态Collider的触发
/// 
/// 技巧：
///     1.因为碰撞事件只能传递给附着的Rigidbody或Collider，因此可以通过在事件中调用其他VRColliderHelper的对应方法，从而完成事件的传递，还能进行事件的分发和筛选
/// </summary>
public class VRColliderEventReceiver : ColliderEventReceiverBase<VRColliderEventReceiver>
{
    [Header("碰撞检测类型")]
    public VRColliderCheckType colliderCheckType = VRColliderCheckType.VRBody_Head;

#if USE_VRTK&&VRTK_VERSION_3_1_0_OR_NEWER
    public VRTK.VRTK_PolicyList targetPolicyList;
#endif


    protected override bool IsValiable(Collider other)
    {
        if (!IsActive)
            return false;

        bool isValid = false;
        switch (colliderCheckType)
        {
            case VRColliderCheckType.DontCheck:
                return false;
            case VRColliderCheckType.All://不检测
                return true;
            case VRColliderCheckType.VRWholeBody:
                isValid = VRInterface.IsVRPlayerWholeBody(other); break;
            case VRColliderCheckType.VRBody_Head:
                isValid = VRInterface.IsVRPlayerHead(other); break;
            case VRColliderCheckType.VRBody_LowerBody:
                isValid = VRInterface.IsVRPlayerLowerBody(other); break;

            case VRColliderCheckType.VRController:
                isValid = VRInterface.IsVRController(other); break;
            case VRColliderCheckType.VRLeftController:
                isValid = VRInterface.IsVRLeftController(other); break;
            case VRColliderCheckType.VRRightController:
                isValid = VRInterface.IsVRRightController(other); break;

#if USE_VRTK&&VRTK_VERSION_3_1_0_OR_NEWER
            case VRColliderCheckType.PolicyList://使用VRTK的部分通用组件
                {
                    isValid = !VRTK.VRTK_PolicyList.Check(other.gameObject, targetPolicyList); //Warning: Check 的作用是 check if a game object should be ignored 
                }
                break;
#endif
            case VRColliderCheckType.Name:
                if (other.attachedRigidbody)
                    return other.attachedRigidbody.gameObject.name == specificName;
                else
                    return other.gameObject.name == specificName;
            case VRColliderCheckType.Layer:
                return LayerMask.LayerToName(other.gameObject.layer) == specificName;
            case VRColliderCheckType.Script:
                other.transform.ForEachParent(
                    (c) =>
                    {
                        if (c.GetComponent(specificName) != null)
                            isValid = true;
                    });
                break;
            default:
                Debug.LogWarning(name + " 未实现switch判断!"); break;
        }
        return isValid;
    }

    bool IsColliderCheckTypeBody()
    {
        return colliderCheckType == VRColliderCheckType.VRWholeBody;
    }

    bool IsColliderCheckTypePolicyList()
    {
        return colliderCheckType == VRColliderCheckType.PolicyList;
    }
}

/// <summary>
/// 碰撞体检测区域（初略细分）
/// </summary>
public enum VRColliderCheckType
{
    DontCheck = -2,//都不行，常用于延后处理触碰
    All = -1,//只要是碰撞体都可
    VRBody_Head = 0,//头(旧款程序，可能都设置为Controller)
    VRBody_LowerBody = 1,//下半身
    VRWholeBody = 2,//全身（头+下半身，不包括手柄）

    VRController = 10,
    VRLeftController = 11,
    VRRightController = 12,

    PolicyList = 20,
    Name = 100,
    Tag = 101,
    Layer = 102,
    Script = 110,//脚本名（可以是特定不通用的脚本）
}
