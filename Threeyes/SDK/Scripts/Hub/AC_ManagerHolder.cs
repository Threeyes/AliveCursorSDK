using Threeyes.GameFramework;
/// <summary>
/// 保存Mod所需的Manager引用
/// </summary>
public static class AC_ManagerHolder
{
	///规范:
	///1.Manager仅暴露属性；
	///2.调用Mod的方法需要通过ModCommunication调用对应接口实现，优势是能够通过接口统一发送信息.相比Action的好处：1.指明参数名；2.避免场景销毁导致Action绑定的方法丢失
	///3.:每个接口对应一个方法，方便扩展，避免因为在同一个接口中新增方法，导致旧类未实现而报错的问题
	///
	/// Note：
	///1.在AC_ManagerBase.SetInstanceFunc中自动注册

	//——Setting——
	public static IAC_CommonSettingManager CommonSettingManager { get; internal set; }

	//——System——
	public static IAC_SystemCursorManager SystemCursorManager { get; internal set; }
	public static IAC_SystemInputManager SystemInputManager { get; internal set; }
	public static IAC_SystemAudioManager SystemAudioManager { get; internal set; }

	//——Mod——
	public static IAC_EnvironmentManager EnvironmentManager { get; internal set; }
	public static IAC_PostProcessingManager PostProcessingManager { get; internal set; }
	public static IAC_TransformManager TransformManager { get; internal set; }
	public static IAC_StateManager StateManager { get; internal set; }
}