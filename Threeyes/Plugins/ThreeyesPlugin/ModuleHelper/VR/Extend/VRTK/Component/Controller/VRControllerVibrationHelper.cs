using System.Collections.Generic;
using UnityEngine;
#if USE_VRTK
using VRTK;
#endif
/// <summary>
/// 负责控制手柄的震动
/// </summary>
public class VRControllerVibrationHelper : VRControllerHelperBase
{
    public float vibValue = 0.1f;//震动的力度
    public float duration = -1;//震动的时长
    public float pulseInterval = 0.01f;//震动间隔


    public AudioClip AudioClip { get { return audioClip; } set { audioClip = value; } }
    public AudioClip audioClip;

    public void Viberation(bool isActive)
    {
        if (isActive)
            Viberation();
        else
            StopViberation();
    }

    [ContextMenu("Viberation")]
    public void Viberation()
    {
        Viberation(vibValue);
    }

    [ContextMenu("ViberationByAudio")]
    public void ViberationByAudio()
    {
        if (!AudioClip)
            return;

        foreach (Component controllerEvents in GetControllers(controllerCheckType))
        {
            VRInterface.Viberation(controllerEvents, AudioClip);
        }
    }

    public void Viberation(float tmpVibValue)
    {
        foreach (Component controllerEvents in GetControllers())
        {
            VRInterface.Viberation(controllerEvents, tmpVibValue, duration, pulseInterval);
        }
    }

    [ContextMenu("StopViberation")]
    public void StopViberation()
    {
        foreach (Component controllerEvents in GetControllers())
        {
            VRInterface.StopViberation(controllerEvents);
        }
    }
}