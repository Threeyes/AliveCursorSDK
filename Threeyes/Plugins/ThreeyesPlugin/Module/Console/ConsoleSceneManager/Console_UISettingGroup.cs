using System.Collections;
using System.Collections.Generic;
using Threeyes.Persistent;
using UnityEngine;
using UnityEngine.UI;

public class Console_UISettingGroup : InstanceBase<Console_UISettingGroup>
{
    private void Start()
    {
        //设置默认的PlayerPref
        SOConsoleSceneConfig sOConsoleSceneCur = SOConsoleSceneManager.Instance.CurConsoleSceneConfig;
        if (sOConsoleSceneCur)
        {
            //真机首次运行时初始化并保存参数
            boolPlayerPrefSetterAutoLoad.SetDefaultValue(sOConsoleSceneCur.isAutoLoad);
            boolPlayerPrefSetterReturnToConsoleOnFinish.SetDefaultValue(sOConsoleSceneCur.isReturnToConsoleOnFinish);
            intPlayerPrefSetterPlaybackOrder.SetDefaultValue((int)sOConsoleSceneCur.playbackOrderType);
        }
    }

    #region Playback

    public Button butPlaybackOrder;
    public Image imagePlaybackOrder;
    public IntPlayerPrefSetter intPlayerPrefSetterPlaybackOrder;
    public List<Sprite> listSpritePlayback = new List<Sprite>();//回放类型图标

    public void OnPlaybackButtonClick()
    {
        int curIndex = intPlayerPrefSetterPlaybackOrder.GetValue();
        int newIndex = curIndex.GetNextIndex(listSpritePlayback.Count);
        intPlayerPrefSetterPlaybackOrder.SetValue(newIndex);
    }
    public void OnPlaybackOrderPrefSetValue(int index)
    {
        if (index < listSpritePlayback.Count)
        {
            imagePlaybackOrder.sprite = listSpritePlayback[index];
#if UNITY_EDITOR
            UnityEditor.SceneView.RepaintAll();
#endif

            SOConsoleSceneConfig sOConsoleSceneCur = SOConsoleSceneManager.Instance.CurConsoleSceneConfig;
            if (sOConsoleSceneCur)
            {
                sOConsoleSceneCur.playbackOrderType = (SOConsoleSceneConfig.PlaybackOrderType)index;
            }
        }
        else
            Debug.LogError("PlaybackOrder 越界！");
    }

    #endregion

    #region  ReturnToConsole

    public Toggle togReturnToConsoleOnFinish;
    public BoolPlayerPrefSetter boolPlayerPrefSetterReturnToConsoleOnFinish;

    public void OnReturnToConsoleOnFinishToggle(bool isOn)
    {
        boolPlayerPrefSetterReturnToConsoleOnFinish.SetValue(isOn);
    }
    public void OnReturnToConsoleOnFinishPrefSetValue(bool isOn)
    {
        togReturnToConsoleOnFinish.SetValueWithoutNotify(isOn);

        SOConsoleSceneConfig sOConsoleSceneCur = SOConsoleSceneManager.Instance.CurConsoleSceneConfig;
        if (sOConsoleSceneCur)
        {
            sOConsoleSceneCur.isReturnToConsoleOnFinish = isOn;
        }
    }
    #endregion

    #region AutoLoad

    public Toggle togAutoLoad;
    public BoolPlayerPrefSetter boolPlayerPrefSetterAutoLoad;

    public void OnAutoLoadToggle(bool isOn)
    {
        boolPlayerPrefSetterAutoLoad.SetValue(isOn);
    }
    public void OnAutoLoadPrefSetValue(bool isOn)
    {
        togAutoLoad.SetValueWithoutNotify(isOn);

        SOConsoleSceneConfig sOConsoleSceneCur = SOConsoleSceneManager.Instance.CurConsoleSceneConfig;
        if (sOConsoleSceneCur)
        {
            sOConsoleSceneCur.isAutoLoad = isOn;
        }
    }

    #endregion
}
