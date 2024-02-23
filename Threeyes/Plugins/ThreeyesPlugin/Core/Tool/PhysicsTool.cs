using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threeyes.Core
{
    public static class PhysicsTool
    {
        static RaycastDistanceComparer raycastDistanceComparer = new RaycastDistanceComparer();

        /// <summary>
        /// 返回从近到远的物体
        /// 
        /// Warning:
        /// -Physics.RaycastAll返回的是未排序的物体（https://docs.unity3d.com/ScriptReference/Physics.RaycastAll.html）
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="maxDistance"></param>
        /// <param name="layerMask"></param>
        /// <param name="queryTriggerInteraction"></param>
        /// <returns></returns>
        public static RaycastHit[] RaycastAllOrdered(Ray ray, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance, layerMask, queryTriggerInteraction);

            Array.Sort(hits, raycastDistanceComparer);
            return hits;
        }

        /// <summary>
        /// 比较两个Hit的距离
        /// Ref：https://forum.unity.com/threads/are-raycasthit-arrays-returned-from-raycastall-in-proper-order.385131/#post-8695650
        /// </summary>
        public class RaycastDistanceComparer : IComparer<RaycastHit>
        {
            public int Compare(RaycastHit x, RaycastHit y)
            {
                //#Warning:以下代码会导致物体下的多个碰撞体距离错位，先注释！
                //var colliderInstanceIDComparison = x.colliderInstanceID.CompareTo(y.colliderInstanceID);
                //if (colliderInstanceIDComparison != 0)
                //    return colliderInstanceIDComparison;
                var distanceComparison = x.distance.CompareTo(y.distance);
                if (distanceComparison != 0)
                    return distanceComparison;
                return x.triangleIndex.CompareTo(y.triangleIndex);
            }
        }
    }
}