using Threeyes.GameFramework;
/// <summary>
/// PS：
/// 1.This interface is only valid for specify builtin Controller classes (eg: MovementController), 
/// 2.Use IAC_ModHandler instead if your want to deal with PersistentData!
/// 
/// Warning：该接口只是为了保证兼容性，真正有效的是IModControllerHandler接口
/// </summary>
public interface IAC_ModControllerHandler : IModControllerHandler
{
}