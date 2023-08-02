using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threeyes.ShowHide
{
    /// <summary>
    /// 逐渐显示 子物体
    /// </summary>
    public class Sequence_ShowHide : SequenceForCompBase<ShowAndHide>
    {
        //Todo:改为ResetAll
        public void ShowAll(bool isShow)
        {
            foreach (var child in ListData)
            {
                child.Show(isShow);
            }
        }

        protected override void SetDataFunc(ShowAndHide data, int index)
        {
            data.Show();
            base.SetDataFunc(data, index);
        }
        protected override void ResetDataFunc(ShowAndHide data, int index)
        {
            data.Hide();
            base.ResetDataFunc(data, index);
        }
    }
}