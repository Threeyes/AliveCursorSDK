using System.Collections;
using System.Collections.Generic;
using Threeyes.Core;
using UnityEngine;
/// <summary>
/// 根据模型的缩放， 同步Rigidbody的CenterOfMass
/// 
/// PS:
/// -仅适用于自定义CenterOfMass
/// -因为Rigidbody的CenterOfMass不受缩放影响， 所以如果是自定义CenterOfMass的刚体就需要挂载该组件，用于确保物体缩放后的重心一致（https://docs.unity3d.com/ScriptReference/Rigidbody-centerOfMass.html）
/// </summary>
public class RigidbodySyncCenterOfMassBySize : ComponentHelperBase<Rigidbody>
{
    Vector3 curSize;
    Transform cacheTf;//缓存Rigidbody的Transform
    public Vector3 centerOfMassPerUnit = new Vector3(0, 0, 0);//每单元对应的UV Scale值
    private void Awake()
    {
        if (!Comp)
        {
            Debug.LogError($"{nameof(Comp)} not set!");
            return;
        }
        cacheTf = Comp.transform;
        curSize = cacheTf.lossyScale;
    }
    private void LateUpdate()
    {
        if (!(Comp && cacheTf))
            return;

        if (cacheTf.lossyScale != curSize)
        {
            curSize = transform.lossyScale;
            Vector3 value = centerOfMassPerUnit;
            value.Scale(curSize);
            Comp.centerOfMass = value;
        }

    }
}
