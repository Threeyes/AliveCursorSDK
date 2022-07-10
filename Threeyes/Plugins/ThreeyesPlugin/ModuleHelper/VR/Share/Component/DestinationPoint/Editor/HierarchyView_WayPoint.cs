#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
namespace Threeyes.Editor
{
    [InitializeOnLoad]
    public static class HierarchyView_WayPoint
    {
        #region WayPoint

        static Color colorSelfActive = Color.white;
        static Color colorSelfDeActive = Color.gray;
        static Color colorEnter = Color.green;

        static Texture TexIcon { get { return _texIcon ? _texIcon : EditorDrawerTool.LoadResourcesSprite("LocationMark"); } }
        static Texture _texIcon;

        static bool isMouseDown = false;


        static HierarchyView_WayPoint()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
        }

        static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

            if (!go)
                return;

            EditorDrawerTool.RecordGUIColors();

            DrawComponent(selectionRect, go);

            EditorDrawerTool.RestoreGUIColors();
        }


        static void DrawComponent(Rect selectionRect, GameObject go)
        {
            WayPoint comp = go.GetComponent<WayPoint>();
            if (!comp)
                return;
            if (!comp.Comp)
                return;

            //中键调用TogglePlay函数
            if (EditorDrawerTool.CheckSelect(ref isMouseDown, selectionRect, 2))
            {
                comp.MovetoThis();
            }


            Rect rectEle = selectionRect.GetAvaliableRect(EditorDrawerTool.buttonSize);
            Color colorResult = comp.IsActive ? colorSelfActive : colorSelfDeActive;
            if (comp.IsEntered)
                colorResult *= colorEnter;//颜色叠加

            if (EditorDrawerTool.DrawButton(rectEle, TexIcon, colorResult))//点击
                comp.MovetoThis();

            RemoteDestinationPoint remoteDestinationPoint = comp.GetComponent<RemoteDestinationPoint>();
            if (remoteDestinationPoint)
            {
                if (remoteDestinationPoint.isTeleportOnGameStart)
                {
                    EditorDrawerTool.DrawLabel("* ", rectEle);
                }
            }
        }

        #endregion
    }
}
#endif