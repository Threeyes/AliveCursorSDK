using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Threeyes.Config;
using System;
using Threeyes.Core;

namespace Threeyes.SpawnPoint
{
    /// <summary>
    /// ToUpdate：改为只针对单个ISpawnPointGroup
    /// -不太通用，可以先作为
    /// </summary>
    public class SpawnPointProvider : ConfigurableComponentBase<SOSpawnPointProviderConfig, SpawnPointProvider.ConfigInfo>
    {
        /// <summary>
        /// 最终的所有Group。
        /// Warning:
        /// -不能在Awake中初始化，因为此时Mod还未生成子物体。
        /// </summary>
        public List<ISpawnPointGroup> ListTargetSpawnPointGroup
        {
            get
            {
                if (listCustomSpawnPointGroup.Count == 0)
                    return listSpawnPointGroup.ConvertAll<ISpawnPointGroup>(s => s);

                List<ISpawnPointGroup> listTargetSpawnPointGroup = new List<ISpawnPointGroup>();
                listTargetSpawnPointGroup.AddRange(listSpawnPointGroup);
                listTargetSpawnPointGroup.AddRange(listCustomSpawnPointGroup);
                return listTargetSpawnPointGroup;
            }
        }

        public List<ISpawnPointGroup> listCustomSpawnPointGroup = new List<ISpawnPointGroup>();//通过接口实现的自定义类，可以在运行时添加
        public List<SpawnPointGroupBase> listSpawnPointGroup = new List<SpawnPointGroupBase>();//继承Unity组件接口的自定义类

        int curSpawnPointIndex = -1;//初始化为-1，方便获取首值
        bool isLastYoYoIncrease = true;
        float lastSpawnTime = 0;

        /// <summary>
        /// ToUpdate：
        /// -只要不是Random，就应该是穷举每个SpawnPoint可提供的点
        /// -增加FallbackSpawnPoint点，方便超出数量后存放
        /// </summary>
        /// <returns></returns>
        public ISpawnPointGroup GetNewSpawnPointGroup()
        {
            if (Time.time - lastSpawnTime < Config.spawnIntervalTime)
                return null;

            ISpawnPointGroup newSpawnPoint = ListTargetSpawnPointGroup.GetNewElement(Config.loopType, ref curSpawnPointIndex, ref isLastYoYoIncrease);
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