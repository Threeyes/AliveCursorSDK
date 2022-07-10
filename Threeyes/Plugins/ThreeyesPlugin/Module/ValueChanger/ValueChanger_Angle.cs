using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Threeyes.ValueHolder;

/// <summary>
/// 修改角度
/// 适用：镜像物体
/// Todo：改名为针对角度更新相关的名称
/// 功能：
/// </summary>
public class ValueChanger_Angle : ValueHolder_Float
{
    /// <summary>
    /// 旋转轴向
    /// </summary>
    public enum Direction
    {
        x,
        y,
        z
    }

    public Direction direction = Direction.z;

    //缩放后的范围
    [Tooltip("The minimum value.")]
    public float min = 0f;
    [Tooltip("The maximum value.")]
    public float max = 100f;
    [Tooltip("The increments in which knob values can change.")]
    public float stepSize = 1f;

    protected Quaternion initialLocalRotation;
    protected Vector3 initialLocalEulerAngles;

    [Tooltip("以初始值为原点，对半分")]
    public bool isSplit = false;//是否以初始点为中点（即初始点为0，向左为负值，向右为正值）

    bool isInit = false;
    /// <summary>
    /// 保存初始角度
    /// </summary>
    public void Init()
    {
        initialLocalRotation = transform.localRotation;
        initialLocalEulerAngles = transform.localRotation.eulerAngles;
        isInit = true;
    }

    float outputValue;
    public void Update()
    {
        if (!isInit)
            return;

        float angle = 0;
        switch (direction)
        {
            case Direction.x:
                angle = transform.localRotation.eulerAngles.x - initialLocalEulerAngles.x;
                break;
            case Direction.y:
                angle = transform.localRotation.eulerAngles.y - initialLocalEulerAngles.y;
                break;
            case Direction.z:
                angle = transform.localRotation.eulerAngles.z - initialLocalEulerAngles.z;
                break;
        }

        angle = Mathf.Round(angle * 1000f) / 1000f; // not rounding will produce slight offsets in 4th digit that mess up initial value

        //修改部分（将角度改为0-180的范围）                                                
        // Quaternion.angle will calculate shortest route and only go to 180
        if (angle > 0 && angle <= 180)
        {
            outputValue = 360 - Quaternion.Angle(initialLocalRotation, transform.localRotation);
            //calculatedValue = 360 - Quaternion.Angle(initialRotation, transform.rotation);//世界坐标会导致报错
        }
        else
        {
            outputValue = Quaternion.Angle(initialLocalRotation, transform.localRotation);
            //calculatedValue = Quaternion.Angle(initialRotation, transform.rotation);
        }

        // adjust to value scale
        outputValue = Mathf.Round((min + Mathf.Clamp01(outputValue / 360f) * (max - min)) / stepSize) * stepSize;


        //修改：对半分（即以0为中点）
        if (isSplit)
        {
            if (outputValue > (max - min) / 2)
            {
                outputValue = -(max - outputValue);
            }
        }

        if (min > max && angle != 0)
        {
            outputValue = (max + min) - outputValue;
        }

        onValueChanged.Invoke(outputValue);
    }
}
