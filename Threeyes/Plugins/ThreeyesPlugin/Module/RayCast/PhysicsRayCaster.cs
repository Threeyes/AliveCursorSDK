using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//[ExecuteInEditMode]
public class PhysicsRayCaster : ShowAndHide
{
    public RayCasterType rayCasterType = RayCasterType.Camera;

    public Camera camRaycastOverride;//指定相机
    public Transform tfOriginRaycastOverride;//指定物体

    public UnityEngine.Events.UnityEvent onAfterHover;

    public string id = "";

    public int activeFrame = -1;//激活的帧数，-1代表无限，1代表指定帧数

    public bool isResetCurOnDisable = true;//隐藏时重置当前选中对象

    [Header("Debug")]
    public Text textDebug;
    public bool showDebugRay = false;

    public Transform Target
    {
        get
        {
            switch (rayCasterType)
            {
                case RayCasterType.Camera:
                    return CamRaycast.transform;
                case RayCasterType.Origin:
                    return TfOriginRaycast;
            }

            return TfOriginRaycast;
        }
    }
    protected virtual Camera CamRaycast
    {
        get
        {
            if (camRaycastOverride)
                return camRaycastOverride;
            return Camera.main;
        }
    }

    Transform TfOriginRaycast
    {
        get
        {
            if (tfOriginRaycastOverride)
                return tfOriginRaycastOverride;
            return transform;
        }
    }

    public Ray ray = default(Ray);
    public RaycastHit raycastHit;
    public PhysicsRaycastRecevier curPhysicsRaycastRecevier;
    public bool isHit = false;

    protected virtual void OnDisable()
    {
        //Warning:如果隐藏后清空数据，那么上次的得Receiver就不会收到Hover(false)
        //清空数据
        if (curPhysicsRaycastRecevier)
        {
            if (isResetCurOnDisable)
            {
                curPhysicsRaycastRecevier.Hover(false);
                curPhysicsRaycastRecevier = null;
            }
        }
    }

    public UpdateRaycastType updateRaycastType = UpdateRaycastType.Update;
    public enum UpdateRaycastType
    {
        Update,
        Manual
    }

    protected virtual void Update()
    {
        //每帧更新
        if (updateRaycastType == UpdateRaycastType.Update)
        {
            UpdateCaster();
        }
    }

    public void ManualCast()
    {
        if (updateRaycastType == UpdateRaycastType.Manual)
            UpdateCaster();
    }

    private void UpdateCaster()
    {
        switch (rayCasterType)
        {
            case RayCasterType.Camera:
                if (!CamRaycast)
                    return;
                ray = CamRaycast.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); break;
            case RayCasterType.Origin:
                ray = new Ray(TfOriginRaycast.position, TfOriginRaycast.forward); break;
        }

        isHit = false;
        if (Physics.Raycast(ray, out raycastHit))
        {
#if UNITY_EDITOR
            if (showDebugRay)
                Debug.DrawRay(CamRaycast.transform.position, ray.direction);
#endif

            Collider collider = raycastHit.collider;
            if (collider)
            {
                if (textDebug)
                    textDebug.text = collider.transform.name;
                var receiver = collider.GetComponent<PhysicsRaycastRecevier>();
                if (receiver)
                {
                    if (receiver != curPhysicsRaycastRecevier)//1.两个不同  2.curPhysicsRaycastRecevier为null
                    {
                        if (id.IsNullOrEmpty() && receiver.targetId.IsNullOrEmpty() || receiver.targetId.NotNullOrEmpty() && receiver.targetId == id)
                        {
                            //调用上一目标的Hover(false)
                            if (curPhysicsRaycastRecevier)
                                curPhysicsRaycastRecevier.Hover(false);

                            curPhysicsRaycastRecevier = receiver;
                            receiver.Hover(true);
                            onAfterHover.Invoke();
                        }
                    }
                    else
                    {
                        //相同的物体
                    }
                    isHit = true;
                }
            }
        }

        //调用方法
        if (isHit)
        {

        }
        else
        {
            //设置上一目标为UnHover
            if (curPhysicsRaycastRecevier)
                curPhysicsRaycastRecevier.Hover(false);
            curPhysicsRaycastRecevier = null;
            isHit = false;
        }
    }

    [ContextMenu("Use")]
    public void Use()
    {
        if (curPhysicsRaycastRecevier)
            curPhysicsRaycastRecevier.Use(raycastHit);
    }

    public enum RayCasterType
    {
        Camera,//基于相机
        Origin,//基于原点
    }
}
