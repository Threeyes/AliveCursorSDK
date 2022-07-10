using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// 碰撞体检测
/// </summary>
public class ColliderEventReceiverBase<T> : MonoBehaviour
    where T : ColliderEventReceiverBase<T>
{
    public bool IsActive { get { return isActive; } set { isActive = value; } }
    public bool isActive = true;
    public bool isLogOnValiable = false;
    public bool isInvokeOncePerItem = false;//针对每个物体，只调用一次进入事件
    public List<T> listReceiver = new List<T>();//接收和筛选


    [Space]
    public ColliderEvent onTriggerEnter;
    public ColliderEvent onTriggerExit;
    public CollisionEvent onCollisionEnter;
    public CollisionEvent onCollisionExit;
    public FloatEvent onCollisionForceEnter;//碰撞的力
    public FloatEvent onCollisionForceExit;//碰撞的力

    [Header("指定碰撞检测")]
    public string specificName;//ColliderCheckType中指定的名字

    /// <summary>
    /// 是否达到条件
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    protected virtual bool IsValiable(Collider other)
    {
        return false;
    }

    #region Physics Event

    //缓存已经作用的物体
    protected List<object> listCacheTriggerObj = new List<object>();
    protected List<object> listCacheCollisionObj = new List<object>();
    protected List<object> listCacheCollisionForceObj = new List<object>();

    public virtual void OnTriggerEnter(Collider other)
    {
        TryInvoke(other, onTriggerEnter, other, ref listCacheTriggerObj, true);

        SendEvent((c) => c.OnTriggerEnter(other));
    }
    public virtual void OnTriggerExit(Collider other)
    {
        TryInvoke(other, onTriggerExit, other, ref listCacheTriggerObj, false);

        SendEvent((c) => c.OnTriggerExit(other));
    }

    //Ps：Sleep的刚体可能不会调用次方法
    public virtual void OnCollisionEnter(Collision collision)
    {
        TryInvoke(collision.collider, onCollisionEnter, collision, ref listCacheCollisionObj, true);
        TryInvoke(collision.collider, onCollisionForceEnter, collision.relativeVelocity.magnitude, ref listCacheCollisionForceObj, true);

        SendEvent((c) => c.OnCollisionEnter(collision));
    }

    public virtual void OnCollisionExit(Collision collision)
    {
        TryInvoke(collision.collider, onCollisionExit, collision, ref listCacheCollisionObj, false);
        TryInvoke(collision.collider, onCollisionForceExit, collision.relativeVelocity.magnitude, ref listCacheCollisionForceObj, false);

        SendEvent((c) => c.OnCollisionExit(collision));
    }

    protected void TryInvoke<TParam>(Collider other, UnityEvent<TParam> unityEvent, TParam param, ref List<object> listCache, bool isSaveOrRemove, UnityAction actExtra = null)
    {
        //Todo:传入一个特定的cachelist
        bool isValiable = IsValiable(other);

#if UNITY_EDITOR
        if (isLogOnValiable)
            Debug.Log(other.name + "  " + isValiable);
#endif

        if (isValiable)
        {
            //Remove Emply Ref
            if (isInvokeOncePerItem)
            {
                int emptyObjCount = listCache.RemoveAll((o) => o.IsNull());//清除空元素
                Debug.Log("Empty Obj Count: " + emptyObjCount);

                if (isSaveOrRemove)
                {
                    if (listCache.Contains(other))
                    {
                        Debug.Log("该物体已经被调用！");
                        return;
                    }
                    else
                    {
                        listCache.Add(other);
                    }
                }
                else
                {
                    listCache.Remove(other);
                }
            }
            unityEvent.Invoke(param);
            if (actExtra != null)
                actExtra.Invoke();
        }
    }

    /// <summary>
    /// 给指定的子类广播消息
    /// </summary>
    /// <param name="action"></param>
    protected void SendEvent(UnityAction<T> action)
    {
        listReceiver.ForEach((c) =>
        {
            if (c)
                action(c);
        });
    }

    #endregion

}
