using System.Collections;
using System.Collections.Generic;
using Threeyes.GameFramework;
using UnityEngine;
/// <summary>
/// 管理与各ManagerHolder相关的初始化配置
/// </summary>
public class AC_ManagerHolderManager : ManagerHolderManager
{
    protected override void InitWorkshopItemInfoFactory()
    {
        GameFrameworkTool.RegisterManagerHolder(AC_WorkshopItemInfoFactory.Instance);//调用其构造函数进行初始化并注册到ManagerHolder
    }

    protected override List<IHubManagerModPreInitHandler> GetListManagerModPreInitOrder()
    {
        return new List<IHubManagerModPreInitHandler>();
    }

    protected override List<IHubManagerModInitHandler> GetListManagerModInitOrder()
    {
        return new List<IHubManagerModInitHandler>()
        {
            AC_ManagerHolder.CommonSettingManager,
            AC_ManagerHolder.EnvironmentManager,
            AC_ManagerHolder.PostProcessingManager,
            AC_ManagerHolder.TransformManager,
            AC_ManagerHolder.StateManager,
            AC_ManagerHolder.SystemCursorManager,
            AC_ManagerHolder.SystemAudioManager
        };
    }
}
