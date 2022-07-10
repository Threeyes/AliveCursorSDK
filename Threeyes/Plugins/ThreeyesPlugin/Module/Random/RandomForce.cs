using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// 添加随机的力
/// </summary>
public class RandomForce : RandomValueComponentBase<Random_Vector3, Vector3>
{
    public bool isIncludeChild;
    public ForceType forceType = ForceType.Force;
    public ForceMode forceMode = ForceMode.VelocityChange;
    public bool isRelative = false;
    public float removePhysicsAfterTime = -1f;//N秒后移除物理组件


    public override void OnSet()
    {
        switch (forceType)
        {
            case ForceType.Force:
                AddForce(); break;
            case ForceType.Torque:
                AddTorque(); break;
            case ForceType.Both:
                AddForce();
                AddTorque(); break;
        }
        base.OnSet();
    }

    [ContextMenu("Add Force")]
    public void AddForce()
    {
        if (isIncludeChild)
        {
            transform.Recursive(AddForce);
        }
        else
            AddForce(transform);
    }


    void AddForce(Component comp)
    {
        if (!comp.gameObject.activeInHierarchy)
            return;
        Rigidbody rig = comp.GetComponent<Rigidbody>();
        if (rig)
        {
            Vector3 power = randomValue.RandomValue;
            if (isRelative)
                rig.AddRelativeForce(power, forceMode);
            else
                rig.AddForce(power, forceMode);
        }
    }

    [ContextMenu("Add Torque")]
    public void AddTorque()
    {
        if (isIncludeChild)
        {
            transform.Recursive(AddTorque);
        }
        else
            AddTorque(transform);
    }
    void AddTorque(Component comp)
    {
        if (!comp.gameObject.activeInHierarchy)
            return;
        Rigidbody rig = comp.GetComponent<Rigidbody>();
        if (rig)
        {
            Vector3 power = randomValue.RandomValue;
            if (isRelative)
                rig.AddRelativeTorque(power, ForceMode.VelocityChange);
            else
                rig.AddTorque(power, ForceMode.VelocityChange);
        }
    }

    void RemoveRigidbody()
    {
        //受力为0时，自动删掉刚体
    }

    /// <summary>
    /// 需要设置的力类型
    /// </summary>
    public enum ForceType
    {
        Force,
        Torque,
        Both,
    }

}
