#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Threeyes.Editor
{
    [InitializeOnLoad]
    public static class HierarchyViewManager_ValueChanger_Float
    {
        static HierarchyViewManager_ValueChanger_Float()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
        }

        static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

            if (!go)
                return;

            EditorDrawerTool.RecordGUIColors();

            DrawValueChanger(selectionRect, go);

            EditorDrawerTool.RestoreGUIColors();
        }

        private static StringBuilder sbSummary = new StringBuilder();
        private static StringBuilder sbProperty = new StringBuilder();

        static Color colReachMax = Color.blue;// new Color(0, 255, 255);
        static void DrawValueChanger(Rect selectionRect, GameObject go)
        {
            ValueChanger_Float comp = go.GetComponent<ValueChanger_Float>();
            if (!comp)
                return;

            sbSummary.Length = 0;
            sbProperty.Length = 0;
            sbSummary.Append(comp.CurValue.Format());//当前值
            sbProperty.Append(comp.Step);
            Rect remainRect = selectionRect.GetRemainRectWithoutNameLabel(comp);

            #region Slider

            //Range
            if (comp.isUseRange)
            {
                AddSplit(ref sbProperty);
                sbProperty.Append(comp.range.MinValue).Append("→").Append(comp.range.MaxValue);//[0.1|0→1]

                Rect eleRect = remainRect.GetAvaliableRect(EditorDrawerTool.sliderSize);

                float curPercent = ((comp.CurValue - comp.range.MinValue) / comp.range.Range);
                Color colSlider = Color.white;
                //if (curPercent == 0)
                //    ;
                if (curPercent < 1)
                {
                    colSlider = Color.Lerp(Color.white, Color.green, curPercent);
                }
                else
                    colSlider = colReachMax;
                GUI.color = colSlider;

                float cacheValue = comp.CurValue;
                float resultValue = GUI.HorizontalSlider(eleRect, comp.CurValue, comp.range.MinValue, comp.range.MaxValue);
                if (resultValue != cacheValue)
                {
                    Undo.RecordObject(comp, "Change CurValue");
                    comp.CurValue = resultValue;
                }

                remainRect = remainRect.GetRemainRect(eleRect);

                GUI.color = Color.white;
            }

            #endregion


            #region Button & Label

            Rect rectEle = remainRect.GetAvaliableRect(EditorDrawerTool.buttonSize);
            if (EditorDrawerTool.DrawButton(rectEle, EditorDrawerTool.TexArrRightIcon))
                comp.Add();
            rectEle = remainRect.GetRemainRect(rectEle).GetAvaliableRect(EditorDrawerTool.buttonSize);
            if (EditorDrawerTool.DrawButton(rectEle, EditorDrawerTool.TexArrLeftIcon))
                comp.Subtract();

            remainRect = remainRect.GetRemainRect(rectEle).GetRemainRect(rectEle);

            if (sbProperty.Length > 0)
            {
                sbSummary.Append("[").Append(sbProperty).Append("]");// CurValue[Step|MinValue→MaxValue]   0.3[0.1|0→1]
            }

            remainRect = EditorDrawerTool.DrawLabel(sbSummary.ToString(), remainRect);

            #endregion
        }

        /// <summary>
        /// 增加指定分隔符
        /// </summary>
        /// <param name="souSB"></param>
        public static void AddSplit(ref StringBuilder souSB)
        {
            if (souSB.NotNull())
                souSB.Append("|");
        }
    }
}
#endif
