﻿#if UNITY_EDITOR
using Threeyes.Core.Editor;
using UnityEditor;
using UnityEngine;

namespace Threeyes.EventPlayer.Editor
{
    /// <summary>
    /// Ref:
    /// http://ilkinulas.github.io/unity/2016/07/20/customize-unity-hierarchy-window.html
    /// https://forum.unity.com/threads/colors-colors-and-more-colors-please.499150/
    /// </summary>
    [InitializeOnLoad]
    public static class HierarchyView_EventPlayer
    {
        static HierarchyView_EventPlayer()
        {
            //Delegate for OnGUI events for every visible list item in the HierarchyWindow.
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
        }

        private static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

            if (!go)
                return;

            EventPlayer comp = go.GetComponent<EventPlayer>();
            if (comp)
            {
                EditorDrawerTool.RecordGUIColors();

                Rect remainRect = DrawButton(selectionRect, comp);
                EditorDrawerTool.DrawHierarchyViewInfo(remainRect, comp, true, SOEventPlayerSettingManager.ShowPropertyInHierarchy);

                EditorDrawerTool.RestoreGUIColors();
            }
        }

        #region 绘制通用界面

        static Color colorSelfActive = Color.green;
        static Color colorSelfDeActive = Color.red;
        //在Group中的状态
        static Color colorInGroupActive = colorSelfActive * 0.8f;
        static Color colorInGroupDeActive = colorSelfDeActive * 0.8f;

        static bool isMouseDown = false;

        private static Rect DrawButton(Rect selectionRect, EventPlayer comp)
        {
            #region ShortCuts 

            //#The whole Rect
            //Alt+Left Clitk: Switch IsActive Property
            if (EditorDrawerTool.CheckSelect(ref isMouseDown, selectionRect, 0, () => Event.current.alt))
            {
                //InvokeSelection(e => e.IsActive = !e.IsActive);
                Undo.RecordObject(comp, "EventPlayer Hierarchy Update");
                comp.IsActive = !comp.IsActive;
                EditorUtility.SetDirty(comp);
            }
            //Middle Click: Toggle Play
            if (EditorDrawerTool.CheckSelect(ref isMouseDown, selectionRect, 2, () => Event.current.alt))
            {
                //Undo.RecordObject(ep, "EventPlayer Hierarchy Update");
                comp.TogglePlay();
            }

            //#Display Active State
            bool isEPActive = comp.IsActive;
            //如果任一父物体管理该 EventPlayerBase，则检查EPG的IsActive
            bool isGroupActive = comp.IsGroupActive;
            //设置是否激活的状态
            Color colorBGActive = isGroupActive ? colorSelfActive : colorInGroupActive;
            Color colorBGDeActive = isGroupActive ? colorSelfDeActive : colorInGroupDeActive;
            GUI.backgroundColor = isEPActive ? colorBGActive : colorBGDeActive;//Toggle 背景颜色代表是否已激活

            //#Toggle
            //ShortCut：Press Left Mouse to Play, Right Mouse to Stop,Middle to Toggle
            Rect remainRect = selectionRect;
            Rect eleRectTog = remainRect.GetAvaliableRect(EditorDrawerTool.toggleSize);
            if (EditorDrawerTool.CheckSelect(ref isMouseDown, eleRectTog, 0))
            {
                comp.Play();
            }
            if (EditorDrawerTool.CheckSelect(ref isMouseDown, eleRectTog, 1))
            {
                comp.Stop();
            }
            if (EditorDrawerTool.CheckSelect(ref isMouseDown, eleRectTog, 2))
            {
                comp.TogglePlay();
            }
            GUI.Toggle(eleRectTog, comp.IsPlayed, new GUIContent(""), gUIStyleSwtichToggle);//Toggle只用于显示状态，其中的Tick 代表是否已经Play

            #endregion

            //减去已用宽度，计算剩余可用宽度
            remainRect = remainRect.GetRemainRect(eleRectTog.width);
            remainRect = remainRect.GetRemainRectWithoutNameLabel(comp);
            return remainRect;
        }




        #endregion

        #region GUI

        static GUIStyle gUIStyleSwtichToggle
        {
            get
            {
                if (_gUIStyleSwitchToggle == null)
                {
                    _gUIStyleSwitchToggle = EditorDrawerTool.CreateToggleGUIStyle(TexSwitchBGOn, TexSwitchBGOff);
                    _gUIStyleSwitchToggle.overflow = new RectOffset(-1, -1, -1, -1);
                }
                return _gUIStyleSwitchToggle;
            }
        }
        static GUIStyle _gUIStyleSwitchToggle;
        static Texture2D TexSwitchBGOn
        {
            get
            {
                if (!_texSwitchBGOn)
                {
                    _texSwitchBGOn = EditorDrawerTool.LoadResourcesSprite("EPToggleBG_On");
                }
                return _texSwitchBGOn;
            }
        }
        static Texture2D _texSwitchBGOn;
        static Texture2D TexSwitchBGOff
        {
            get
            {
                if (!_texSwitchBGOff)
                {
                    _texSwitchBGOff = EditorDrawerTool.LoadResourcesSprite("EPToggleBG_Off");
                }
                return _texSwitchBGOff;
            }
        }
        static Texture2D _texSwitchBGOff;
    }

    #endregion
}
#endif
