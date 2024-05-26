using Threeyes.Action;
using Threeyes.Config;

///To提醒：
///1.重写难度较大，一般只建议Modder克隆内置的soCursorStateActionCollection，并且修改其中的部分Action
///2.挂在AC物体上
public abstract class AC_StateControllerBase<TSOConfig, TConfig> : ConfigurableComponentBase<TSOConfig, TConfig>,
	IAC_StateController
	where TSOConfig : SOConfigBase<TConfig>
    where TConfig : new()
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
