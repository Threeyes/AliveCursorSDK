using System;
using System.Collections.Generic;
using UnityEngine;

namespace Threeyes.KeyLock
{
    /// <summary>
    /// Use this object as a generic way to validate if an object can perform some action.
    /// The check is done in the <see cref="CanUnlock"/> method.
    /// This class is used in combination with a <see cref="Keychain"/> component.
    /// 
    /// Ref: UnityEngine.XR.Content.Interaction.Lock
    /// </summary>
    [Serializable]
    public class Lock
    {
        /// <summary>
        /// If this Lock require any keys
        /// </summary>
        public bool IsValid { get { return m_RequiredKeys != null && m_RequiredKeys.Count > 0; } }

        [SerializeField]
        [Tooltip("The required keys to unlock this lock")]
        List<SOKey> m_RequiredKeys;

        /// <summary>
        /// Returns the required keys to unlock this lock.
        /// </summary>
        public List<SOKey> requiredKeys => m_RequiredKeys;

        /// <summary>
        /// Checks if the supplied keychain has all the required keys to open this lock.
        /// </summary>
        /// <param name="keychain">The keychain to be checked.</param>
        /// <returns>True if the supplied keychain has all the required keys; false otherwise.</returns>
        public bool CanUnlock(IKeychain keychain)
        {
            if (keychain == null)
                return m_RequiredKeys.Count == 0;

            foreach (var requiredKey in m_RequiredKeys)
            {
                if (requiredKey == null)
                    continue;

                if (!keychain.Contains(requiredKey))
                    return false;
            }

            return true;
        }
    }
}
