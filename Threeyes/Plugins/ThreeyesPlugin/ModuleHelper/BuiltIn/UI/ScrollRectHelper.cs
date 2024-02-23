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

    }
}