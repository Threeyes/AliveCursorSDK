using UnityEngine;

#if UNITY_EDITOR
using UnityEditor.Build;
#endif

/// <summary>
/// 在打包前修改场景，适用于更改OEM视频
/// </summary>
public abstract class SOSceneModifierBase : ScriptableObject
{
    public SOSceneInfo sOSceneInfo;

#if UNITY_EDITOR

    public virtual void OnBeforeBuild()
    {
        //打开对应场景并修改
        OpenScene();
        ModifyScene();
    }

    protected virtual void OpenScene()
    {
        if (sOSceneInfo)
        {
            sOSceneInfo.OpenSceneInEditor();
        }
    }

    protected virtual void ModifyScene()
    {
    }

    public virtual void OnAfterBuild()
    {

    }

#endif

}
