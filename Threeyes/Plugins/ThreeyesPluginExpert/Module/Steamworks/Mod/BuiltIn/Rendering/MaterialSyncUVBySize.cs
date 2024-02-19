using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 根据模型的缩放，同步UV的Scale
/// 
/// 适用于：
/// -洞洞板等需要表面重复且贴图不变的模型
/// 
/// ToAdd：
/// -参考此实现，增加基于位置更改UVOffset的组件
/// </summary>
public class MaterialSyncUVBySize : MonoBehaviour
{
    public Material Material
    {
        get
        {
            if (targetRenderer)
                return targetRenderer.material;
            return targetMaterial;
        }
    }
    [SerializeField] protected Renderer targetRenderer;//Where the material attached
    [SerializeField] protected Material targetMaterial;//Target material asset
    public string textureName = "_BaseMap";
    public Vector2 uvScalePerUnit = new Vector2(1, 1);//每单元对应的UV Scale值

    //Runtime

    Vector3 curSize;
    private void Awake()
    {
        curSize = transform.lossyScale;
    }
    private void LateUpdate()
    {
        if (transform.lossyScale != curSize)
        {
            if (!Material)
                return;

            curSize = transform.lossyScale;
            Material.SetTextureScale(textureName, curSize * uvScalePerUnit);
        }
    }
}
