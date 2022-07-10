#if UNITY_EDITOR

using Threeyes.Editor;
using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// 在打包前设置场景的视频，适用于OEM设置对应的视频，既可以将视频打包，又能够避免场景引用多余的视频
/// 配置方法：：
/// 1.
/// </summary>
[CreateAssetMenu(menuName = EditorDefinition.AssetMenuPrefix_SO_Build + "SOSceneModifier_Video")]
public class SOSceneModifier_Video : SOSceneModifierBase
{
    static string defaultName = "ModifySceneOnPreprocessBuild_Video";
    static string pathInResources = defaultName;//该Manager在Resources下的路径，默认是Resources根目录


    public string goTargetName = "VideoPlayer";//游戏物体的名字
    public string clipAssetName = "Promotional";//从CompanyConfig中读取资源的名字

    protected override void ModifyScene()
    {
        //修改视频源
        GameObject goTarget = EditorTool.FindTargetInScene(goTargetName);
        if (goTarget)
        {
            VideoPlayer videoPlayer = goTarget.GetComponent<VideoPlayer>();
            if (videoPlayer)
            {
                SOCompanyConfig sOCompanyConfig = SOBuildInfoManager.Instance.curCompanyConfig;
                if (sOCompanyConfig)
                {
                    VideoClip clip = sOCompanyConfig.TryGetVideoClipAsset(clipAssetName);
                    if (clip)
                    {
                        videoPlayer.clip = clip;
                        Debug.Log("将" + goTargetName + " 物体中的视频修改为 " + clip.name);
                    }
                    else
                    {
                        Debug.Log("修改视频失败！");
                    }
                }
                else
                {
                    Debug.Log("没有SOCompanyConfig！");
                }
            }
        }

    }
}

#endif