namespace Threeyes.KeyLock
{
    /// <summary>
    /// Interface to implement for objects that hold a set of <c>Key</c>s
    /// 
    /// Ref: UnityEngine.XR.Content.Interaction.IKeychain
    /// </summary>
    public interface IKeychain
    {
        /// <summary>
        /// This callback is used to check if this keychain has a specific <c>Key</c>
        /// <see cref="Lock"/>
        /// </summary>
        /// <param name="key">the key to be checked</param>
        /// <returns>True if this keychain has the supplied key; false otherwise</returns>
        bool Contains(SOKey key);
    }
}