using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;
using Newtonsoft.Json;

/// <summary>
/// Function：
/// -Control Character Movement
/// -Random Animations using BlendTree
/// 
/// 实现原理：
/// -主BlendTree通过MoveSpeed参数控制Idle/Move的动画切换（中间可以按需求细化成Walk和Run），sub BlendTree通过RandomParamInfo（随机值参数）随机调用对应Bored的动画
/// Howto：
/// -为每个sub BlendTree设置对应的动画权重：在BlendTree设置界面中，先将第一个设置为0，最后一个设置为1，然后勾选[Automate Thresholds]
/// </summary>
public class AC_CharacterAnimatorController : AC_ConfigableComponentBase<Animator, AC_SOCharacterAnimatorControllerConfig, AC_CharacterAnimatorController.ConfigInfo>
{
	const string moveSpeedParamName = "MoveSpeed";

	[ValidateInput(nameof(ValidateObjectMovement), "Please set this field to an instance that inherits IAC_ObjectMovement!")] public MonoBehaviour objectMovement;//通过基类获取位移信息(PS:无法声明接口，只能通过强转实现)

	//[Header("Config")]
	//public List<RandomParamInfo> listRandomParamInfo = new List<RandomParamInfo>();//Random Animations for each sub BlendTree


	[Header("Runtime")]
	public float nextChangeRandomInterval = 5;
	public float lastChangeRandomTime;
	public bool isLastMoving = false;
	IAC_ObjectMovement objectMovementReal;

	protected virtual void Start()
	{
		objectMovementReal = objectMovement as IAC_ObjectMovement;
		if (objectMovementReal == null)
		{
			Debug.LogError($"Please set {nameof(objectMovement)} to an instance that inherits {nameof(IAC_ObjectMovement)}!");
			return;
		}

		Config.listRandomParamInfo.ForEach((i) => i.InitValue(Comp));
	}
	protected virtual void LateUpdate()
	{
		bool isMoving = objectMovementReal.IsMoving;
		Comp.SetFloat(moveSpeedParamName, objectMovementReal.CurMoveSpeedPercent);

		Config.listRandomParamInfo.ForEach((i) => i.UpdateValue(isMoving));

		isLastMoving = isMoving;
	}

	#region Utility
	bool ValidateObjectMovement(MonoBehaviour monoBehaviour)
	{
		if (monoBehaviour is IAC_ObjectMovement)
			return true;
		return false;
	}
	//PS: Manual add data to list, because nested class in List can't construct with default values: https://forum.unity.com/threads/lists-default-values.206956/
	[Button("AddParamInfo")]
	void AddParamInfo()
	{
		Config.listRandomParamInfo.Add(new RandomParamInfo());
	}
	#endregion

	#region Define
	[System.Serializable]
	public class ConfigInfo : AC_SerializableDataBase
	{
		public List<RandomParamInfo> listRandomParamInfo = new List<RandomParamInfo>();//Random Animations for each sub BlendTree
	}

	/// <summary>
	/// Animator Param info for BlendTree
	/// </summary>
	[System.Serializable]
	public class RandomParamInfo
	{
		public string paramName;
		[Min(1)] public int totalAnimation = 1;//Total animation in the target BlendTree
		[Range(0, 1)] public float defaultValue = 1;

		public bool changeAtRuntime = true;//Change at runtime
		[EnableIf(nameof(changeAtRuntime))] [AllowNesting] public bool changeOnMove = false;//Change when object moving
		[EnableIf(nameof(changeAtRuntime))] [AllowNesting] public Vector2 changIntervalRange = new Vector2(5, 10);//The interval time between switching random values
		[EnableIf(nameof(changeAtRuntime))] [AllowNesting] public bool preciseValue = true;//Should change to the exactly value [0, 1] of the animation in BlendTree (Mainly used for multiple mismatched animations)
		[EnableIf(nameof(changeAtRuntime))] [AllowNesting] public float transitionDuration = 0.2f;//duration for Tween random value, set to zero if you need this

		//[Header("Runtime")]
		[JsonIgnore] Animator cacheAnimator;
		[JsonIgnore] float nextChangeRandomInterval = 5;
		[JsonIgnore] float lastChangeRandomTime;

		public RandomParamInfo()
		{
			totalAnimation = 1;
			defaultValue = 1;
			changeAtRuntime = true;
			changeOnMove = false;
			changIntervalRange = new Vector2(5, 10);
			preciseValue = true;
			transitionDuration = 0.2f;
		}

		public RandomParamInfo(string paramName) : this()
		{
			this.paramName = paramName;
		}

		public void InitValue(Animator animator)
		{
			cacheAnimator = animator;
			nextChangeRandomInterval = Random.Range(changIntervalRange.x, changIntervalRange.y);
			cacheAnimator.SetFloat(paramName, defaultValue);
		}
		public void UpdateValue(bool isMoving)
		{
			if (!changeAtRuntime)
				return;

			if (!changeOnMove && isMoving)//Only change move random value on idle, incase motion transition not smooth
				return;

			if (Time.time - lastChangeRandomTime > nextChangeRandomInterval)
			{
				TweenValue_Float(paramName, GetNextRandomValue(), transitionDuration);//PS:要Tween到目的值，否则会出现硬切动画
				nextChangeRandomInterval = Random.Range(changIntervalRange.x, changIntervalRange.y);
				lastChangeRandomTime = Time.time;
			}
		}
		float GetNextRandomValue()
		{
			if (preciseValue)
			{
				int animationCount = Mathf.Max(1, totalAnimation);
				float intervalBetweenAnimation = animationCount == 1 ? 1 : 1f / (animationCount - 1);//动画之间的间隔（[0,1]区间）

				var rnd = new System.Random();
				int newRandomIndex = rnd.Next(0, animationCount);
				float newRandomValue = (newRandomIndex == animationCount - 1) ? 1 : newRandomIndex * intervalBetweenAnimation;//避免无穷余数
				return newRandomValue;
			}
			else
				return Random.value;
		}

		void TweenValue_Float(string paramName, float targetValue, float duration)
		{
			if (duration > 0)
			{
				DOTween.To(
			() => cacheAnimator.GetFloat(paramName),
			(v) => cacheAnimator.SetFloat(paramName, v),
			targetValue, duration);
			}
			else
			{
				cacheAnimator.SetFloat(paramName, targetValue);
			}
		}
	}
	#endregion
}
