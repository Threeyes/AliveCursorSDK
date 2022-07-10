using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if USE_DOTween
using DG.Tweening;
#endif

public class LightManager : MonoBehaviour
{
    public Vector3 sunDeltaRot;
    //减弱灯光
    public float minValue = 0.1f;
    public Light sun;

    [ContextMenu("DimAllLight")]
    public void DimAllLight()
    {
        DimAllLight(2f);
    }
    public void DimAllLight(float tweenDuration)
    {
        StartDimAllLight(tweenDuration, false);
    }

    /// <summary>
    /// 完成后减弱灯光，优化性能
    /// </summary>
    /// <param name="tweenDuration"></param>
    public void DimAllLightThenDisable(float tweenDuration)
    {
        StartDimAllLight(tweenDuration, true);
    }

    void StartDimAllLight(float tweenDuartion, bool isDisableAfterFinish = false)
    {
#if USE_DOTween
        if (sun)
            sun.transform.DOLocalRotate(sun.transform.localEulerAngles + sunDeltaRot, tweenDuartion);

        transform.Recursive((tfLight) =>
        {
            Light light = tfLight.GetComponent<Light>();
            if (light)
            {
                Tweener tweener = light.DOIntensity(minValue, tweenDuartion);
                if (isDisableAfterFinish)
                {
                    tweener.onComplete += () => light.enabled = false;
                }
            }
        });
#endif
    }

}
