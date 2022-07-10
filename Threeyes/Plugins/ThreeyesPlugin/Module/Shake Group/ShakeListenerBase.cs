using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ShakeListenerBase : MonoBehaviour
{
    protected virtual void OnEnable()
    {
        AddListeners();
    }
    protected virtual void OnDisable()
    {
        RemoveListeners();
    }
    protected virtual void AddListeners()
    {
        ShakeManager.Instance.onShaking.AddListener(OnShaking);
        ShakeManager.Instance.onShakeStop.AddListener(OnShakeStop);
    }

    protected virtual void RemoveListeners()
    {
        //也可以直接删掉该脚本，但是为了方便Debug保留
        ShakeManager.Instance.onShaking.RemoveListener(OnShaking);
        ShakeManager.Instance.onShakeStop.RemoveListener(OnShakeStop);
    }
    protected virtual void OnShaking(float percent)
    {

    }
    protected virtual void OnShakeStop()
    {

    }


}
