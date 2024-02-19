using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Threeyes.KeyLock
{
    /// <summary>
    /// An asset that represents a key. Used to check if an object can perform some action
    /// (<see cref="Keychain"/>)
    /// 
    /// Ref: UnityEngine.XR.Content.Interaction.Key
    /// </summary>
    [CreateAssetMenu(menuName = EditorDefinition_KeyLock.AssetMenuPrefix_KeyLock + "Key", fileName = "Key")]
    public class SOKey : ScriptableObject
    { }
}