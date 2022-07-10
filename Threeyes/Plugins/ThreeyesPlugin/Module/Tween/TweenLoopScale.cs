#if USE_DOTween
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Loop Tween 缩放
/// </summary>
public class TweenLoopScale : GameObjectBase
{
    Vector3 startScale;
    public Vector2 scaleRange = new Vector2(1, 2);
    public float scaleSpeed = 1f;
    protected  void Awake()
    {
        startScale = tfThis.localScale;
        deltaPercent = scaleRange.x;
    }
    public float deltaPercent = 0;
    private void Update()
    {
        deltaPercent = Mathf.PingPong(Time.time * scaleSpeed, scaleRange.y - scaleRange.x);
        tfThis.localScale = startScale * (scaleRange.x + deltaPercent);
    }
}
#endif