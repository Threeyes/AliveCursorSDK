using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Obsolete("Use PersistentControllerManager_PlayerPref instead")]
public class FloatPlayerPrefSetter : PlayerPrefSettterBase<float, FloatEvent>
{
    public Slider slider;//对应的Slider（可空）

    protected override void Awake()
    {
        //监听对应的UI事件
        if (slider)
            slider.onValueChanged.AddListener(SetValue);

        base.Awake();
    }

    protected override void SetUIFunc(float value)
    {
        if (slider)
            slider.SetValueWithoutNotify(value);
    }

    protected override void SetValueFunc(float value)
    {
        PlayerPrefs.SetFloat(Key, value);
    }
    protected override float GetValueFunc()
    {
        return PlayerPrefs.GetFloat(Key, defaultValue);
    }

    [ContextMenu("DeleteKey")]
    public void DeleteKey()
    {
        if (PlayerPrefs.HasKey(key))
            PlayerPrefs.DeleteKey(key);
    }

}
