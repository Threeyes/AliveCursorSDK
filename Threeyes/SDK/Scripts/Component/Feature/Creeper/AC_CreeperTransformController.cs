using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
/// <summary>
/// 控制整体的移动
/// 
/// Todo:
/// -完善后整合到SDK中
/// -增加Config（每个LegController的参数独立，不使用Config）
/// 
/// 功能：
/// -决定根模型的位置/旋转
///
/// PS:
/// -为了方便多个实例Creeper，不做成单例
/// </summary>
public class AC_CreeperTransformController : ComponentGroupBase<AC_CreeperLegController>
{
	public bool IsAnyLegMoving { get { return ListComp.Any(c => c.isMoving); } }
	public Transform tfModelBody { get { return creeperModelController.transform; } }//模型躯干（根物体）

	public AC_CreeperModelController creeperModelController;
	[Header("Body")]
	public float bodyMoveSpeed = 5;
	public float bodyRotateSpeed = 0.5f;
	public float maxBodyTurn = 90;//躯干最大旋转值（ToUse）
	public Vector3 bodyOffsetToCenter;//躯干相对于脚中心的默认全局位移（在运行前通过调用菜单”SaveBodyCenterOffset“进行设置）
	public Transform tfBodyMixer;//叠加影响躯体的位移及旋转（单独使用一个物体控制躯干的好处是，对躯干的修改不会影响到脚）（更改该物体的位置、旋转可实现跳跃、蹲下、转身等动作）

	[Header("Legs")]
	public float legMoveIntervalTime = 0.1f;//Warning：要比CreeperLegGhostController.tweenDuration值大，否则某个Leg会因此提前Tween完成而再次移动，从而出现某个脚频繁移动的问题
	public float legRealignDelayTime = 5;//停下后，多久开始脚对齐，大于0有效 
	public List<LegControllerGroup> listLegControllerGroup = new List<LegControllerGroup>();//PS:脚需要分组（如左上对右下），每次只能移动一组脚，长途奔袭时两组脚交错移动【兼容其他爬虫的行走】

	[Header("Runtime")]
	public int lastMoveGroupIndex = -1;
	public float lastMoveTime = 0;
	public Vector3 baseBodyPosition;
	public bool hasAligned = false;//在本次停下后是否已经对齐
	void Awake()
	{
		//Init
		baseBodyPosition = tfModelBody.position;
	}

	private void LateUpdate()
	{
		//——Body——
		//1.从所有脚的中心位置计算得出躯干的目标位置
		Vector3 bodyTargetPos = GetLegsCenterPos() + bodyOffsetToCenter * AC_ManagerHolder.CommonSettingManager.CursorSize;
		baseBodyPosition = Vector3.Lerp(baseBodyPosition, bodyTargetPos, Time.deltaTime * bodyMoveSpeed);//躯干的目标位置（与移动相关）

		//2.计算GhostBody的世界轴偏移量，并用其影响躯干位置（因为与音频等即时响应相关，因此不能用Lerp）
		Vector3 worldOffset = transform.TransformDirection(tfBodyMixer.localPosition);//计算tfGhostBody的局部位移，并转换为全局矢量
		worldOffset *= AC_ManagerHolder.CommonSettingManager.CursorSize;//乘以光标缩放（因为目标物体同步了缩放）
		tfModelBody.position = baseBodyPosition + worldOffset;//相对坐标不需要乘以缩放值，因为Ghost与目标物体的缩放一致，因此位置单位也一致（音频响应要求即时同步） 

		//通过tfGhostBody控制躯干的旋转
		//Todo:限制最大旋转值
		Quaternion targetRotation = tfBodyMixer.rotation;
		//tfModelRoot.rotation = Quaternion.Lerp(tfModelRoot.rotation, targetRotation, Time.deltaTime * bodyRotateSpeed);
		tfModelBody.rotation = targetRotation;//直接同步，便于及时响应音频

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
			if (!hasAligned && legRealignDelayTime > 0 && Time.time - lastMoveTime > legRealignDelayTime)
			{
				MoveAllLeg();
				hasAligned = true;//标记为已对齐，避免重复进入
			}
		}
	}

	void LegGroupTweenMove(int groupIndex)
	{
		if (Time.time - lastMoveTime < legMoveIntervalTime)//两次移动之间要有间隔，否则很假
		{
			return;
		}
		var listTarget = listLegControllerGroup[groupIndex].listLegController;
		listTarget.ForEach(com => com.TweenMoveAsync());
		lastMoveGroupIndex = groupIndex;
		lastMoveTime = Time.time;
		hasAligned = false;
	}

	public void SetLocalScale(Vector3 localScale)
	{
		transform.localScale = localScale;
	}

	/// <summary>
	/// 立即传送Creeper到最终位置
	/// </summary>
	public void Teleport()
	{
		ListComp.ForEach(c => c.Teleport());
	}

	/// <summary>
	/// 移动所有Leg到指定位置，忽略时间间隔
	/// </summary>
	public void MoveAllLeg()
	{
		ListComp.ForEach(c => c.TweenMoveAsync(true));
	}
	Vector3 GetLegsCenterPos()
	{
		Vector3 centerPos = Vector3.zero;
		ListComp.ForEach(com => centerPos += com.tfSourceTarget.position);
		centerPos /= ListComp.Count;
		return centerPos;
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
		public List<AC_CreeperLegController> listLegController = new List<AC_CreeperLegController>();
	}
	#endregion

	#region Editor
#if UNITY_EDITOR
	[ContextMenu("SaveBodyOffsetToCenter")]
	void SaveBodyOffsetToCenter()
	{
		//在程序开始时或开始前记录默认的位移，因为有些躯干不在正中心【如Hand】
		Vector3 legsCenterPos = GetLegsCenterPos();
		bodyOffsetToCenter = tfModelBody.position - legsCenterPos;
	}

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
