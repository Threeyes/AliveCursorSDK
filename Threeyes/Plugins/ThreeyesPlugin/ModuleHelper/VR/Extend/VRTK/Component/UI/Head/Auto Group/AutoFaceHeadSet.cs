using UnityEngine;

/// <summary>
/// 永远朝向头部，常用于UI
/// PS:因为是Z轴朝前，因此UI要翻转
/// </summary>
public class AutoFaceHeadSet : MonoBehaviour
{
    public bool alwaysFaceHeadset = true;
    //protected Transform headset;
    //protected virtual void Awake()
    //{
    //    VRTK_SDKManager.instance.AddBehaviourToToggleOnLoadedSetupChange(this);
    //}

    //protected virtual void OnEnable()
    //{
    //    headset = VRTK_DeviceFinder.HeadsetTransform();
    //}

    //protected virtual void OnDestroy()
    //{
    //    VRTK_SDKManager.instance.RemoveBehaviourToToggleOnLoadedSetupChange(this);
    //}

    protected virtual void Update()
    {
        if (alwaysFaceHeadset)
        {
            //var direction = transform.position - headset.position;
            //transform.rotation = Quaternion.FromToRotation(Vector3.forward, (transform.position - headset.position).normalized);//反方向
            //transform.LookAt(headset);
            Transform tfHeadSet = VRInterface.tfCameraEye;
            if (tfHeadSet)
                transform.LookAt(tfHeadSet);
            //transform.LookAt(headset, transform.up);
        }
    }
}
