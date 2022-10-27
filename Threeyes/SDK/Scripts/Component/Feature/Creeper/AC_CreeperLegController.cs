using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using DG.Tweening;
using System.Threading.Tasks;
using System.Linq;
/// <summary>
/// 功能：
/// -标注Spider单个脚的落脚点，某个脚与落脚点的距离过大时，就开始挪动操作
/// 
/// Todo:
/// -也需要保存当前脚的固定点，这样即使挪动整个身体，脚也是固定在原地
/// -不要跟踪对应GhostLeg目标，而是当父物体偏移达到一定值就自动移动，这样能解决Bored时快速旋转导致脚扭成一团的问题
/// -弄成局部坐标的长度（或者乘以SpiderGhostController的缩放）
/// -脚：
///     -！！活动范围应该是以根关节与躯干的中点为原点，半径为legMaxDistance的球形区域，然后是基于当前躯体的相对位置为轴心，移动时优先移动targetPos跟中点比重最近的脚（可通过计算连线跟球体的交点，取最短值）
///
/// ToAdd:
/// -增加自由跟随模式，常用于停止移动后某个脚进行随机移动
/// </summary>
public class AC_CreeperLegController : ComponentHelperBase<ChainIKConstraint>
		, IAC_CommonSetting_CursorSizeHandler
{
	public bool NeedMove { get { return isExcessive && !isMoving; } }
	public float CompWeight { get { return Comp.weight; } set { Comp.weight = value; } }
	public float MaxReachDistanceFinal { get { return maxReachDistance * settingCursorSize; } }//乘以光标缩放值
	public float UpdatePositionDistanceFinal { get { return moveThreshold * settingCursorSize; } }
	public Transform tfSourceTarget { get { return Comp.data.target; } }//运行时从chainIKConstraint中获取，注意要与模型分开摆放，否则会受其位置影响

	public Transform tfEndPoint;//脚的终点
	public float moveThreshold = 0.1f;//How far to begin move(当脚与目标点的距离超过一定距离后更新脚位置)
	public float maxReachDistance = 0.3f;//脚能移动的最远距离

	//ToAdd：限制关节旋转轴向，比如手指中段只能沿着单个轴向旋转
	public Vector2 weightRange = new Vector2(0, 1);//Range on move
	public float tweenDuration = 0.08f;
	public Ease easeLegUp = Ease.Linear;
	public Ease easeLegDown = Ease.Linear;

	[Header("Runtime")]
	public float curDistance;//Distance to EndPoint
	public bool isExcessive = false;//距离过长（需要移动）
	public bool isMoving = false;//正在移动
	public Vector3 endPos;

	//Pivot
	AC_CreeperTransformController creeperTransformController;
	public Transform tfModelBody;//模型躯干
	public Vector3 localPivotPos;//脚移动的轴心（相对于躯干的局部位置，注意因为缩放同步，因此不需要乘以光标尺寸）
	private void Awake()
	{
		//Init
		tfEndPoint.position = tfSourceTarget.position = Comp.data.tip.position;//同步初始位置
		endPos = tfEndPoint.position;
		settingCursorSize = AC_ManagerHolder.CommonSettingManager.CursorSize;

		//以脚末端及躯干的中点作为脚的锚点
		creeperTransformController = transform.GetComponentInParent<AC_CreeperTransformController>(true);
		tfModelBody = creeperTransformController.tfModelBody;
		localPivotPos = tfModelBody.InverseTransformPoint((tfModelBody.position + tfSourceTarget.position) / 2);
	}
	private void Update()
	{
		curDistance = Vector3.Distance(tfSourceTarget.position, tfEndPoint.position);
		isExcessive = curDistance > UpdatePositionDistanceFinal;
	}

	public void Teleport()
	{
		transform.position = tfSourceTarget.position = endPos = tfEndPoint.position;
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
		if ((vectorLength - MaxReachDistanceFinal) < 0)//在可移动区域内:直接使用目标位置
		{
			endPos = tfEndPoint.position;
		}
		else//在可移动区域外：使用连线最远点
		{
			Vector3 vectorNormal = vector.normalized;
			vectorNormal.Scale(Vector3.one * MaxReachDistanceFinal);
			endPos = worldPivotPos + vectorNormal;
		}

		/// 挪动操作（针对ChainIKConstraint：
		/// -将Weight设置为0
		/// -将Target的位置设置为落脚点
		/// -将Weight设置为1（可以通过Tween）
		var tweenExit = DOTween.To(() => CompWeight, (val) => CompWeight = val, weightRange.x, tweenDuration / 2).SetEase(easeLegUp);
		tweenExit.onComplete +=
			() =>
			{
				var tweenEnter = DOTween.To(() => CompWeight, (val) => CompWeight = val, weightRange.y, tweenDuration / 2).SetEase(easeLegDown);
				tweenEnter.onComplete +=
				() =>
				{
					tfSourceTarget.position = endPos;
					isMoving = false;
				};
			};

		while (!isMoving)
		{
			await Task.Yield();
		}
	}

	#region Callback
	public float settingCursorSize = 1;
	public void OnCursorSizeChanged(float value)
	{
		settingCursorSize = value;
	}
	#endregion

#if UNITY_EDITOR
	#region Editor
	[Header("Editor")]
	public float gizmosRadius = 0.1f;
	public bool gizmosShowDistance = false;
	private void OnDrawGizmos()
	{
		//绘制Pivot
		if (tfModelBody)//青色：中点
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireSphere(tfModelBody.TransformPoint(localPivotPos), gizmosRadius);
			Gizmos.color = Color.white;
		}

		if (tfEndPoint)//绿色：EndPoint
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(tfEndPoint.position, gizmosRadius);
			Gizmos.color = Color.white;
		}

		Gizmos.DrawWireSphere(endPos, gizmosRadius);//白色：待移动位置

		if (Application.isPlaying && tfSourceTarget)//渐变色代表距离接近度
		{
			float distancePercent = Vector3.Distance(endPos, tfSourceTarget.position) / MaxReachDistanceFinal;
			Color color = Color.Lerp(Color.green, Color.red, distancePercent);
			Gizmos.color = color;
			if (gizmosShowDistance)
			{
				UnityEditor.Handles.Label(transform.position, $"{(int)(distancePercent * 100)}%");//绘制当前距离
			}
			Gizmos.DrawLine(endPos, tfSourceTarget.position);
		}
	}
	#endregion
#endif
}
