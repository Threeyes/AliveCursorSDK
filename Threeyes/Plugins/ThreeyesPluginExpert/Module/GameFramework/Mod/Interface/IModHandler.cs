namespace Threeyes.GameFramework
{
    public interface IModPreHandler
    {
        /// <summary>
        /// Get call before PersistentData is Loaded
        /// </summary>
        void OnModPreInit();
    }

    /// <summary>
    /// Mod lifecycle event
    /// </summary>
    public interface IModHandler
    {
        /// <summary>
        /// Get call right after PersistentData is Loaded
        /// </summary>
        void OnModInit();

        /// <summary>
        /// Get call right after PersistentData is Saved
        /// </summary>
        void OnModDeinit();
    }

}