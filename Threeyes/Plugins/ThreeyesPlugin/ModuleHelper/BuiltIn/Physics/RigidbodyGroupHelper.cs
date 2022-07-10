using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 设置一组Rigidbody 的属性
/// </summary>
public class RigidbodyGroupHelper : ComponentGroupBase<Rigidbody>
{
    #region Editor

    [ContextMenu("SetColliderTriggerFalse")]
    public void SetColliderTriggerFalse()
    {
        SetColliderTrigger(false);
    }
    [ContextMenu("SetColliderTriggerTrue")]
    public void SetColliderTriggerTrue()
    {
        SetColliderTrigger(true);
    }

    [ContextMenu("ShowCollider")]
    public void ShowCollider()
    {
        EnableCollider(true);
    }
    [ContextMenu("HideCollider")]
    public void HideCollider()
    {
        EnableCollider(false);
    }
    [ContextMenu("Sleep")]
    public void Sleep()
    {
        RigidbodyAwake(false);
    }
    [ContextMenu("WakeUp")]
    public void WakeUp()
    {
        RigidbodyAwake(true);
    }

    [ContextMenu("SetKinectTrue")]
    public void SetKinematicTrue()
    {
        SetKinematic(true);
    }
    [ContextMenu("SetKinectFalse")]
    public void SetKinematicFalse()
    {
        SetKinematic(false);
    }
    #endregion

    public void SetColliderTrigger(bool isSet)
    {
        ForEachChildComponent<Collider>((c) => c.isTrigger = isSet);
    }

    public void SetKinematic(bool isSet)
    {
        ForEachChildComponent((r) => r.isKinematic = isSet);
    }

    public void DisableCollider()
    {
        EnableCollider(false);
    }
    public void EnableCollider(bool isEnable)
    {
        ForEachChildComponent<Collider>((c) => c.enabled = isEnable);
    }


    /// <summary>
    /// 变成静态碰撞体
    /// </summary>
    [ContextMenu("RemoveRigidbody")]
    public void RemoveRigidbody()
    {
        //ForEachChildComponent<Joint>((c) => DestroyImmediate(c));
        ForEachChildComponent((c) => DestroyImmediate(c));
        //ForEachChildComponent<Collider>((c) => DestroyImmediate(c));
    }

    [ContextMenu("RemoveCollider")]
    public void RemoveCollider()
    {
        ForEachChildComponent<Collider>((c) => DestroyImmediate(c));
    }

    public void RigidbodyAwake(bool isAwake)
    {
        ForEachChildComponent((r) =>
        {
            if (isAwake)
                r.WakeUp();
            else
                r.Sleep();
        }
        );
    }
}
