using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ToggleGroup_Ex : ToggleGroup
{
    //PS:避免ToggleGroup在开始时强制选中Toggle
    protected override void Start()
    {
        //base.Start();
    }
    protected override void OnEnable()
    {
        //base.OnEnable();
    }
}
