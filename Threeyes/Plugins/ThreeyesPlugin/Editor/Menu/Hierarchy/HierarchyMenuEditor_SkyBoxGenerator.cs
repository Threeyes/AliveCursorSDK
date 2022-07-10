#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
/// <summary>
/// 在选中物体的位置生成天空盒
/// 可以挂到物体上并修改参数（如faceSize）
/// </summary>
public class HierarchyMenuEditor_SkyBoxGenerator : MonoBehaviour
{
    static int faceSizeStatic = 512;
    static string skyBoxsDirectory = "Assets/Skyboxes";
    static string panoramicDirectory = "Assets/Panoramics";
    static string skyboxShader = "RenderFX/Skybox";

    static string[] skyBoxImage = new string[] { "front", "right", "back", "left", "up", "down" };
    static string[] skyBoxProps = new string[] { "_FrontTex", "_RightTex", "_BackTex", "_LeftTex", "_UpTex", "_DownTex" };

    static Vector3[] skyDirection = new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, -90, 0), new Vector3(0, 180, 0), new Vector3(0, 90, 0), new Vector3(-90, 0, 0), new Vector3(90, 0, 0) };

    public int faceSize = 512;
    [ContextMenu("BakeCubemap")]
    public void BakeCubeMap()
    {
        BakeFunc(faceSize, true);
    }

    [ContextMenu("BakeSkybox")]
    public void BakeSkybox()
    {
        BakeFunc(faceSize, false);
    }

    #region Menu

    const string SkyBoxGeneratorPrefix = EditorDefinition.HierarchyMenuItemPrefix + "SkyBoxGenerator/";

    [MenuItem(SkyBoxGeneratorPrefix + "Bake Panoramic", false, -100)]
    static void BakePanoramicStatic()
    {
        //BakeFunc(faceSizeStatic, false);
    }

    //ToAdd:增加爱不同分辨率的选项
    //注意：只能通过菜单调用，如果在物体对象上调用，会调用多次
    [MenuItem(SkyBoxGeneratorPrefix + "Bake Skybox", false, -100)]
    static void BakeSkyboxStatic()
    {
        BakeFunc(faceSizeStatic, false);
    }
    [MenuItem(SkyBoxGeneratorPrefix + "Bake Cubemap", false, -101)]
    static void BakeCubemapStatic()
    {
        BakeFunc(faceSizeStatic, true);
    }
    #endregion

    private static void BakeFunc(int faceSize, bool isGenerateCubemap = true)
    {
        if (Selection.transforms.Length == 0)
        {
            Debug.LogWarning("Select at least one scene object as a skybox center!");
            return;
        }
        string outputDir = skyBoxsDirectory;
        outputDir += "/" + DateTime.Now.ToString("yyyyMMddHHmmssffff");

        int i = 0;

        foreach (GameObject go in Selection.gameObjects)
        {
            string finalOutputPath = outputDir + "_" + i;
            if (!Directory.Exists(finalOutputPath))
                Directory.CreateDirectory(finalOutputPath);

            Transform t = go.transform;
            RenderSkybox(t, faceSize, finalOutputPath, isGenerateCubemap);//目录重置 ，防止覆盖
            i++;
        }
        AssetDatabase.Refresh();
    }

    static void RenderSkybox(Transform t, int faceSize, string outputDir, bool isGenerateCubemap = true)
    {
        GameObject go = new GameObject("SkyboxCamera", typeof(Camera));
        Camera cam = go.GetComponent<Camera>();

        cam.backgroundColor = Color.black;
        cam.clearFlags = CameraClearFlags.Skybox;
        cam.fieldOfView = 90;
        cam.aspect = 1.0f;

        go.transform.position = t.position;
        go.transform.rotation = Quaternion.identity;

        if (isGenerateCubemap)
        {
            Cubemap cubemap = new Cubemap(faceSize, TextureFormat.RGBA32, true);
            if (cam.RenderToCubemap(cubemap))
            {

                string assetPath = Path.Combine(outputDir, t.name + "_cubemap" + ".cubemap");
                AssetDatabase.CreateAsset(cubemap, assetPath);
                Cubemap cubemapInst = AssetDatabase.LoadAssetAtPath<Cubemap>(assetPath);

                var refle = t.GetComponent<ReflectionProbe>();
                if (refle)
                    refle.customBakedTexture = cubemapInst;
            }
            else
            {
                Debug.LogError("生成CubeMap失败！");
            }
        }
        else
        {
            //Render skybox        
            for (int orientation = 0; orientation < skyDirection.Length; orientation++)
            {
                string assetPath = Path.Combine(outputDir, t.name + "_" + skyBoxImage[orientation] + ".png");
                RenderSkyBoxFaceToPNG(orientation, cam, assetPath, faceSize);
            }

            //Wire skybox material
            AssetDatabase.Refresh();

            Material skyboxMaterial = new Material(Shader.Find(skyboxShader));
            for (int orientation = 0; orientation < skyDirection.Length; orientation++)
            {
                string texPath = Path.Combine(outputDir, t.name + "_" + skyBoxImage[orientation] + ".png");
                Texture2D tex = (Texture2D)AssetDatabase.LoadAssetAtPath(texPath, typeof(Texture2D));
                tex.wrapMode = TextureWrapMode.Clamp;
                skyboxMaterial.SetTexture(skyBoxProps[orientation], tex);
            }

            //Save material
            string matPath = Path.Combine(outputDir, t.name + "_skybox" + ".mat");
            AssetDatabase.CreateAsset(skyboxMaterial, matPath);

            Selection.activeObject = skyboxMaterial;

        }

        GameObject.DestroyImmediate(go);
    }

    static void RenderSkyBoxFaceToPNG(int orientation, Camera cam, string assetPath, int faceSize)
    {
        cam.transform.eulerAngles = skyDirection[orientation];
        RenderTexture rt = new RenderTexture(faceSize, faceSize, 24);
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;

        Texture2D screenShot = new Texture2D(faceSize, faceSize, TextureFormat.RGB24, false);
        screenShot.ReadPixels(new Rect(0, 0, faceSize, faceSize), 0, 0);

        RenderTexture.active = null;
        //GameObject.DestroyImmediate(rt);

        byte[] bytes = screenShot.EncodeToPNG();
        File.WriteAllBytes(assetPath, bytes);

        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
    }
}
#endif