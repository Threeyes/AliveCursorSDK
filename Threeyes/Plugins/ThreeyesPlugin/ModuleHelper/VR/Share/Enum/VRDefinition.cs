
/// <summary>
/// 检查/获取控制器的类型
/// </summary>
public enum ControllerCheckType
{
    //以下用于全局
    Both = 0,
    Left = 1,
    Right = 2,

    //以下用于交互中
    /// <summary>
    /// 触碰当前物体的手柄(适用于脚本挂载在VRTK_InteractableObject上)
    /// </summary>
    TouchingThisObject,
    /// <summary>
    /// 自身及父类的控制器(适用于脚本挂载在Left/RightController下)
    /// </summary>
    ParentController,
}