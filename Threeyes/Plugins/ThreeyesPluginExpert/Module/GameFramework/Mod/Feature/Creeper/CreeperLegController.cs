using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using DG.Tweening;
using System.Threading.Tasks;
using Threeyes.Core;

namespace Threeyes.GameFramework
{
    /// <summary>
    /// Control Creeper Leg's Movement
    /// 
    /// 功能：
    /// -标注Spider单个脚的落脚点，某个脚与落脚点的距离过大时，就开始挪动操作。原理类似于牵线木偶，先控制身体移动，关节随后跟随移动
    /// 
    /// PS:
    /// -当ChainIKConstraint.Weight为0时，对应关节使用当前动画的位置/旋转值，因此可以给模型制作静态的抬脚动画（命名为Idle），从而实现平滑的抬脚过渡
    /// -ChainIKConstant中的Tip应该是针对最终的骨骼，这样能够避免Target旋转导致指尖关节意外偏转。
    /// 
    /// Todo:
    /// -【优先】增加可选的最大扭转容忍度，如果当前旋转值与初始旋转值偏离过大时也会强制移动，避免过扭（如猫后腿）。同时要增加移动/旋转对触发移动的比重字段（参考CanvasScaler.Match）；curDistance使用curOffset封装，包含了位移及偏转基于权重的值
    /// -也需要保存当前脚的固定点，这样即使挪动整个身体，脚也是固定在原地
    /// -不要跟踪对应GhostLeg目标，而是当父物体偏移达到一定值就自动移动，这样能解决Bored时快速旋转导致脚扭成一团的问题
    /// -弄成局部坐标的长度（或者乘以SpiderGhostController的缩放）
    /// -脚：
    ///     -！！活动范围应该是以根关节与躯干的中点为原点，半径为legMaxDistance的球形区域，然后是基于当前躯体的相对位置为轴心，移动时优先移动targetPos跟中点比重最近的脚（可通过计算连线跟球体的交点，取最短值）
    ///
    /// ToAdd:
    /// -增加自由跟随模式，常用于停止移动后某个脚进行随机移动
    ///
    /// </summary>
    public class CreeperLegController : ComponentHelperBase<ChainIKConstraint>
    {
        public bool NeedMove { get { return isExcessive && !isMoving; } }
        public float CompWeight { get { return Comp.weight; } set { Comp.weight = value; } }
        public float MaxReachDistanceFinal { get { return maxReachDistance * baseScale; } }//乘以光标缩放值
        public float UpdatePositionDistanceFinal { get { return moveThreshold * baseScale; } }
        public Transform tfSourceTarget { get { return Comp.data.target; } }//运行时从chainIKConstraint中获取，注意要与模型分开摆放，否则会受其位置影响
        public float baseScale { get { return creeperTransformController.baseScale; } }//基础缩放值

        public Transform tfEndPoint;//脚的终点
        public float moveThreshold = 0.1f;//How far to begin move(当脚与目标点的距离超过一定距离后更新脚位置)
        public float maxReachDistance = 0.3f;//脚能移动的最远距离

        //ToAdd：限制关节旋转轴向，比如手指中段只能沿着单个轴向旋转
        public Vector2 weightRange = new Vector2(0, 1);//Range on move
        public float tweenDuration = 0.08f;
        public Ease easeLegUp = Ease.Linear;
        public Ease easeLegDown = Ease.Linear;

        [Header("Runtime")]
        public Vector3 targetPos;//当前目标位置
        public float curDistance;//Distance to EndPoint
        public bool isExcessive = false;//是否过度（如距离过长）（需要移动）
        public bool isMoving = false;//正在移动

        //Pivot
        CreeperTransformController creeperTransformController;
        public Transform tfModelBody;//模型躯干
        public Vector3 localPivotPos;//脚移动的轴心（程序开始前，脚应该摆在该中心位置）（注意因为缩放同步，因此不需要乘以光标尺寸）
        protected virtual void Awake()
        {
            Init();//Init
        }
        protected virtual void Init()
        {
            tfEndPoint.position = tfSourceTarget.position = Comp.data.tip.position;//同步初始位置
            targetPos = tfEndPoint.position;

            //以脚末端及躯干的中点作为脚的锚点
            creeperTransformController = transform.GetComponentInParent<CreeperTransformController>(true);
            tfModelBody = creeperTransformController.tfModelBody;

            localPivotPos = tfModelBody.InverseTransformPoint(tfSourceTarget.position);// -中点是程序开始时的默认点
        }

