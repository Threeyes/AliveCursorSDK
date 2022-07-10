using UnityEngine.SceneManagement;

/// <summary>
/// Init/Deinit Mod scene
/// </summary>
public interface IAC_Manager_ModInitHandler
{
    void OnModInit(Scene scene, AC_AliveCursor aliveCursor);
    void OnModDeinit(Scene scene, AC_AliveCursor aliveCursor);
}