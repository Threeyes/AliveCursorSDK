using UnityEngine.Events;
using UnityEngine;
#if USE_VRTK
using VRTK;
using VRTK.UnityEventHelper;
#endif

/// <summary>
/// Knob或Slider等继承VRTK_Control的组件移动到特定值后，执行方法
/// </summary>
public class VRControlHelper :
#if USE_VRTK
    VRTK_UnityEvents<VRTK_Control>
#else
 MonoBehaviour
#endif
{
    public CheckValueType checkValueType = CheckValueType.Equal;//值的检测类型
    public int triggerValue;//触发的值
    public FloatEvent onValueChanged;
    public FloatEvent onNormalizedChanged;//0~100
    public UnityEvent onReach;

    [Header("ReadOnly")]
    public float curValue;
    public enum CheckValueType
    {
        Equal,
        Greater,
        Less
    }

#if USE_VRTK
    protected override void AddListeners(VRTK_Control component)
    {
        component.ValueChanged += ValueChanged;
    }

    protected override void RemoveListeners(VRTK_Control component)
    {
        component.ValueChanged -= ValueChanged;
    }

    private void ValueChanged(object o, Control3DEventArgs e)
    {
        onValueChanged.Invoke(e.value);
        onNormalizedChanged.Invoke(e.normalizedValue);
        bool isReach = false;
        switch (checkValueType)
        {
            case CheckValueType.Equal:
                isReach = e.value == triggerValue; break;
            case CheckValueType.Greater:
                isReach = e.value > triggerValue; break;
            case CheckValueType.Less:
                isReach = e.value < triggerValue; break;
        }
        if (isReach)
            onReach.Invoke();

        curValue = e.value;
    }
#endif
}