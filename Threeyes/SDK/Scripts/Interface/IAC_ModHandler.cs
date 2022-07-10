/// <summary>
/// Mod lifecycle event
/// </summary>
public interface IAC_ModHandler
{
    /// <summary>
    /// PS:
    /// 1.Get call right after PersistentData is Loaded
    /// </summary>
    void OnModInit();

    /// <summary>
    /// PS:
    /// 1.Get call right after PersistentData is Saved
    /// </summary>
    void OnModDeinit();
}   