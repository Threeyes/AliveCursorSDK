using UnityEngine;
#if USE_VRTK
using VRTK;
#endif

/// <summary>
/// 沿着指定的轴向移动，适用于滑轨门等物体
/// PS：建议给物体加上VRTK_TrackObjectGrabAttach
/// </summary>
public class VRSlider :

#if USE_VRTK
 VRTK_Control
#else
 MonoBehaviour
#endif
{
    public float min = 0f;
    public float max = 10f;

    public bool isClamp = true;//限制上下限
    public bool snapToStep = false;//步进

    public Vector3 axis = new Vector3(0, 0, 1);//运行时计算得出轴向

    public Vector3 startLocalPos;
    public Vector3 endLocalPos;

    public Vector3 startLocalRot;


#if USE_VRTK

    VRTK_InteractableObject interactableObject;
    Transform target { get { return transform; } }
    private void OnEnable()
    {
        interactableObject = GetComponent<VRTK_InteractableObject>();
        interactableObject.InteractableObjectTouched += OnInteractableObjectGrabbed;
        interactableObject.InteractableObjectUntouched += OnInteractableObjectUnGrabbed;

    }

    private void OnInteractableObjectGrabbed(object sender, InteractableObjectEventArgs e)
    {
        Active(true);
    }

    private void OnInteractableObjectUnGrabbed(object sender, InteractableObjectEventArgs e)
    {
        Active(false);
    }

    void Active(bool isActive)
    {
        this.enabled = isActive;
    }

    protected override bool DetectSetup()
    {
        return true;
    }

    protected override void InitRequiredComponents()
    {
        startLocalRot = target.localEulerAngles;
    }


    protected override void HandleUpdate()
    {

        //https://answers.unity.com/questions/598060/vector3project-how-to.html
        Vector3 curLocalPos = target.localPosition;

        Vector3 sumDist = endLocalPos - startLocalPos;
        axis = sumDist.normalized;
        Vector3 vet1 = curLocalPos - startLocalPos;
        Vector3 vet2 = Vector3.Project(vet1, axis);//Vet along the axis

        target.localEulerAngles = startLocalRot;
        float maxValue = max - min;

        float newValue = Mathf.Sign(Vector3.Dot(vet2, sumDist)) * (vet2.magnitude / sumDist.magnitude) * maxValue;//计算当前的进度

        if (isClamp)
        {
            if (newValue < 0)//反向
            {
                newValue = 0;
                target.localPosition = startLocalPos;
            }
            else if (newValue > maxValue)
            {
                newValue = maxValue;
                target.localPosition = endLocalPos;
            }
            else
            {
                if (snapToStep)
                    newValue = Mathf.CeilToInt(newValue);
            }
        }
        value = newValue;
        target.localPosition = startLocalPos + sumDist * (value / maxValue);

    }


    protected override ControlValueRange RegisterValueRange()
    {
        return new ControlValueRange()
        {
            controlMin = min,
            controlMax = max
        };
    }
#endif
}
