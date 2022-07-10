using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 普通的场景物体，需要获取其transform值
/// (Warning:因为新版已经优化了transform的读取速度，而且命名不够规范，所以该组件非必要）
/// </summary>
[System.Obsolete("Use MonoBehaviour",false)]
public class GameObjectBase : MonoBehaviour
{
    /// <summary>
    /// 缓存的transform值
    /// </summary>
    public virtual Transform tfThis
    {
        get
        {
            if (!_tfThis)
                _tfThis = transform;
            return _tfThis;

        }
    }
    protected Transform _tfThis;


    /// <summary>
    /// 缓存的gameObject值
    /// </summary>
    public GameObject goThis
    {
        get
        {
            if (!_goThis)
                _goThis = gameObject;
            return _goThis;

        }
    }
    protected GameObject _goThis;

}
