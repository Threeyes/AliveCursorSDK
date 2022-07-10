using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyHelper : ComponentHelperBase<Rigidbody>
{
    /// <summary>
    /// 从某个点开始爆炸
    /// </summary>
    [Header("AddExplosionForce")]
    public float explosionForce = 100;
    public float explosionRadius = 1;
    public void AddExplosionForce(Vector3 explosionPosition)
    {
        Comp.AddExplosionForce(explosionForce, explosionPosition, explosionRadius);
    }

    /// <summary>
    /// 沿指定方向及力度施力
    /// </summary>
    [Header("AddForce")]
    public Vector3 force;
    public void AddForce(Vector3 point)
    {
        Comp.AddForceAtPosition(force, point);
    }
}
