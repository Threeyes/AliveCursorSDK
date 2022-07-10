#if USE_VIU
using HTC.UnityPlugin.Vive;
#endif
using UnityEngine;
using UnityEngine.Events;

#if USE_VIU
[RequireComponent(typeof(VivePoseTracker))]
public class VivePoseTrackerHelper : ComponentHelperBase<VivePoseTracker>
#else
     class VivePoseTrackerHelper : MonoBehaviour
#endif
{
    public UnityEvent onFollowStart;
    public UnityEvent onFollowFinish;
    public BoolEvent onFollow;
    public bool isFollowOnAwake = false;

    bool isFollowing = false;


#if USE_VIU
    private void Reset()
    {
        Comp.enabled = false;
    }

    private void Awake()
    {
        Comp.origin = VRInterface.tfCameraRig;//基于相机父物体
        if (isFollowOnAwake)
            Follow(true);
    }


    public void Follow(bool value)
    {
        FollowFunc(value);
        if (value)
        {
            onFollowStart.Invoke();
            onFollow.Invoke(true);
        }
        else
        {
            onFollowFinish.Invoke();
        }
        isFollowing = value;
    }

    [ContextMenu("Follow")]
    public void Follow()
    {
        Follow(true);
    }

    [ContextMenu("UnFollow")]
    public void UnFollow()
    {
        Follow(false);
    }

    public void FollowFunc(bool value)
    {
        Comp.enabled = value;
    }
#endif
}