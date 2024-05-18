using System;
using Threeyes.Core;
using UnityEngine;
namespace Threeyes.Action
{
    /// <summary>
    /// Placeholder class, no action taken
    /// 占位类，不做任何操作
    /// 
    /// ToUpdate：
    /// -Receiver可以为Transform
    /// -需要更新stateProgress，否则永远无法完成
    /// </summary>
    [CreateAssetMenu(menuName = EditorDefinition_Action.AssetMenuPrefix_Action_Common + "Empty", fileName = "Empty")]
#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
#endif
    public class SOAction_Empty : SOActionBase
    {
        public override Type ReceiverType { get { return typeof(Transform); } }//暂无实际用途

        public override bool IsComplete(ObjectID objectID, ActionState actionState)
        {
            return true;
        }
        protected override void EnterFunc(ActionRuntimeData runtimeData) { }

        protected override void ExitFunc(ActionRuntimeData runtimeData) { }
    }
}