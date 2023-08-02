using UnityEngine;
namespace Threeyes.Action
{
    /// <summary>
    /// 占位类，不做任何操作
    /// </summary>
    [CreateAssetMenu(menuName = EditorDefinition_Action.AssetMenuPrefix_Action_Common + "Empty", fileName = "Empty")]
#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
#endif
    public class SOAction_Empty : SOActionBase
    {
        public override bool IsComplete(GameObject target, ActionState actionState, string id = "")
        {
            return true;
        }
        protected override void EnterFunc(ActionRuntimeData runtimeData) { }

        protected override void ExitFunc(ActionRuntimeData runtimeData) { }
    }
}