using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Threeyes.Config;
using System;

namespace Threeyes.SpawnPoint
{
    /// <summary>
    /// ToUpdate：改为只针对单个ISpawnPointGroup
    /// </summary>
    public class SpawnPointProvider : ConfigurableComponentBase<SOSpawnPointProviderConfig, SpawnPointProvider.ConfigInfo>
    {
        public Transform tfSpawnPointParent;//Parent of all SpawnPoint

        //Runtime
        public List<ISpawnPointGroup> listSpawnPointGroup = new List<ISpawnPointGroup>();

        private void Awake()
        {
            listSpawnPointGroup = tfSpawnPointParent.GetComponentsInChildren<ISpawnPointGroup>().ToList();
        }

        int curSpawnPointIndex = -1;//初始化为-1，方便获取首值
        bool isLastYoYoIncrease = true;
        float lastSpawnTime = 0;

        /// <summary>
        /// ToUpdate：
        /// -只要不是Random，就应该是穷举每个SpawnPoint可提供的点
        /// -增加FallbackSpawnPoint点，方便超出数量后存放
        /// </summary>
        /// <returns></returns>
        public ISpawnPointGroup GetNewSpawnPoint()
        {
            if (Time.time - lastSpawnTime < Config.spawnIntervalTime)
                return null;

            ISpawnPointGroup newSpawnPoint = listSpawnPointGroup.GetNewElement(Config.loopType, ref curSpawnPointIndex, ref isLastYoYoIncrease);

            //ISpawnPointGroup curSpawnPoint = listSpawnPointGroup.IsIndexValid(curSpawnPointIndex) ? listSpawnPointGroup[curSpawnPointIndex] : null;//初次使用，可能为空
            //switch (Config.loopType)
            //{
            //    case LoopType.Random:
            //        newSpawnPoint = listSpawnPointGroup.GetNewRandom(curSpawnPoint);
            //        break;
            //    case LoopType.InOrder:
            //        newSpawnPoint = listSpawnPointGroup.GetNextByIndex(curSpawnPointIndex);
            //        break;
            //    case LoopType.Yoyo:
            //        if (listSpawnPointGroup.Count <= 1)
            //            newSpawnPoint = listSpawnPointGroup.FirstOrDefault();
            //        else
            //        {
            //            int nextIndex = curSpawnPointIndex;
            //            if (isLastYoYoIncrease)
            //            {
            //                nextIndex = curSpawnPointIndex.GetDeltaIndex(+1, listSpawnPointGroup.Count);
            //                if (nextIndex == listSpawnPointGroup.Count - 1)
            //                    isLastYoYoIncrease = false;
            //            }
            //            else
            //            {
            //                nextIndex = curSpawnPointIndex.GetDeltaIndex(-1, listSpawnPointGroup.Count);
            //                if (nextIndex == 0)
            //                    isLastYoYoIncrease = true;
            //            }
            //            newSpawnPoint = listSpawnPointGroup[nextIndex];
            //        }
            //        break;
            //}
            //if (newSpawnPoint != null)
            //{
            //    curSpawnPointIndex = listSpawnPointGroup.IndexOf(newSpawnPoint);
            //}

            lastSpawnTime = Time.time;
            return newSpawnPoint;
        }


        #region Define
        [Serializable]
        public class ConfigInfo : SerializableDataBase
        {
            public LoopType loopType = LoopType.Random;
            public float spawnIntervalTime = 0;//How long to provide next spawn info , 0 means instantly

        }
        #endregion
    }

    /// <summary>
    /// Provide info for spawn point info
    /// 
    /// 【Todo】：重命名为SpawnPointGroup，因为其功能类似与LayoutGroup
    /// </summary>
    public interface ISpawnPointGroup
    {
        ///ToAdd：
        ///-capacity：代表该组件的容量，用于SpawnPointProvider穷举每个ISpawnPoint。如果无限则为Infinity，如果是程序化则返回程序化数量值，也可以是由一个bool值确定是否由用户设定上限
        Pose spawnPose { get; }
    }

    public abstract class SpawnPointGroupBase : MonoBehaviour, ISpawnPointGroup
    {
        public abstract Pose spawnPose { get; }

        #region Editor Method
#if UNITY_EDITOR
        protected Color gizmoColor = new Color(0f, 1f, 1f, 0.5f);//Cyan
        /// <summary>
        /// Gizmos are drawn only when the object is selected. 
        /// </summary>
        protected virtual void OnDrawGizmosSelected() { }
#endif
        #endregion
    }

    public enum LoopType
    {
        Random,
        InOrder,
        Yoyo
    }

    public static class SpawnPointExtension
    {
        /// <summary>
        /// 根据SpawnLoopType，返回下一个元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listSpawnPointGroup"></param>
        /// <param name="loopType"></param>
        /// <param name="curIndex"></param>
        /// <param name="isLastYoYoIncrease">用于缓存上次Yoyo的方向</param>
        /// <returns></returns>
        public static T GetNewElement<T>(this List<T> listSpawnPointGroup, LoopType loopType, ref int curIndex, ref bool isLastYoYoIncrease)
        {
            T newSpawnPoint = default(T);
            T curSpawnPoint = listSpawnPointGroup.IsIndexValid(curIndex) ? listSpawnPointGroup[curIndex] : default(T);//初次使用，可能为空
            switch (loopType)
            {
                case LoopType.Random:
                    newSpawnPoint = listSpawnPointGroup.GetNewRandom(curSpawnPoint);
                    break;
                case LoopType.InOrder:
                    newSpawnPoint = listSpawnPointGroup.GetNextByIndex(curIndex);
                    break;
                case LoopType.Yoyo:
                    if (listSpawnPointGroup.Count <= 1)
                        newSpawnPoint = listSpawnPointGroup.FirstOrDefault();
                    else
                    {
                        int nextIndex = curIndex;
                        if (isLastYoYoIncrease)
                        {
                            nextIndex = curIndex.GetDeltaIndex(+1, listSpawnPointGroup.Count);
                            if (nextIndex == listSpawnPointGroup.Count - 1)
                                isLastYoYoIncrease = false;
                        }
                        else
                        {
                            nextIndex = curIndex.GetDeltaIndex(-1, listSpawnPointGroup.Count);
                            if (nextIndex == 0)
                                isLastYoYoIncrease = true;
                        }
                        newSpawnPoint = listSpawnPointGroup[nextIndex];
                    }
                    break;
            }

            if (newSpawnPoint != null)
            {
                curIndex = listSpawnPointGroup.IndexOf(newSpawnPoint);
            }
            return newSpawnPoint;
        }
    }
}