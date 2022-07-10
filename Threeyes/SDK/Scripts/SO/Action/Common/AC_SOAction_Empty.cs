using UnityEngine;
using System.Collections;
using Threeyes.Action;
using Threeyes.Coroutine;
/// <summary>
/// 占位类，不做任何操作
/// </summary>
[CreateAssetMenu(menuName = AC_EditorDefinition.AssetMenuPrefix_SO_Action_Common + "Empty", fileName = "Empty")]
#if UNITY_EDITOR
[UnityEditor.CanEditMultipleObjects]
#endif
public class AC_SOAction_Empty : SOActionBase
{
    public override bool IsComplete(GameObject target, ActionState actionState, string id = "")
    {
        return true;
    }
    protected override void EnterFunc(ActionRuntimeData runtimeData) { }

    protected override void ExitFunc(ActionRuntimeData runtimeData) { }
}
