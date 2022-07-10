using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 更改该物体及子物体的材质
/// </summary>
[System.Obsolete("Use RendererHelper instead.")]
public class RendererHelper_Wasted : MonoBehaviour
{
    public Transform target;
    public Material targetMaterial;
    public int layer = 0;
    public bool isIncludeChild = false;

    [ContextMenu("OnValidate")]
    private void OnValidate()
    {
        //自动更新
        RendererHelper reHelper = gameObject.AddComponentOnce<RendererHelper>();
        if (target)
        {
            reHelper.Comp = target.GetComponent<Renderer>();
            reHelper.targetMaterial = targetMaterial;
            reHelper.layer = layer;
            reHelper.isIncludeChild = isIncludeChild;

            print(name + " AutoUpdate Complete！");
        }
    }

    [ContextMenu("Change")]
    public void Change()
    {
        target = target ? target : transform;
        if (isIncludeChild)
        {
            target.Recursive((tf) =>
            {
                SetMaterial(tf);
            });
        }
        else
        {
            SetMaterial(target);
        }
    }

    void SetMaterial(Component component)
    {
        MeshRenderer meshRenderer = component.GetComponent<MeshRenderer>();
        if (meshRenderer)
        {
            if (layer == 0)
                meshRenderer.material = targetMaterial;
            else
            {
                if (meshRenderer.materials.Length >= layer + 1)
                {
                    //返回的是一个数组引用，需要更改此数组 Note that like all arrays returned by Unity, this returns a copy of materials array. If you want to change some materials in it, get the value, change an entry and set materials back.
                    Material[] mats = meshRenderer.materials;
                    mats[layer] = targetMaterial;
                    meshRenderer.materials = mats;
                }
            }

        }
    }
}
