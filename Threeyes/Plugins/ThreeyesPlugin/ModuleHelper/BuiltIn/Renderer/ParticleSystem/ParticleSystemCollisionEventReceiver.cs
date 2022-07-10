using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// 功能：接收常见的粒子碰撞信息
/// </summary>
public class ParticleSystemCollisionEventReceiver : UnityColliderEventReceiverBase<ParticleSystemCollisionEventReceiver>
{
    public UnityEvent onParticleCollision;
    public GameObjectEvent onParticleCollisionGO;


    protected List<object> listCacheParticleCollisionObj = new List<object>();

    protected override void Reset()
    {
        colliderCheckType = ColliderCheckType.Name;
    }

    /// <summary>
    /// PS: 要接收粒子碰撞信息，需要Particle System - Collision - Send Collision Message 为 true
    /// </summary>
    /// <param name="other"></param>
    void OnParticleCollision(GameObject other)
    {
        if (!IsValiable(other.GetComponent<Collider>()))
            return;

        onParticleCollision.Invoke();
        onParticleCollisionGO.Invoke(other);
    }
}
