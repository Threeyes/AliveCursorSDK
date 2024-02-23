using System.Collections;
using System.Collections.Generic;
using Threeyes.Core;
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

        /// <summary>
        /// 只显示特定物体
        /// ToUpdate：增加传入GameObject的针对普通子物体的方法
        /// </summary>
        /// <param name="showAndHideBase"></param>
        public virtual void ShowSolo(ShowAndHideBase showAndHideBase)
        {
            ForEachChildComponent<ShowAndHideBase>((c) => c.Show(c == showAndHideBase));

        }

    }
}