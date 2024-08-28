using System.Collections;
using System.Collections.Generic;
using Threeyes.Core;
using UnityEngine;
namespace Threeyes.GameFramework
{
    /// <summary>
    /// 检测模型缩放事件，并更新Joint的ConnectedAnchor，支持Auto或Custom
    /// 
    /// PS：
    /// -原理：Unity不会自动更新，因此需要主动通知其刷新（https://forum.unity.com/threads/how-to-re-initialize-a-joint.1006907/）
    /// -autoConfigureConnectedAnchor：如果激活，则（If this is enabled, then the connectedAnchor property will be calculated automatically to match the global position of the anchor property. This is the default behavior. If this is disabled, you can configure the position of the connected anchor using the connectedAnchor property.）
    /// -如果是自定义ConnectedAnchor，则会该值是基于被链接刚体的【局部坐标】（If Joint.autoConfigureConnectedAnchor is not enabled, then this will be used to set the position of the anchor on the connected rigidbody. The position is given in 【local coordinates of the connected rigidbody】, or in world coordinates if there is no connected rigidbody.）（https://docs.unity3d.com/ScriptReference/Joint-connectedAnchor.html）
    /// </summary>
    public class JointSyncConnectedAnchorBySize : ComponentHelperBase<Joint>
    {
        //# Runtime
        Vector3 curSize;
        Transform cacheTf;//缓存Rigidbody的Transform
        Vector3 cacheConnectedAnchor = new Vector3(0, 0, 0);//默认的ConnectedAnchor值（该值基于局部坐标，但是需要通知刚体更新。因为不会有变化，所以不需要像RigidbodySyncCenterOfMassBySize一样需要初始化）

        private void Awake()
        {
            if (!Comp)
            {
                Debug.LogError($"{nameof(Comp)} not set!");
                return;
            }
            //缓存当前的尺寸以及对应的Anchor
            cacheTf = Comp.transform;
            cacheConnectedAnchor = Comp.connectedAnchor;
            curSize = cacheTf.lossyScale;
        }
        private void LateUpdate()
        {
            if (!(Comp && cacheTf))
                return;

            //尺寸变化：通知Joint进行更新
            if (cacheTf.lossyScale != curSize)
            {
                if (!isReactiving)//避免DelayReactiveAutioConfig将autoConfigureConnectedAnchor临时设置为false，导致意外进入自定义connectedAnchor的逻辑
                {
                    bool cacheAuto = Comp.autoConfigureConnectedAnchor;
                    if (cacheAuto)//自动计算：需要先取消，等待缩放完成后再重置
                    {
                        DelayReactiveAutioConfig();
                    }
                    else//自定义：重新设置值，让其主动更新
                    {
                        Comp.connectedAnchor = cacheConnectedAnchor;
                    }
                }

                curSize = transform.lossyScale;
            }
        }

        bool isReactiving = false;
        protected UnityEngine.Coroutine cacheEnum;

        void DelayReactiveAutioConfig()
        {
            TryStopCoroutine();
            cacheEnum = CoroutineManager.StartCoroutineEx(IEDelayReactiveAutioConfig());
        }
        protected virtual void TryStopCoroutine()
        {
            if (cacheEnum != null)
            {
                CoroutineManager.StopCoroutineEx(cacheEnum);
                cacheEnum = null;
            }
        }
        IEnumerator IEDelayReactiveAutioConfig()
        {
            ///PS：通过XR抓取并缩放时，会因为其在FixedUpdate的调用时机导致autoConfigureConnectedAnchor计算错误，因此需要等待一帧，等其位置正确后再激活自动计算
            Comp.autoConfigureConnectedAnchor = false;
            yield return null;//需要等待，因为
            Comp.autoConfigureConnectedAnchor = true;
        }
    }
}