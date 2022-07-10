using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 通用体属性配属性配置// </summary>
public abstract class UnityColliderEventReceiverBase<T> : ColliderEventReceiverBase<T>
        where T : ColliderEventReceiverBase<T>
{
    public ColliderCheckType colliderCheckType = ColliderCheckType.DontCheck;

    protected virtual void Reset()
    {
        //数值自定初始化（如设置默认的colliderCheckType）
    }

    protected override bool IsValiable(Collider other)
    {
        if (!IsActive)
            return false;

        bool isValid = false;
        switch (colliderCheckType)
        {
            case ColliderCheckType.DontCheck:
                return false;
            case ColliderCheckType.All:
                return true;
            case ColliderCheckType.Name:
                return other.gameObject.name == specificName;
            case ColliderCheckType.Tag:
                return other.gameObject.tag == specificName;
            case ColliderCheckType.Layer:
                return LayerMask.LayerToName(other.gameObject.layer) == specificName;
            case ColliderCheckType.Script:
                other.transform.ForEachParent(
                    (c) =>
                    {
                        if (c.GetComponent(specificName) != null)
                            isValid = true;
                    });
                break;
        }
        return isValid;
    }
}

/// <summary>
/// 碰撞体检测区域（初略细分）
/// </summary>
public enum ColliderCheckType
{
    DontCheck = -2,//都不行，常用于延后处理触碰
    All = -1,//只要是碰撞体都可
    Name = 100,
    Tag = 101,
    Layer = 102,
    Script = 110,//脚本名（可以是特定不通用的脚本）
}
