using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITipsGroup : ComponentGroupBase<UITips>
{
    public void Show(bool isShow)
    {
        ForEachChildComponent<UITips>((c) => c.Show(isShow));
    }
    public void Hide()
    {
        ForEachChildComponent<UITips>((c) => c.Hide());
    }
}
