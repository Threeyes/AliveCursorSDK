using System.Collections.Generic;
using UnityEngine;

namespace Threeyes.KeyLock
{
    /// <summary>
    /// A generic Keychain component that holds the <see cref="SOKey"/>s to open a <see cref="Lock"/>.
    /// Attach a Keychain component to an Interactable and assign to it the same Keys of an <see cref="XRLockSocketInteractor"/>
    /// or an <see cref="XRLockGridSocketInteractor"/> to open (or interact with) them.
    /// 
    /// Ref: UnityEngine.XR.Content.Interaction.Keychain
    /// </summary>
    [DisallowMultipleComponent]
    public class Keychain : MonoBehaviour, IKeychain
    {
        [SerializeField]
        [Tooltip("The keys on this keychain")]
        List<SOKey> m_Keys = new List<SOKey>();


        //#ToDelete：原实现通过HashSet的方式来缓存Key，没必要，而且容易出错（比如通过uMod加载后，HashSet未及时更新）
        //HashSet<int> m_KeysHashSet = new HashSet<int>();
        //protected virtual void Awake()
        //{
        //    RepopulateHashSet();
        //}
        //protected virtual void OnValidate()
        //{
        //    //#编辑器模式更改时更新
        //    // A key was added through the inspector while the game was running: Update
        //    if (Application.isPlaying && m_Keys.Count != m_KeysHashSet.Count)
        //        RepopulateHashSet();
        //}
        //void RepopulateHashSet()
        //{
        //    m_KeysHashSet.Clear();
        //    foreach (var key in m_Keys)
        //    {
        //        if (key != null)
        //            m_KeysHashSet.Add(key.GetInstanceID());
        //    }
        //}

        /// <summary>
        /// Adds the supplied key to this keychain
        /// </summary>
        /// <param name="key">The key to be added to the keychain</param>
        public void AddKey(SOKey key)
        {
            if (key == null || Contains(key))
                return;

            m_Keys.Add(key);
            //m_KeysHashSet.Add(key.GetInstanceID());
        }

        /// <summary>
        /// Adds the supplied key from this keychain
        /// </summary>
        /// <param name="key">The key to be removed from the keychain</param>
        public void RemoveKey(SOKey key)
        {
            m_Keys.Remove(key);

            //if (key != null)
            //    m_KeysHashSet.Remove(key.GetInstanceID());
        }

        /// <inheritdoc />
        public bool Contains(SOKey key)
        {
            return key != null && m_Keys.Contains(key);
        }
    }
}
