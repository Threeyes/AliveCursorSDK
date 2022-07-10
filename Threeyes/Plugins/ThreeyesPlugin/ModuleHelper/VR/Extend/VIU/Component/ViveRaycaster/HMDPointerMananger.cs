#if USE_VIU
using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.VRModuleManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HMDPointerMananger : MonoBehaviour
{
    public GameObject goHMDContent;
    public Image imageFocusPercent;
    public uint leftControllerIndex;
    public uint rightControllerIndex;

    public RenderModelHook rMHLeft;
    public RenderModelHook rMHRight;

    bool isLeftControllerActive;
    bool isRightControllerActive;

    private void Awake()
    {
        Transform tfParent = transform.parent;
        rMHLeft = tfParent.Find("Left").FindFirstComponentInChild<RenderModelHook>(isRecursive: true);
        rMHRight = tfParent.Find("Right").FindFirstComponentInChild<RenderModelHook>(isRecursive: true);

    }

    private void OnEnable()
    {
        rMHLeft.viveRole.onDeviceIndexChanged += OnRightDeviceIndexChanged;
        rMHRight.viveRole.onDeviceIndexChanged += OnRightDeviceIndexChanged;
    }

    private void OnDisable()
    {
        rMHLeft.viveRole.onDeviceIndexChanged -= OnRightDeviceIndexChanged;
        rMHRight.viveRole.onDeviceIndexChanged -= OnRightDeviceIndexChanged;
    }

    private void OnLeftDeviceIndexChanged(uint deviceIndex)
    {
        isLeftControllerActive = false;
        if (VRModule.IsValidDeviceIndex(deviceIndex))//检查手柄是否可用
        {
            if (deviceIndex == VRModule.GetLeftControllerDeviceIndex())
            {
                isLeftControllerActive = true;
            }
        }
        UpdateState();
    }

    private void OnRightDeviceIndexChanged(uint deviceIndex)
    {
        isRightControllerActive = false;
        if (VRModule.IsValidDeviceIndex(deviceIndex))//检查手柄是否可用
        {
            if (deviceIndex == VRModule.GetRightControllerDeviceIndex())
            {
                isRightControllerActive = true;
            }
        }
        UpdateState();
    }

    void UpdateState()
    {
        bool isAnyControllerConnected = isLeftControllerActive || isRightControllerActive;
        goHMDContent.SetActive(!isAnyControllerConnected);
    }
}
#endif