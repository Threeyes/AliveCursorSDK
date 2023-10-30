using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Threeyes.SpawnPoint
{
    /// <summary>
    /// 自定义的SpawnPoint
    /// </summary>
    public class SpawnPointGroup_Custom : SpawnPointGroupBase
    {

        public List<Transform> listSpawnPoint = new List<Transform>();//自行摆放的生成点        
        public LoopType loopType = LoopType.InOrder;

        public override Pose spawnPose
        {
            get
            {
                if (listSpawnPoint.Count == 0)
                {
                    Debug.LogError("Element count is 0!");
                    return new Pose(transform.position, transform.rotation);
                }

                Transform randomTarget = listSpawnPoint.GetNewElement(loopType, ref curSpawnPointIndex, ref isLastYoYoIncrease);
                return new Pose(randomTarget.position, randomTarget.rotation);
            }
        }
        //Runtime
        int curSpawnPointIndex = -1;//初始化为-1，方便获取首值
        bool isLastYoYoIncrease = true;




        #region Editor Method
#if UNITY_EDITOR
        float gizmoSphereRadius = 0.06f;
        protected override void OnDrawGizmosSelected()
        {
            foreach (Transform tf in listSpawnPoint)
            {
                Gizmos.color = gizmoColor;
                Gizmos.DrawSphere(tf.position, gizmoSphereRadius * tf.lossyScale.x);//Draw IcoSphere's shape
            }
        }
#endif
        #endregion
    }
}