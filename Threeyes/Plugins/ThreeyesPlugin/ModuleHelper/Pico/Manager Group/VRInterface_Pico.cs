#if USE_PicoMobileSDK
using System.Collections;
using System.Collections.Generic;
using Threeyes.Coroutine;
using UnityEngine;

public partial class VRInterface : InstanceBase<VRInterface>
{
    public Transform leftController;
    public Transform rightController;

    private void HeadsetFadeFunc(float duration)
    {
        StartCoroutine(ScreenFade(duration));
    }

    private void HeadsetReleaseFadeFunc(float duration)
    {
        StartCoroutine(ScreenReleaseFade(duration));
    }

    private void Start()
    {
        StopIEWaitTime();
        CoroutineManager.Instance.StartCoroutine(IEWaitForTime(1f));
    }



#region Screen Fade
    //[Tooltip("If true, specific color gradient when switching scenes.")]
    //public bool screenFade = false;
    //[Tooltip("Define the duration of screen fade.")]
    //public float fadeTime = 5.0f;
    //[Tooltip("Define the color of screen fade.")]
    //public Color fadeColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
    private int renderQueue = 5000;
    private MeshRenderer fadeMeshRenderer;
    private MeshFilter fadeMeshFilter;
    private Material fadeMaterial = null;
    private float elapsedTime;
    private bool isFading = false;
    private float currentAlpha;
    float nowFadeAlpha;


    public  void CreateFadeMesh()
    {


        fadeMaterial = new Material(Shader.Find("Pvr_UnitySDK/Fade"));

        fadeMeshFilter = Instance.tfCameraEyeOverride.transform.gameObject.GetComponent<MeshFilter>();
        if(!fadeMeshFilter)
        fadeMeshFilter =  Instance.tfCameraEyeOverride.transform.gameObject.AddComponent<MeshFilter>();

        fadeMeshRenderer = Instance.tfCameraEyeOverride.transform.gameObject.GetComponent<MeshRenderer>();
        if(!fadeMeshRenderer)
        fadeMeshRenderer = Instance.tfCameraEyeOverride.transform.gameObject.AddComponent<MeshRenderer>();

        var mesh = new Mesh();
        fadeMeshFilter.mesh = mesh;

        Vector3[] vertices = new Vector3[4];

        float width = 2f;
        float height = 2f;
        float depth = 1f;

        vertices[0] = new Vector3(-width, -height, depth);
        vertices[1] = new Vector3(width, -height, depth);
        vertices[2] = new Vector3(-width, height, depth);
        vertices[3] = new Vector3(width, height, depth);

        mesh.vertices = vertices;

        int[] tri = new int[6];

        tri[0] = 0;
        tri[1] = 2;
        tri[2] = 1;

        tri[3] = 2;
        tri[4] = 3;
        tri[5] = 1;

        mesh.triangles = tri;

        Vector3[] normals = new Vector3[4];

        normals[0] = -Vector3.forward;
        normals[1] = -Vector3.forward;
        normals[2] = -Vector3.forward;
        normals[3] = -Vector3.forward;

        mesh.normals = normals;

        Vector2[] uv = new Vector2[4];

        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(1, 0);
        uv[2] = new Vector2(0, 1);
        uv[3] = new Vector2(1, 1);

        mesh.uv = uv;

    }

    private void DestoryFadeMesh()
    {
        if (fadeMeshRenderer != null)
            Destroy(fadeMeshRenderer);

        if (fadeMaterial != null)
            Destroy(fadeMaterial);

        if (fadeMeshFilter != null)
            Destroy(fadeMeshFilter);
    }

    public void SetCurrentAlpha(float alpha)
    {
        currentAlpha = alpha;
        SetMaterialAlpha();
    }

    /// <summary>
    /// Fades alpha from startAlpha to endAlpha
    /// </summary>
    IEnumerator ScreenFade(float duration)
    {
        float elapsedTime = 0.0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            nowFadeAlpha = Mathf.Lerp(0, 1, Mathf.Clamp01(elapsedTime / duration));
            SetMaterialAlpha();
            yield return new WaitForEndOfFrame();
        }
    }

    /// <summary>
    /// Fades alpha from startAlpha to endAlpha
    /// </summary>
    IEnumerator ScreenReleaseFade(float duration )
    {
        float elapsedTime = 0.0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            nowFadeAlpha = Mathf.Lerp(1, 0, Mathf.Clamp01(elapsedTime / duration));
            SetMaterialAlpha();
            yield return new WaitForEndOfFrame();
        }
    }

    /// <summary>
    /// show mesh if alpha>0
    /// </summary>
    private void SetMaterialAlpha()
    {
        Color color = Color.black;
        color.a = Mathf.Max(currentAlpha, nowFadeAlpha);
        isFading = color.a > 0;
        if (fadeMaterial != null)
        {
            fadeMaterial.color = color;
            fadeMaterial.renderQueue = renderQueue;
            fadeMeshRenderer.material = fadeMaterial;
            fadeMeshRenderer.enabled = isFading;
        }
    }



#endregion

#region Utility
    Coroutine cacheEnum;


    public void StopIEWaitTime()
    {
        if (cacheEnum != null)
            CoroutineManager.Instance.StopCoroutine(cacheEnum);
    }

    public IEnumerator IEWaitForTime(float time)
    {
        yield return new WaitForSeconds(time);
        CreateFadeMesh();
        SetCurrentAlpha(0);
    }
#endregion
}
#endif