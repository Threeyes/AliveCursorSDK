using UnityEngine;
#if USE_VRTK
using VRTK;
#elif USE_VIU
using HTC.UnityPlugin.Pointer3D;
#endif

/// <summary>
/// 根据运行平台自行添加对应的插件，以支持UI交互
/// 使用方式：挂在Canvas统计物体下
/// </summary>
[RequireComponent(typeof(Canvas))]
public class VR_UICanvasPlatformDependency : MonoBehaviour
{
    private void Awake()
    {
#if USE_VRTK
        gameObject.AddComponentOnce<VRTK_UICanvas>();
#elif USE_VIU
        gameObject.AddComponentOnce<CanvasRaycastTarget>();
# endif
    }
}
