using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxManager : InstanceBase<SkyboxManager>
{
    public Material matSkybox { get { return matOverride ? matOverride : RenderSettings.skybox; } }

    public Material matOverride;
    public bool isAutoRotate = true;
    public float rotationSpeed = 1;

    public float CurRotation
    {
        get { return matSkybox ? matSkybox.GetFloat("_Rotation") : 0; }
        set { if (matSkybox) matSkybox.SetFloat("_Rotation", value); }
    }

    public void SetMaterial(Material material)
    {
        RenderSettings.skybox = material;
    }
    public void SetTexture(Texture texture)
    {
        if (matSkybox)
        {
            matSkybox.mainTexture = texture;
        }
    }
    private void Update()
    {
        if (isAutoRotate)
            CurRotation += rotationSpeed * Time.deltaTime;
    }

}
