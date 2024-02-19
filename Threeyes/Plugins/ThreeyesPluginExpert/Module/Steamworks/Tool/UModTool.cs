using System;
using System.Collections;
using System.Collections.Generic;
using UMod.Shared.Linker;
using UnityEngine;
/// <summary>
/// 因为忘了提交Expert，只能暂时使用该脚本
/// </summary>
public static class UModTool
{
  public static bool  IsUModGameObject(Component component)
    {
        if (!component)
            return false;

        return component.GetComponent<LinkBehaviourV2>() != null;
    }

    public static T FixSerializationCallbackReceiverData<T>(T data)
        where T : ISerializationCallbackReceiver
    {
        data.OnAfterDeserialize();
        return data;
    }
}
