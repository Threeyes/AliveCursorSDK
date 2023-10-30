using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threeyes.SpawnPoint
{
    /// <summary>
    /// 提供方形区域或边界中的任意一点
    /// 
    /// -ToAdd:考虑物体缩放，增加类似BoxCollider的调整Handle
    /// </summary>
    public class SpawnPointGroup_Bounds : SpawnPointGroupBase
    {
        public Vector3 boundsSize = Vector3.one;

        public override Pose spawnPose
        {
            get
            {
                Vector3 newPointPos = transform.TransformPoint((boundsSize / 2).RandomRange());//返回Bounds中的某一点(计算方式为先计算出该物体局部坐标的随机位置，然后转为全局坐标)
                Quaternion newRotation = transform.rotation;//暂时先用该物体的旋转值，后续通过Enum来提供不同的计算方式
                return new Pose(newPointPos, newRotation);
            }
        }
    }
}