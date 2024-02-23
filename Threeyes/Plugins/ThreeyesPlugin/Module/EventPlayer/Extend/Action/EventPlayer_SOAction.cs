using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Text;
using System.Linq;
using Threeyes.Action;
using Threeyes.Core;
#if USE_NaughtyAttributes
using NaughtyAttributes;
using Threeyes.Core.Editor;
#endif
#if UNITY_EDITOR
#endif

namespace Threeyes.EventPlayer
{
    /// <summary>
    /// Common Action EP
    /// 
    /// PS:The param here will be SoAction's param
    /// Warning: Don't call Play(TParam value) because it will conflict with Play(bool isPlay), use PlayWithParam(TParam value) instead.
    /// </summary>
    public class EventPlayer_SOAction : EventPlayerWithParamBase<EventPlayer_SOAction, ObjectEvent, object>
    {
        #region Property & Field

        public override string ValueToString { get { return/* Value != null ? SOAction.name :*/ ""; } }//ToAdd:显示Action的简称
        public GameObject GOTarget
        {
            get
            {
                if (!goTarget)
                    goTarget = gameObject;
                return goTarget;
            }
            set { goTarget = value; }
        }
        public SOActionBase SOAction { get { return soAction; } set { soAction = value; } }

        [SerializeField] protected GameObject goTarget;
#if USE_NaughtyAttributes
        [Expandable]
#endif
        [SerializeField]
        [Tooltip("Event will work without soAction.")]
        protected SOActionBase soAction;


        #endregion

        #region Inner Method

        protected override void PlayFunc()
        {
            SOAction?.Enter(true, GOTarget, listModifier: ListActionModifier);//PS：Get all IActionModifier from this gameobject
            base.PlayFunc();
        }
        protected override void StopFunc()
        {
            SOAction?.Enter(false, GOTarget, listModifier: ListActionModifier);
            base.StopFunc();
        }

        protected override void PlayWithParamFunc(object value)
        {
            SOAction?.Enter(true, GOTarget, value, listModifier: ListActionModifier);//Set SOAction's value
            base.PlayWithParamFunc(value);
        }

        protected override void StopWithParamFunc(object value)
        {
            SOAction?.Enter(false, GOTarget, value, listModifier: ListActionModifier);
            base.StopWithParamFunc(value);
        }

        /// <summary>
        /// Get all modifiers from this gameobject
        /// </summary>
        List<IActionModifier> ListActionModifier { get { return GetComponents<IActionModifier>().ToList(); } }
        #endregion

        #region Editor Method
#if UNITY_EDITOR
        //public override bool IsCustomInspector => false;
        //——MenuItem——
        protected const int intActionMenuOrder = 500;
        protected const string strActionMenuGroup = strMenuItem_Root + "Action/";
        static string instName = "ActionEP ";
        [UnityEditor.MenuItem(strActionMenuGroup + "ActionEventPlayer", false, intActionMenuOrder + 0)]
        public static void CreateActionEventPlayer()
        {
            EditorTool.CreateGameObjectAsChild<EventPlayer_SOAction>(instName);
        }

        public override string ShortTypeName { get { return "Act"; } }

        //——Inspector GUI——

        public override void SetInspectorGUISubProperty(GUIPropertyGroup group)
        {
            base.SetInspectorGUISubProperty(group);
            group.listProperty.Add(new GUIProperty(nameof(goTarget), "Target"));
            group.listProperty.Add(new GUIProperty(nameof(soAction), "Action"));
        }

        public override void SetInspectorGUICommonTextArea(StringBuilder sB)
        {
            base.SetInspectorGUICommonTextArea(sB);
            if (SOAction)
            {
                SOAction.SetInspectorGUICommonTextArea(sB, GOTarget);
            }
        }
#endif
        #endregion
    }
}
