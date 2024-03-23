using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Threeyes.RuntimeEditor
{
    /// <summary>
    /// Mark a gameobject as selectable
    /// 
    /// PS：
    /// -当射线选中碰撞体后，会优先扫描并选中首个带该接口的父物体（包括自身）
    /// </summary>
    public interface IRuntimeEditorSelectable { }
}