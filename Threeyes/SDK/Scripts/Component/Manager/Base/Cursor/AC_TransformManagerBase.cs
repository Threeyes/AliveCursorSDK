using UnityEngine;
using UnityEngine.SceneManagement;
using NaughtyAttributes;
public class AC_TransformManagerBase<T> : AC_ManagerWithControllerBase<T, IAC_TransformController, AC_DefaultTransformController>, IAC_TransformManager
where T : AC_TransformManagerBase<T>
{
	#region Interface
	public Vector3 CursorBaseScale
	{
		get { return cursorBaseScale; }
		set
		{
			cursorBaseScale = value;
			ActiveController.SetLocalScale(AC_ManagerHolder.CommonSettingManager.CursorSize, value);//通知更新
		}
	}
	#endregion

	#region Property & Field
	[ReadOnly] [SerializeField] protected Vector3 cursorBaseScale = Vector3.one;//【受StateManager影响】模型基础缩放【0，1】，用于状态切换时更改模型缩放
	#endregion

	#region Public Method
	public virtual void TeleportCursor()
	{
		ActiveController.Teleport(AC_ManagerHolder.SystemCursorManager.WorldPosition);//MoveToTargetPointAtOnce
	}
	#endregion

	#region Unity Method
	//PS: 通过这里主动调用(Default或Modder)MovementController，避免多个同时存在时自行调用
	protected virtual void Update()
	{
		ActiveController.UpdateFunc();
	}
	protected virtual void FixedUpdate()
	{
		ActiveController.FixedUpdateFunc();
	}
	#endregion

	#region Callback
	// 设置位置、旋转等参数
	public virtual void OnModInit(Scene scene, AC_AliveCursor aliveCursor)
	{
		///Warning：
		///1.Controller直接通过alivecursor的单例直接调用方法，这样方便Modder随时在任意状态调用DefaultTransformController（如不想覆盖Bored状态）
		///2.因为场景可能有多个Controller，因此需要由Manager决定需要调用哪一个，而不是使用SendMessage
		modController = aliveCursor.GetComponent<IAC_TransformController>();//尝试获取
		ActiveController.OnModControllerInit();//初始化引用等（注意不能提前调用，否则会报错）
	}
	public virtual void OnModDeinit(Scene scene, AC_AliveCursor aliveCursor)
	{
		modController?.OnModControllerDeinit();//仅DeinitMod的Controller
		modController = null;
	}
	#endregion
}
