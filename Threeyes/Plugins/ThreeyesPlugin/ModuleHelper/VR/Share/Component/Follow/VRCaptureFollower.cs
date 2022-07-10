
using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
/// <summary>
/// 设置录制相机的持续跟随方式
/// </summary>
public class VRCaptureFollower : MonoBehaviour
{
    static string rootName = "VR Capture Camera Root";
    static VRCaptureFollower Instance;

    public Camera cam;
    Transform tfCam;
    Transform tfRoot = null;//默认根物体为空
    public bool isSyncYRotation = false;//同步相机的Y轴朝向
    public bool isSyncCameraRigParent = false;//同步VR相机的父物体(常用于开车、坠落模拟等需要跟随其他物体位移的操作)

    public bool isSetAsVRCameraChild = false;//直接作为VR相机的子物体

    private void Awake()
    {
        if (!cam)
            cam = GetComponentInChildren<Camera>();
        if (!cam)
        {
            Debug.LogError("No Camera!");
        }
        tfCam = cam.transform;


        VRInterface.RegistOnHMDLoaded(SetUpVRCam);

    }
    void SetUpVRCam()
    {
        if (isSetAsVRCameraChild)
            tfCam.SetParent(VRInterface.tfCameraEye);
    }

    private void OnEnable()
    {
#if USE_VRTK

        RemoteDestinationPoint.actionTeleportFinished += OnTeleportFinished;
        VRInterface.Instance.actOnSetCameraRigParent += SetCameraRigParent;
#endif
    }

    private void OnDisable()
    {
#if USE_VRTK
        RemoteDestinationPoint.actionTeleportFinished -= OnTeleportFinished;
#endif
    }

    private void SetCameraRigParent(Transform target, bool isValidObj)
    {
        if (isSyncCameraRigParent)
        {
            if (isValidObj)
                tfCam.SetParent(target);
            else
            {
                tfCam.SetParent(tfRoot);
            }
        }
    }


    void OnTeleportFinished(RemoteDestinationPoint destinationPoint)
    {
        //设置位置
        if (!VRInterface.tfCameraEye || !tfCam)
            return;

        tfCam.position = VRInterface.tfCameraEye.position;

        //设置旋转

        //若需要对齐路标，则保留X轴
        if (destinationPoint.isAlignToDP)
        {
            Vector3 rotDestinationPoint = destinationPoint.transform.eulerAngles;
            tfCam.eulerAngles = rotDestinationPoint;//对齐路标
        }
        else
        {
            Vector3 rotVRCam = VRInterface.tfCameraEye.eulerAngles;
            rotVRCam.Scale(new Vector3(0, 1, 0));//只保留Y轴
            tfCam.eulerAngles = rotVRCam;
        }

        if (isSyncCameraRigParent)
        {
            tfCam.SetParent(VRInterface.tfCameraRig);
        }

    }

    private void Update()
    {
        //Todo：应该是同步相机的X、Y轴朝向
        if (isSyncYRotation)
            SyncRotationAtOnce();

        if (Input.GetKeyDown(KeyCode.F))
            SyncRotationAtOnce();
    }

    private void SyncRotationAtOnce()
    {
        if (VRInterface.tfCameraEye)
        {
            Vector3 rotVRCam = VRInterface.tfCameraEye.eulerAngles;
            rotVRCam.Scale(new Vector3(0, 1, 0));//只保留Y轴
            tfCam.eulerAngles = rotVRCam;
        }
    }


#if UNITY_EDITOR
    #region Editor

    [MenuItem(EditorDefinition.TopMenuItemPrefix + "AVPro视频录制/" + "初始化全景视频录像")]
    static void Init()
    {
        GameObject rootInst = GetRootInst();
        if (!rootInst)
        {

            string[] arrStr = AssetDatabase.FindAssets("VR Capture Camera Root");
            if (arrStr.Length > 0)
            {
                string GUIDs = arrStr[0];
                string path = AssetDatabase.GUIDToAssetPath(GUIDs);
                Debug.Log(AssetDatabase.GUIDToAssetPath(GUIDs));
                GameObject goPre = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(GUIDs));
                rootInst = PrefabUtility.InstantiatePrefab(goPre) as GameObject;//克隆一个有链接关系的物体
                rootInst.name = rootName;
                Debug.Log("初始化完成");
            }
            else
            {
                Debug.LogError("找不到指定的预制物！");
            }
        }
    }

    [MenuItem(EditorDefinition.TopMenuItemPrefix + "AVPro视频录制/" + "结束全景视频录像")]
    static void Finish()
    {
        GameObject rootInst = GetRootInst();
        if (rootInst)
            DestroyImmediate(rootInst);
    }

    static GameObject GetRootInst()
    {
        return GameObject.Find(rootName);
    }

    #endregion
#endif

}

