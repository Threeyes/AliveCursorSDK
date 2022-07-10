using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 随机设置新浮点值
/// </summary>
public class RandomSetValue : RandomValueComponentBase<Random_Float, float>
{
    public FloatEvent onSetValue;//需要调用的设置参数的方法

    public override void OnSet()
    {
        onSetValue.Invoke(ResultValue);//设置随机值
        base.OnSet();
    }
}
