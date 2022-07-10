using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Obsolete("Use PersistentControllerManager_PlayerPref instead")]
public class StringPlayerPrefSetter : PlayerPrefSettterBase<string, StringEvent>
{
    protected override void SetValueFunc(string value)
    {
        PlayerPrefs.SetString(Key, value);
    }
    protected override string GetValueFunc()
    {
        return PlayerPrefs.GetString(Key, defaultValue);
    }

    [ContextMenu("DeleteKey")]
    public void DeleteKey()
    {
        if (PlayerPrefs.HasKey(key))
            PlayerPrefs.DeleteKey(key);
    }
}
