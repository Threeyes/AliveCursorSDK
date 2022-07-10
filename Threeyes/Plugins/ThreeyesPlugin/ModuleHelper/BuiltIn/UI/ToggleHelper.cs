using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if USE_NaughtyAttributes
using NaughtyAttributes;
#endif
/// <summary>
/// Toggle的主要用法：
/// 1.主动调用（isOn+UI状态更新+事件调用）——ToggleOn
/// 2.状态更新（isOn+UI状态更新）[初始化、回调]——ToggleOnWithoutNotify
/// </summary>
public class ToggleHelper : ComponentHelperBase<Toggle>
{
    public ToggleGroup toggleGroup;

    public bool isSwitchSprite = false;
    public Sprite spriteOn;
    public Sprite spriteOff;
    /// <summary>
    /// PS:Toggle的isOn默认值需要设置为false，否则调用该函数无效
    /// </summary>
#if USE_NaughtyAttributes
    [Button]
#endif

    private void Awake()
    {
        if (Comp)
            Comp.onValueChanged.AddListener(SwitchSprite);//主动调用
    }


    #region 主动调用

    [ContextMenu("ToggleOn")]
    public void ToggleOn()
    {
        ToggleOn(true);
    }

#if USE_NaughtyAttributes
    [Button]
#endif
    [ContextMenu("ToggleOff")]
    public void ToggleOff()
    {
        ToggleOn(false);
    }

    public void ToggleOn(bool isOn)
    {
        Comp.isOn = isOn;
    }

    /// <summary>
    /// 只更新状态，不调用事件，适用于更新UI。【调用时机：初始化，回调】
    /// </summary>
    /// <param name="isOn"></param>
    public void ToggleOnWithoutNotify(bool isOn)
    {
        Comp.SetValueWithoutNotify(isOn);
        SwitchSprite(isOn);
    }


    [ContextMenu("GroupToggleOn")]
    public void GroupToggleOn()
    {
        GroupToggleOn(true);
    }
    public void GroupToggleOn(bool isOn)
    {
        if (!toggleGroup)
            toggleGroup = GetComponentInParent<ToggleGroup>();
        if (!toggleGroup)
            return;

        Comp.isOn = isOn;
    }

    #endregion

    void SwitchSprite(bool isOn)
    {
        if (isSwitchSprite)
        {
            Image imageBG = Comp.image;
            if (imageBG)
                imageBG.sprite = isOn ? spriteOn : spriteOff;
        }
    }
}
