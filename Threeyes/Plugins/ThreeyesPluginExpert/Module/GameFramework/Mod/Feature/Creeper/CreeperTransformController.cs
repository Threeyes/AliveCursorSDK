using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Threeyes.Config;

namespace Threeyes.GameFramework
{
    /// <summary>
    /// Control Creeper Body's Movement
    /// 
    /// 功能：
    /// -决定根模型的位置/旋转
    ///
    /// ToUpdate：
    /// -尝试tfLookEndPoint与tfPosEndPoint分别对应不同物体，且通过Lerp逐渐朝向目的位置（可以是本组件创建一个空物体，限定角速度Lerp到tfLookEndPoint的目标位置），从而使移动时的朝向更加平滑而不是固定一个角度
    /// -针对AD，提炼出CreeperUserInput，通过向该组件或类似组件传递移动信号。实现：
    ///     #1 可以攀爬障碍物：该组件会尝试往前方及下向发射射线，当检测到前方指定距离有障碍物就获取其法线指定高度的一个点作为新的目标移动点
    ///     #2 平面移动：遇到障碍物就暂停，跟车辆一样
    /// 
    /// PS:
    /// -为了方便多个实例Creeper，不做成单例
    /// -不需要将所有脚都弄成IK，比如螃蟹就可以保留2个自由活动的前手关节
    /// -每个LegController的参数独立，因此不使用Config
    /// </summary>
    public class CreeperTransformController : ConfigurableComponentBase<SOCreeperTransformControllerConfig, CreeperTransformController.ConfigInfo>
    {
        public Transform tfModelBody { get { return creeperModelController.transform; } }//模型躯干（根物体）
        public float baseScale { get { return creeperModelController.baseScale; } }

        public CreeperModelController creeperModelController;
        [Header("Body")]
        public bool isSyncRotOrLookAt = true;// GhostBody是同步tfEndPoint的旋转，还是朝向该目标（ToUpdate：挪到Config中）
        public Vector3 localBodyUp = new Vector3(0, 1, 0);//GhostBody的局部Up轴
        public Transform tfPosEndPoint;//躯干位置的终点
        public Transform tfLookEndPoint;//躯干朝向的目标
        public Transform tfGhostBody;//Root ghost body
        public Transform tfBodyMixer;//叠加影响躯体的位移及旋转（单独使用一个物体控制躯干的好处是，对躯干的修改不会影响到脚）（更改该物体的位置、旋转可实现跳跃、蹲下、转身等动作）

        [Header("Legs")]
        public List<CreeperLegController> listLegController = new List<CreeperLegController>();//All child Leg Controllers (The index will be used by AC_CreeperAudioVisualizer)
        public List<LegControllerGroup> listLegControllerGroup = new List<LegControllerGroup>();//PS:脚需要分组（如左上对右下），每次只能移动一组脚，长途奔袭时两组脚交错移动【兼容其他爬虫的行走】

        [Header("Runtime")]
        public int lastMoveGroupIndex = -1;
        public float lastMoveTime = 0;
        public Vector3 baseBodyPosition;
        Quaternion ghostBodyRotation;

        public bool hasAligned = false;//在本次停下后是否已经对齐
        protected virtual void Awake()
        {
            baseBodyPosition = tfModelBody.position;
        }

