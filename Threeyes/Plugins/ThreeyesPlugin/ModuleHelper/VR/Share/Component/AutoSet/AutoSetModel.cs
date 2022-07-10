#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
/// <summary>
/// 自动设置模型的属性
/// </summary>
public class AutoSetModel : MonoBehaviour
{
    public Material matHL;

    [ContextMenu("SetHighLight")]
    public void SetHighLight()
    {
        transform.ForEachChildComponent<Renderer>(
            (r) =>
            {
                //返回的是一个数组引用，需要更改此数组 Note that like all arrays returned by Unity, this returns a copy of materials array. If you want to change some materials in it, get the value, change an entry and set materials back.
                Material[] mats = r.materials;
                for (int i = 0; i != mats.Length; i++)
                {
                    mats[i] = matHL;
                }
                Undo.RegisterFullObjectHierarchyUndo(r, "SetHighLight" + r.name);

                r.materials = mats;
            });

    }

}
#endif