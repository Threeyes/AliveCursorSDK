using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threeyes.ShowHide
{
    [DefaultExecutionOrder(-30000)]
    /// <summary>
    /// 在发布时隐藏
    /// 适用于测试物体
    /// </summary>
    public class HideAtEditor : ShowAndHide
    {
        protected override void Awake()
        {
            base.Awake();

            if (Application.isEditor)
                Hide();
        }
    }
}