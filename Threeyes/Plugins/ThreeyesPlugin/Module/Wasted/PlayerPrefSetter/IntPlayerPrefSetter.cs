using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Obsolete("Use PersistentControllerManager_PlayerPref instead")]
public class IntPlayerPrefSetter : PlayerPrefSettterBase<int, IntEvent>
{
    protected override void SetValueFunc(int value)
    {
        PlayerPrefs.SetInt(Key, value);
    }
    protected override int GetValueFunc()
    {
        return PlayerPrefs.GetInt(Key, defaultValue);
    }

    [ContextMenu("DeleteKey")]
    public void DeleteKey()
    {
        if (PlayerPrefs.HasKey(key))
            PlayerPrefs.DeleteKey(key);
    }

}
