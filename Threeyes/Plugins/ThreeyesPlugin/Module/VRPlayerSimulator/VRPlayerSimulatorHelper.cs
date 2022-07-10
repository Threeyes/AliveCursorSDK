using System.Collections;
using Threeyes.Coroutine;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// 通用的玩家帮助类，用于
/// 使玩家的手柄指向点击位置
/// </summary>
public class VRPlayerSimulatorHelper : InstanceBase<VRPlayerSimulatorHelper>
{

    public bool IsActive { get { return isActive; } set { isActive = value; } }

    public Transform TfRig
    {
        get
        {
            return tfRig ? tfRig : transform;
        }
        set
        {
            tfRig = value;
        }
    }

    public bool isActive = true;

    public Camera camMain;
    public Transform tfRightController;//控制器，带有含PhysicsRayCaster的子物体
    public Transform tfRig;

    public Transform tfRayEndPointMesh;//射线末端的模型，可空
    public UnityEvent onLookAt;//朝向完成
    public BoolEvent onLookAtDownUp;//朝向完成，并且按下/抬起按键

    public void RotateHorizontal(bool isRight)
    {
        TfRig = VRInterface.tfCameraRig;
    }

    public void SetPosition(Vector3 point)
    {
        TfRig.position = point;
    }
    public void SetRotation(Quaternion rotation)
    {
        TfRig.rotation = rotation;
    }

    /// <summary>
    /// 朝当前方向、当前光标位置发射射线
    /// </summary>
    /// <param name="isKeyDown"></param>
    public void RaycastToTarget(bool isKeyDown)
    {
        if (!IsActive && isKeyDown)
            IsActive = true;

        if (!IsActive)
            return;


        if (!tfRightController)
        {
            GameObject goRightHand = null;
#if USE_VRTK
            goRightHand = VRTK.VRTK_DeviceFinder.GetControllerRightHand();
#endif
            if (goRightHand)
                tfRightController = goRightHand.transform;
        }

        if (!tfRightController)
            camMain = VRInterface.vrCamera;

        if (!camMain || !tfRightController)
        {
            Debug.LogError("未找到指定物体！");
            return;
        }


        Ray ray = camMain.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Vector3 endPoint = hit.point;
            tfRightController.LookAt(endPoint);//设置控制器的朝向
            ShowRayEndMesh(true, endPoint);
        }

        if (isKeyDown)
        {
            CoroutineManager.StartCoroutineEx(IENotify());
        }
        else
        {
            onLookAtDownUp.Invoke(false);
            IsActive = false;
        }
    }

    void ShowRayEndMesh(bool isShow, Vector3 worldPoint = default(Vector3))
    {
        if (tfRayEndPointMesh)
        {
            tfRayEndPointMesh.gameObject.SetActive(isShow);
            if (isShow)
                tfRayEndPointMesh.position = worldPoint;
        }
    }
    IEnumerator IENotify()
    {
        yield return new WaitForEndOfFrame();
        onLookAt.Invoke();
        onLookAtDownUp.Invoke(true);
    }
}
