#if USE_VRTK
using UnityEngine;
using UnityEngine.Events;
using VRTK;
/// <summary>
/// 常见的事件
/// ——SDK初始化完成
/// </summary>
public class VRCommonEventListener : MonoBehaviour
{
    public UnityEvent onLoadedSetupChanged;


    private void Awake()
    {
        VRTK_SDKManager.instance.LoadedSetupChanged += OnLoadedSetupChanged;//保持贴在当前的相机前
    }

    private void OnLoadedSetupChanged(VRTK_SDKManager sender, VRTK_SDKManager.LoadedSetupChangeEventArgs e)
    {
        onLoadedSetupChanged.Invoke();
    }
}
#endif