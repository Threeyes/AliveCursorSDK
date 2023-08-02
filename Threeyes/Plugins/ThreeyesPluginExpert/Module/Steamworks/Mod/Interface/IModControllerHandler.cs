namespace Threeyes.Steamworks
{
    /// <summary>
    /// PS：
    /// 1.This interface is only valid for specify builtin Controller classes (eg: MovementController), 
    /// 2.Use IAC_ModHandler instead if your want to deal with PersistentData!
    /// </summary>
    public interface IModControllerHandler
    {
        void OnModControllerInit();
        void OnModControllerDeinit();
    }
}