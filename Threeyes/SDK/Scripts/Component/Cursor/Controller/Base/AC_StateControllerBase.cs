using Threeyes.Action;
public interface IAC_StateController : IAC_ModControllerHandler
{
	void SetState(AC_CursorStateInfoEx cursorStateInfo);
	bool IsCurStateActionComplete(ActionState actionState);
}

///To提醒：
///1.重写难度较大，一般只建议Modder克隆内置的soCursorStateActionCollection，并且修改其中的部分Action
///2.挂在AC物体上
public abstract class AC_StateControllerBase<TSOConfig, TConfig> : AC_ConfigableComponentBase<TSOConfig, TConfig>,
	IAC_StateController
	where TSOConfig : AC_SOConfigBase<TConfig>
{
	#region Interface
	public abstract void SetState(AC_CursorStateInfoEx cursorStateInfo);
	public abstract bool IsCurStateActionComplete(ActionState actionState);
	#endregion


	#region Callback
	public virtual void OnModControllerInit()
	{
	}
	public virtual void OnModControllerDeinit()
	{

	}
	#endregion
}
