#if USE_VIU


using System.Collections;using System.Collections.Generic;using UnityEngine;






/// <summary>/// 管理指定Controller，可挂在任意地方/// </summary>public class ControllerManagerSampleHelper : ComponentHelperBase<ControllerManagerSample>{    protected override ControllerManagerSample GetCompFunc()    {        var result = this.GetComponent<ControllerManagerSample>();        if (!result)        {            result = Component.FindObjectOfType<ControllerManagerSample>();        }        return result;    }    public ControllerCheckType allowedController = ControllerCheckType.Both;    public void ToggleLaserPointer(bool isShow)    {
        //Comp.SetLeftLaserPointerActive(isShow);
        //Comp.SetRightLaserPointerActive(isShow);
        //Comp.UpdateActivity();
        if (Comp.rightLaserPointer)            Comp.rightLaserPointer.SetActive(isShow);    }    public void ToggleCurvePointer(bool isShow)    {        Comp.SetLeftCurvePointerActive(isShow);        Comp.SetRightCurvePointerActive(isShow);        Comp.UpdateActivity();    }    public void ToggleCustomModel(bool isShow)    {        Comp.SetLeftCustomModelActive(isShow);        Comp.SetRightCustomModelActive(isShow);        Comp.UpdateActivity();    }}

#endif