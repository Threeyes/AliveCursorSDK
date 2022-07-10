using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Obsolete("Use PersistentControllerManager_PlayerPref instead")]
public class BoolPlayerPrefSetter : PlayerPrefSettterBase<bool, BoolEvent>
{
    public Toggle toggle;//对应的Toggle（可空）

    protected override void Awake()
    {
        //自动监听对应的UI事件
        if (toggle)
            toggle.onValueChanged.AddListener(SetValue);

        base.Awake();
    }

    public void Test1(bool fuck = true) { }
    protected override void SetUIFunc(bool value)
    {
        if (toggle)
            toggle.SetValueWithoutNotify(value);
    }

    protected override void SetValueFunc(bool value)
    {
        PlayerPrefs.SetInt(Key, value == false ? 0 : 1);
    }

    protected override bool GetValueFunc()
    {
        return PlayerPrefs.GetInt(Key, defaultValue == false ? 0 : 1) == 0 ? false : true;
    }

    [ContextMenu("DeleteKey")]
    public void DeleteKey()
    {
        if (PlayerPrefs.HasKey(key))
            PlayerPrefs.DeleteKey(key);
    }

}
