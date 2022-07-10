using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// 相机特效
/// </summary>
public class CameraFXHelperBase : MonoBehaviour
{
    /// <summary>
    /// 场景中需要作用的相机
    /// </summary>
    public static List<Camera> listAvaliableCam { get { return VRInterface.listAvaliableCam; } }

    protected bool isMultiCam { get { return listAvaliableCam.Count > 1; } }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="actionMain"></param>
    /// <param name="actionForMultiCam">针对多摄像机进行操作，通常是重置部分属性</param>
    public void SetUpForEachCam(UnityAction actionMain, UnityAction actionForMultiCam = null)
    {
        SetUpForEachCam((cam) => actionMain.Execute(), (cam) => actionForMultiCam.Execute());
    }

    public virtual void SetUpForEachCam(UnityAction<Camera> actionMain, UnityAction<Camera> actionForMultiCam = null)
    {
        //遍历多个相机
        List<Camera> listCam = listAvaliableCam;
        for (int i = 0; i != listCam.Count; i++)
        {
            Camera cam = listCam[i];
            InitForEachCam(ref cam);
            //针对第二个及以后的相机，需要先进行一些清理设置
            if (/*i > 0 &&*/ actionForMultiCam.NotNull())
            {
                actionForMultiCam.Execute(cam);
            }
            actionMain.Execute(cam);
        }
    }

    public virtual void InitForEachCam(ref Camera cam)
    {
        //初始化各相机
    }
}
