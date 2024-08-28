using System;
using System.Collections;
using System.Collections.Generic;
using UMod.Shared.Linker;
using UnityEngine;
/// <summary>
/// 
/// Warning：
/// -仅能包含SDK可访问的UMod代码
/// </summary>
public static class UModTool
{
  public static bool  IsUModGameObject(Component component)
    {
        if (!component)
            return false;

        return component.GetComponent<LinkBehaviourV2>() != null;
    }
}
