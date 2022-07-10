using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxManagerRemoteHelper : ComponentRemoteHelperBase<SkyboxManager>
{
    public override SkyboxManager ManagerInstance { get { return skyboxManagerOverride ? skyboxManagerOverride : SkyboxManager.Instance; } }
    public Material TargetMaterial { get { return targetMaterial; } set { targetMaterial = value; } }
    public Texture TargetTexture { get { return targetTexture; } set { targetTexture = value; } }

    public SkyboxManager skyboxManagerOverride;

    [SerializeField]
    protected Texture targetTexture;
    [SerializeField]
    private Material targetMaterial;

    public void SetMaterial()
    {
        if (targetMaterial)
            SetMaterial(targetMaterial);
    }
    public void SetMaterial(Material material)
    {
        if (ManagerInstance)
            ManagerInstance.SetMaterial(material);
    }

    public void SetTexture()
    {
        if (targetTexture)
            SetTexture(targetTexture);
    }

    public void SetTexture(Texture texture)
    {
        if (ManagerInstance)
            ManagerInstance.SetTexture(texture);
    }

    public void SetMaterialAndTexture()
    {
        SetMaterial();
        SetTexture();
    }

}