        private void LateUpdate()
        {
            //——Body——
            tfGhostBody.position =
             Vector3.MoveTowards(tfGhostBody.position, tfPosEndPoint.position, Config.ghostBodyMoveSpeed * Time.deltaTime * baseScale);//以固定速度移动Ghost

            baseBodyPosition = Vector3.Lerp(baseBodyPosition, tfGhostBody.position, Time.deltaTime * Config.bodyMoveSpeed);//躯干向GhostBody逐渐移动，更加真实

            //2.计算tfBodyMixer的世界轴偏移量，并用其影响躯干位置（因为与音频等即时响应相关，因此不能用Lerp）
            Vector3 worldOffset = tfBodyMixer.parent.TransformDirection(tfBodyMixer.localPosition);//将tfBodyMixer的局部位移转换为全局矢量（ToUpdate）
            worldOffset *= baseScale;//乘以光标缩放（因为目标物体同步了缩放）
            tfModelBody.position = baseBodyPosition + worldOffset;//相对坐标不需要乘以缩放值，因为Ghost与目标物体的缩放一致，因此位置单位也一致（音频响应要求即时同步） 

            //ToAdd：增加testSyncOrLookAt选项，可以决定tfGhostBody的朝向是直接同步tfEndPoint，还是朝向tfEndPoint以便做指向路线行为（如猫）

            //通过tfGhostBody控制躯干的旋转
            if (isSyncRotOrLookAt)
            {
                ghostBodyRotation = tfLookEndPoint.rotation;
            }
            else//LookAt
            {
                Vector3 direction = tfLookEndPoint.position - tfGhostBody.position;
                if (direction.sqrMagnitude > 0.001f)//移动中
                {
                    ghostBodyRotation = Quaternion.LookRotation(direction, tfGhostBody.TransformDirection(localBodyUp));
                }
                else//距离为0：使用tfEndPoint的旋转值或逐渐趋向
                {
                    ghostBodyRotation = tfLookEndPoint.rotation;
                }
                //tfGhostBody.LookAt(tfEndPoint, tfGhostBody.TransformDirection(testLocalBodyUp));
            }

            tfGhostBody.rotation = Quaternion.Lerp(tfGhostBody.rotation, ghostBodyRotation, Time.deltaTime * Config.ghostBodyRotateSpeed);//设置tfGhostBody的旋转，因为Lerp的原因，因此不会出现过扭的问题（tfGhostBody会控制其子GhostLeg的朝向）（不需要像位移一样限定固定旋转速度）
            tfModelBody.rotation = tfBodyMixer.rotation;//直接同步tfBodyMixer的旋转，便于及时响应音频(tfBodyMixer作为tfGhostBody的子物体，也会受其旋转影响)（只影响躯干的朝向，不影响脚，因为它们有各自的目标点）

            ///——Legs——
            ///-检查哪一组需要更新位置且偏移量最大，如果是就先更新该组；同时只能有一组进行移动
            float maxGroupDistance = 0;//记录所有组中平均距离最大的
            int needMoveGroupIndex = -1;
            for (int i = 0; i != listLegControllerGroup.Count; i++)
            {
                if (lastMoveGroupIndex == i)//防止同一组连续移动
                    continue;
                var lcg = listLegControllerGroup[i];
                if (lcg.NeedMove && lcg.AverageDistance > maxGroupDistance)
                {
                    needMoveGroupIndex = i;
                    maxGroupDistance = lcg.AverageDistance;
                }
            }
            if (needMoveGroupIndex >= 0)//任意脚需要移动
            {
                LegGroupTweenMove(needMoveGroupIndex);
            }
            else//所有脚都不需要移动
            {
                //在停止移动一定时间后，强制对齐所有GhostLegs的位置，避免强迫症患者（如本人）觉得不对称
                if (!hasAligned && Config.legRealignDelayTime > 0 && Time.time - lastMoveTime > Config.legRealignDelayTime)
                {
                    MoveAllLeg();
                    hasAligned = true;//标记为已对齐，避免重复进入
                }
            }
        }

        void LegGroupTweenMove(int groupIndex)
        {
            if (Time.time - lastMoveTime < Config.legMoveIntervalTime)//两次移动之间要有间隔，否则很假
            {
                return;
            }
            var listTarget = listLegControllerGroup[groupIndex].listLegController;
            listTarget.ForEach(com => com.TweenMoveAsync());
            lastMoveGroupIndex = groupIndex;
            lastMoveTime = Time.time;
            hasAligned = false;
        }

        /// <summary>
        /// 立即传送Creeper到最终位置
        /// </summary>
        public void Teleport()
        {
            listLegController.ForEach(c => c.Teleport());
        }

        /// <summary>
        /// 移动所有Leg到指定位置，忽略时间间隔
        /// </summary>
        public void MoveAllLeg()
        {
            listLegController.ForEach(c => c.TweenMoveAsync(true));
        }

        #region Define
        [System.Serializable]
        public class LegControllerGroup
        {
            public bool NeedMove { get { return listLegController.Any(com => com.NeedMove); } }
            public float AverageDistance
            {
                get
                {
                    //ToUpdate：应该是只统计需要移动的脚的距离
                    _averageDistance = 0;
                    listLegController.ForEach(c => _averageDistance += c.curDistance);
                    _averageDistance /= listLegController.Count;
                    return _averageDistance;
                }
            }//总位移
            float _averageDistance;
            public List<CreeperLegController> listLegController = new List<CreeperLegController>();
        }
        [System.Serializable]
        public class ConfigInfo : SerializableDataBase
        {
            [Header("Body")]
            public float bodyMoveSpeed = 3;
            public float ghostBodyMoveSpeed = 1.3f;
            public float ghostBodyRotateSpeed = 1;

            [Header("Leg")]
            [Range(0, 1)] public float legMoveIntervalTime = 0.1f;//（Warning：要比任意CreeperLegGhostController的tweenDuration值大，否则某个Leg会因此提前Tween完成而再次移动，从而出现某个脚频繁移动的问题）
            [Range(0, 120)] public float legRealignDelayTime = 5;//Realign legs after specify seconds, only valid when value>0
        }
        #endregion

        #region Editor
#if UNITY_EDITOR
        //[ContextMenu("SaveBodyOffsetToCenter")]
        //void SaveBodyOffsetToCenter()
        //{
        //	//在程序开始时或开始前记录默认的位移，因为有些躯干不在正中心【如Hand】
        //	Vector3 legsCenterPos = GetLegsCenterPos();
        //	bodyOffsetToCenter = tfModelBody.position - legsCenterPos;
        //}

        [Header("Editor")]
        public float gizmosRadius = 0.1f;
        private void OnDrawGizmos()
        {
            if (tfBodyMixer)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(tfBodyMixer.position, tfBodyMixer.position + tfBodyMixer.right * gizmosRadius);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(tfBodyMixer.position, tfBodyMixer.position + tfBodyMixer.up * gizmosRadius);
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(tfBodyMixer.position, tfBodyMixer.position + tfBodyMixer.forward * gizmosRadius);
            }
        }
#endif
        #endregion
    }
}