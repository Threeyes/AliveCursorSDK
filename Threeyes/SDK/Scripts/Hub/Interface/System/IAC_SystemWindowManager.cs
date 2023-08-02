using Threeyes.Steamworks;

public interface IAC_SystemWindow_ChangedHandler
{
    /// <summary>
    /// Called before and after screen switching/resolution change
    /// </summary>
    /// <param name="e"></param>
    public void OnWindowChanged(AC_WindowEventExtArgs e);
}