using System;
using System.Collections;
using System.Collections.Generic;
using Threeyes.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Threeyes.ModuleHelper
{
    /// <summary>
    /// 更改Comp的百分值
    /// </summary>
    public class ScrollRectHelper : ComponentHelperBase<ScrollRect>
    {
        public BoolEvent onReachTop;
        public BoolEvent onReachBottom;

        public Direction direction = Direction.TopToBottom;
        //private void Update()
        //{
        //    if (Input.GetKeyDown(KeyCode.S))
        //        NextPage();
        //    if (Input.GetKeyDown(KeyCode.W))
        //        LastPage();
        //}

        float NormalizedPosition
        {
            get { return Comp.horizontal ? Comp.horizontalNormalizedPosition : Comp.verticalNormalizedPosition; }
            set
            {
                value = Mathf.Clamp01(value);
                if (Comp.horizontal)
                    Comp.horizontalNormalizedPosition = value;
                if (Comp.vertical)
                    Comp.verticalNormalizedPosition = value;

                normalizedPosition = Comp.normalizedPosition;
                onReachTop.Invoke(value == direction.GetAttributeOfType<DirectionInfo>().top);
                onReachBottom.Invoke(value == direction.GetAttributeOfType<DirectionInfo>().bottom);
            }
        }

        [ContextMenu("LastPage")]
        public void LastPage()
        {
            MoveToPage(-1);
        }
        [ContextMenu("NextPage")]
        public void NextPage()
        {
            MoveToPage(+1);
        }

        [ContextMenu("TopPage")]
        public void TopPage()
        {
            NormalizedPosition = direction.GetAttributeOfType<DirectionInfo>().top;
        }

        [ContextMenu("BottomPage")]
        public void BottomPage()
        {
            NormalizedPosition = direction.GetAttributeOfType<DirectionInfo>().bottom;
        }

        public Vector2 vt2Step;//每次翻页的数量
        public Vector2 normalizedPosition;//归一化的位置
        void MoveToPage(int deltaIndex)
        {
            Vector2 viewportSize = Comp.viewport.rect.size;
            Vector2 contentSize = Comp.content.rect.size;
            vt2Step = new Vector2(viewportSize.x / contentSize.x, viewportSize.y / contentSize.y) * deltaIndex;

            float symbol = direction.GetAttributeOfType<DirectionInfo>().symbol;
            if (Comp.horizontal)
                NormalizedPosition = NormalizedPosition + vt2Step.x * symbol;
            if (Comp.vertical)
                NormalizedPosition = NormalizedPosition + vt2Step.y * symbol;

            Canvas.ForceUpdateCanvases();//强制Canvas更新
        }


        #region Center on item (Ref: https://forum.unity.com/threads/scrollrect-scroll-to-a-gameobject-position.473214/#post-3088504)
        [Header("Center on item")]
        public RectTransform rectTfScroll;
        public RectTransform rectTfMask;
        public RectTransform rectTfContent;
        [ContextMenu("SetupCenter")]
        private void SetupCenter()
        {
            if (!rectTfScroll)
                rectTfScroll = Comp.GetComponent<RectTransform>();
            if (!rectTfContent)
                rectTfContent = Comp.content;
            if (!rectTfMask)
            {
                var mask = rectTfScroll.GetComponentInChildren<Mask>(true);
                if (mask)
                {
                    rectTfMask = mask.rectTransform;
                }
                else
                {
                    var mask2D = rectTfScroll.GetComponentInChildren<RectMask2D>(true);
                    if (mask2D)
                    {
                        rectTfMask = mask2D.rectTransform;
                    }
                }
            }
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
        /// <summary>
        /// 居中到指定子元素
        /// </summary>
        /// <param name="target"></param>
        public void CenterOnItem(RectTransform target)
        {
            Canvas.ForceUpdateCanvases();//强制Canvas更新，确保即使该帧添加了新元素也能定位到正确位置

            // Item is here
            var itemCenterPositionInScroll = GetWorldPointInParent(rectTfScroll, target);
            // But must be here(目标位置为Mask的锚点位置，后续可自定义)
            var targetPositionInScroll = GetWorldPointInParent(rectTfScroll, rectTfMask);
            // So it has to move this distance
            var difference = targetPositionInScroll - itemCenterPositionInScroll;
            difference.z = 0f;

            //clear axis data that is not enabled in the scrollrect
            if (!Comp.horizontal)
            {
                difference.x = 0f;
            }
            if (!Comp.vertical)
            {
                difference.y = 0f;
            }

            var normalizedDifference = new Vector2(
                difference.x / (rectTfContent.rect.size.x - rectTfScroll.rect.size.x),
                difference.y / (rectTfContent.rect.size.y - rectTfScroll.rect.size.y));

            var newNormalizedPosition = Comp.normalizedPosition - normalizedDifference;
            if (Comp.movementType != ScrollRect.MovementType.Unrestricted)
            {
                newNormalizedPosition.x = Mathf.Clamp01(newNormalizedPosition.x);
                newNormalizedPosition.y = Mathf.Clamp01(newNormalizedPosition.y);
            }

            Comp.normalizedPosition = newNormalizedPosition;
        }

        static Vector3 GetWidgetWorldPoint(RectTransform target)
        {
            //pivot position + item size has to be included
            var pivotOffset = new Vector3(
                (0.5f - target.pivot.x) * target.rect.size.x,
                (0.5f - target.pivot.y) * target.rect.size.y,
                0f);
            var localPosition = target.localPosition + pivotOffset;
            return target.parent.TransformPoint(localPosition);
        }
        static Vector3 GetWorldPointInParent(RectTransform target, RectTransform child)
        {
            Vector3 worldPoint = GetWidgetWorldPoint(child);
            return target.InverseTransformPoint(worldPoint);
        }
        #endregion 

        #region Define
        [AttributeUsage(AttributeTargets.Field)]
        public class DirectionInfo : Attribute
        {
            public float top;
            public float bottom;
            public float symbol;//+-1

            public DirectionInfo(float top, float bottom, float symbol)
            {
                this.top = top;
                this.bottom = bottom;
                this.symbol = symbol;
            }
        }

        // 摘要:
        //     Setting that indicates one of four directions.
        public enum Direction
        {
            //
            // 摘要:
            //     From left to right.
            [DirectionInfo(0, 1, +1)]
            LeftToRight = 0,
            //
            // 摘要:
            //     From right to left.
            [DirectionInfo(1, 0, -1)]
            RightToLeft = 1,
            //
            // 摘要:
            //     From bottom to top.
            [DirectionInfo(0, 1, +1)]
            BottomToTop = 2,
            //
            // 摘要:
            //     From top to bottom.
            [DirectionInfo(1, 0, -1)]
            TopToBottom = 3
        }
        #endregion
    }
}