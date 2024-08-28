using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threeyes.GameFramework
{
    /// <summary>
    /// Mark this gameobject as can't set to buoyant by BuoyantVolumeController
    /// 标记该物体不可漂浮，可避免意外修改
    /// </summary>
    public interface INotBuoyantableObject { }

    /// <summary>
    /// Mark this gameobject as can't set to buoyant by BuoyantVolumeController
    /// 标记该物体不可漂浮，可避免意外修改
    /// </summary>
    public class NotBuoyantableObject : MonoBehaviour, INotBuoyantableObject
    {
    }
}
