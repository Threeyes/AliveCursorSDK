using UnityEngine;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
using Threeyes.Editor;
#endif

namespace Threeyes.EventPlayer
{
    /// <summary>
    /// Manage List EP
    /// </summary>
    public class Sequence_EventPlayer : SequenceForCompBase<EventPlayer>
    {
        #region Inner Method

        protected override void SetDataValid(EventPlayer data)
        {
            data.IsActive = true;
        }
        protected override bool IsDataVaild(EventPlayer data)
        {
            return data.IsActive;
        }
        protected override void SetDataFunc(EventPlayer data, int index)
        {
            data.Play();
            base.SetDataFunc(data, index);
        }

        protected override void ResetDataFunc(EventPlayer data, int index)
        {
            data.Stop();
            base.ResetDataFunc(data, index);
        }

        #endregion

        #region Editor Method
#if UNITY_EDITOR

        public int FindIndexForDataEditor(EventPlayer data)
        {
            if (IsLoadChildOnAwake)
            {
                return GetComponentsFromChild().IndexOf(data);
            }
            return -1;
        }
        //——MenuItem——
        static string instGroupName = "EPS ";
        [MenuItem(EventPlayer.strMenuItem_Root_Collection + "EventPlayerSequence", false, EventPlayer.intCollectionMenuOrder + 2)]
        public static void CreateEventPlayerSequence()
        {
            EditorTool.CreateGameObject<Sequence_EventPlayer>(instGroupName);
        }
        [MenuItem(EventPlayer.strMenuItem_Root_Collection + "EventPlayerSequence Child", false, EventPlayer.intCollectionMenuOrder + 3)]
        public static void CreateEventPlayerSequenceChild()
        {
            EditorTool.CreateGameObjectAsChild<Sequence_EventPlayer>(instGroupName);
        }

        //——Hierarchy GUI——
        public override string ShortTypeName { get { return "EPS"; } }

        public override void SetHierarchyGUIProperty(StringBuilder sB)
        {
            base.SetHierarchyGUIProperty(sB);
        }

#endif
        #endregion
    }
}