        private void Update()
        {
            curDistance = Vector3.Distance(tfSourceTarget.position, tfEndPoint.position);
            isExcessive = curDistance > UpdatePositionDistanceFinal;
        }

        public void Teleport()
        {
            transform.position = tfSourceTarget.position = targetPos = tfEndPoint.position;
        }

        public async void TweenMoveAsync(bool forceUpdate = false)
        {
            if (!forceUpdate)
                if (!isExcessive)//不需要判断，强制更新位置
                    return;

            isMoving = true;
            ///限制可移动区域为原点的指定圆形区间（在TweenMoveAsync中判断是否可以移动）
            ///-计算目的点与锚点的连线与半径范围球体的交点（如果在球体内，则直接使用目的点），然后取最靠近目的地的点
            /// PS:
            /// -因为脚长有限，因此新位置只能是与目标连线的投影（长度为moveFootDistance）位置
            Vector3 worldPivotPos = tfModelBody.TransformPoint(localPivotPos);
            Vector3 vector = tfEndPoint.position - worldPivotPos;
            float vectorLength = vector.magnitude;

            //Todo:检查targetPos是否在可移动范围中
            if (vectorLength - MaxReachDistanceFinal < 0)//在可移动区域内:直接使用目标位置
            {
                targetPos = tfEndPoint.position;
            }
            else//在可移动区域外：使用连线最远点
            {
                Vector3 vectorNormal = vector.normalized;
                vectorNormal.Scale(Vector3.one * MaxReachDistanceFinal);
                targetPos = worldPivotPos + vectorNormal;
            }

            /// 挪动操作（针对ChainIKConstraint：
            /// 1.模拟抬脚动作：将Weight设置为0
            /// 2.将Target的位置设置为落脚点
            /// 3.将Weight设置为1（通过Tween）
            var tweenExit = DOTween.To(() => CompWeight, (val) => CompWeight = val, weightRange.x, tweenDuration / 2).SetEase(easeLegUp);
            tweenExit.onComplete +=
                () =>
                {
                    var tweenEnter = DOTween.To(() => CompWeight, (val) => CompWeight = val, weightRange.y, tweenDuration / 2).SetEase(easeLegDown);
                    tweenEnter.onComplete +=
                    () =>
                    {
                        tfSourceTarget.position = targetPos;
                        isMoving = false;
                    };
                };

            while (!isMoving)
            {
                await Task.Yield();
            }
        }


#if UNITY_EDITOR
        #region Editor
        [Header("Editor")]
        public float gizmosRadius = 0.1f;
        public bool gizmosShowDistance = false;
        private void OnDrawGizmos()
        {
            //绘制Pivot
            if (tfModelBody)//青色：中点、启动距离及最远距离
            {
                Gizmos.color = new Color(0f, 1f, 1f, 0.2f);//Cyan
                Gizmos.DrawWireSphere(tfModelBody.TransformPoint(localPivotPos), UpdatePositionDistanceFinal);
                Gizmos.DrawWireSphere(tfModelBody.TransformPoint(localPivotPos), MaxReachDistanceFinal);
                Gizmos.color = Color.white;
            }

            if (tfEndPoint)//绿色：EndPoint
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(tfEndPoint.position, gizmosRadius);
                Gizmos.color = Color.white;
            }

            Gizmos.DrawWireSphere(targetPos, gizmosRadius);//白色：当前目标位置

            if (Application.isPlaying && tfSourceTarget)
            {
                if (gizmosShowDistance)//渐变色代表距离接近度
                {
                    float distancePercent = Vector3.Distance(targetPos, tfSourceTarget.position) / MaxReachDistanceFinal;
                    Color color = Color.Lerp(Color.green, Color.red, distancePercent);
                    Gizmos.color = color;
                    UnityEditor.Handles.Label(transform.position, $"{(int)(distancePercent * 100)}%");//绘制当前距离
                }
                Gizmos.DrawLine(targetPos, tfSourceTarget.position);
            }
        }
        #endregion
#endif
    }
}