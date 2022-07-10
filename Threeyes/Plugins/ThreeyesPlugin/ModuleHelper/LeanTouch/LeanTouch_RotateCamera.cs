using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
#if USE_LeanTouch
using Lean.Touch;
#endif
/// <summary>
/// 触摸屏控制相机旋转
/// </summary>
public class LeanTouch_RotateCamera : MonoBehaviour
{
    public float SensitivityX
    {
        get
        {
            return sensitivityX;
        }

        set
        {
            sensitivityX = value;
        }
    }

    public float SensitivityY
    {
        get
        {
            return sensitivityY;
        }

        set
        {
            sensitivityY = value;
        }
    }

    //上下的限制
    public float minimumY = -60F;
    public float maximumY = 60F;

    public float sensitivityX = 0.5f;
    public float sensitivityY = 0.3f;

    public Transform tfCamRig;
    public Transform tfCamera;

    public BoolEvent onTouchDownUp;
    public UnityEvent onMove;
    public Vector2 vet2MoveThreshold = new Vector2(0.05f, 0.05f);//判断是否移动的阈值(占屏幕像素的百分比）

#if USE_LeanTouch
    protected virtual void OnEnable()
    {
        // Hook into the events we need
        LeanTouch.OnFingerDown += HandleFingerDown;
        LeanTouch.OnFingerSet += HandleFingerSet;
        LeanTouch.OnFingerUp += HandleFingerUp;
        //LeanTouch.OnFingerTap += HandleFingerTap;
        //LeanTouch.OnFingerSwipe += HandleFingerSwipe;
        LeanTouch.OnGesture += HandleGesture;
    }

    protected virtual void OnDisable()
    {
        // Unhook the events
        LeanTouch.OnFingerDown -= HandleFingerDown;
        LeanTouch.OnFingerSet -= HandleFingerSet;
        LeanTouch.OnFingerUp -= HandleFingerUp;
        //LeanTouch.OnFingerTap -= HandleFingerTap;
        //LeanTouch.OnFingerSwipe -= HandleFingerSwipe;
        LeanTouch.OnGesture -= HandleGesture;
    }
#endif

    private void Start()
    {
        originalRotation = tfCamera.localRotation;
    }
    public bool isNeedToRotate;
    Quaternion originalRotation;

    public void ForbidRotate(bool isRotate)
    {
        isNeedToRotate = isRotate;
    }

#if USE_LeanTouch
    public void HandleFingerDown(LeanFinger finger)
    {
        onTouchDownUp.Invoke(true);
        //Debug.Log("Finger " + finger.Index + " began touching the screen");
    }

    public void HandleFingerSet(LeanFinger finger)
    {
        Vector2 vet2DeltaMove = finger.ScreenPosition - finger.StartScreenPosition;
        //vet2DeltaMove.Log();

        Vector2 vec2ScenePosition = new Vector2(Screen.width, Screen.height);
        Vector2 vec2MovePercent = vet2DeltaMove / vec2ScenePosition;
        if (vec2MovePercent.x > vet2MoveThreshold.x || vec2MovePercent.y > vet2MoveThreshold.y)
        {
            onMove.Invoke();
        }
        //Debug.Log("Finger " + finger.Index + " is still touching the screen");
    }

    public void HandleFingerUp(LeanFinger finger)
    {
        onTouchDownUp.Invoke(false);
        //Debug.Log("Finger " + finger.Index + " finished touching the screen");
    }

    public void HandleFingerTap(LeanFinger finger)
    {
        //Debug.Log("Finger " + finger.Index + " tapped the screen");
    }

    public void HandleFingerSwipe(LeanFinger finger)
    {
        //Debug.Log("Finger " + finger.Index + " swiped the screen");
    }

    public void HandleGesture(List<LeanFinger> fingers)
    {
        if (!isNeedToRotate)
        {
            Vector2 vetDelta = LeanGesture.GetScreenDelta(fingers);
            tfCamRig.Rotate(new Vector3(0, vetDelta.x * SensitivityY, 0), Space.World);//左右


            tfCamera.Rotate(new Vector3(-vetDelta.y * SensitivityX, 0, 0), Space.Self);//上下
                                                                                       //float vetX = ClampAngle(tfCamera.localEulerAngles.x, minimumY, maximumY);
            float vetX = CheckAngle(tfCamera.localEulerAngles.x);
            vetX = Mathf.Clamp(vetX, minimumY, maximumY);
            var curlocalEulerAngles = tfCamera.localEulerAngles;
            curlocalEulerAngles.x = CheckAngleInverse(vetX);
            curlocalEulerAngles.y = 0;
            curlocalEulerAngles.z = 0;
            tfCamera.localEulerAngles = curlocalEulerAngles;
        }
    }
#endif

    public static float ClampAngle(float angle, float min, float max)
    {
        angle = angle % 360;
        if ((angle >= -360F) && (angle <= 360F))
        {
            if (angle < -360F)
            {
                angle += 360F;
            }
            if (angle > 360F)
            {
                angle -= 360F;
            }
        }
        return Mathf.Clamp(angle, min, max);
    }

    public float CheckAngleInverse(float value)
    {
        float angle = value;

        if (angle < 0)
        {
            return 360 + angle;
        }
        return angle;
    }
    public float CheckAngle(float value)  // 将大于180度角进行以负数形式输出
    {
        float angle = value - 180;

        if (angle > 0)
        {
            return angle - 180;
        }

        if (value == 0)
        {
            return 0;
        }

        return angle + 180;
    }

}

