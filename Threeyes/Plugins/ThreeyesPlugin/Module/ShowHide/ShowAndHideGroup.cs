using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threeyes.ShowHide
{
    /// <summary>
    /// Todo:继承Seq
    /// </summary>
    public class ShowAndHideGroup : ComponentGroupBase<ShowAndHideBase>
    {
        public void Show()
        {
            Show(true);
        }
        public void Hide()
        {
            Show(false);
        }
        public virtual void Show(bool isShow)
        {
            ForEachChildComponent<ShowAndHideBase>((c) => c.Show(isShow));
        }

    }
}