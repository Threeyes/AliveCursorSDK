using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using static Threeyes.Steamworks.Whiteboard;

namespace Threeyes.Steamworks
{
    /// <summary>
    /// Ref: https://git.fh-aachen.de/MakeItTrue2/VR/-/blob/ea8b3e6728db29fd229603b12b3a5f88c315fa54/unity-game/Assets/Scripts/WhiteBoard/WhiteboardPen.cs
    /// 
    /// Todo:
    /// -增加笔刷样式（实现方式：调用whiteboard.SetPenColor设置笔刷块颜色时，要传入对应样式的矢量图或者矢量图计算公式（通常为Alpha通道图），将样式的每个像素点与笔刷的颜色相乘，得到的就是笔刷形状。绘制时要和原像素混合。提供保存图像的功能）
    /// </summary>
    public class WhiteboardPen : InstanceBase<WhiteboardPen>
    {
        public Transform tfTip;//The end point of pen, the y axis must face the Whiteboard

        [Header("Runtime")]
        public Color penColor = Color.yellow;
        public int penSize = 2;
        public PenType penType = PenType.Pen;
        public EffectType effectType = EffectType.None;
        private Whiteboard whiteboard;
        private RaycastHit touch;
        ConfigInfo Config { get { return Whiteboard.Instance.Config; } }

        #region Public
        public void SetPenType(PenType penType)
        {
            this.penType = penType;
        }
        public void SetEffect(EffectType effectType)
        {
            this.effectType = effectType;
        }

        public void SetPenColor(Color color)
        {
            penColor = color;
            //ToAdd:如果Tip有对应的Renderer，那就同步设置颜色
        }
        public void SetPenSize(int size)
        {
            penSize = (int)Mathf.Clamp(size, Config.penSizeRange.x, Config.penSizeRange.y);
        }
        #endregion

        #region Callback
        bool isDrawMouseButtonDown = false;
        public void OnMouseButtonDownUp(bool isDown)//Invoked by CursorInputBehaviour.onButtonDownUp
        {
            isDrawMouseButtonDown = isDown;
        }
        #endregion

        protected virtual bool IsDrawKeyDown()
        {
            return isDrawMouseButtonDown;
        }
        protected virtual bool IsDrawKeyUp()
        {
            return !isDrawMouseButtonDown;
        }
        protected virtual void Update()
        {
            // Check for a Raycast from the tip of the pen
            if (IsDrawKeyDown())
            {
                if (Physics.Raycast(tfTip.position, tfTip.up, out touch, 1000))
                {
                    whiteboard = touch.collider.GetComponent<Whiteboard>();
                    if (whiteboard == null) return;

                    // Set whiteboard parameters
                    whiteboard.SetPenSize(penSize);
                    whiteboard.SetPenColor(penType == PenType.Pen ? penColor : Color.clear);//画笔颜色
                    whiteboard.SetEffect(effectType);
                    whiteboard.SetTouchPosition(touch.textureCoord.x, touch.textureCoord.y);
                    whiteboard.ToggleTouch(true);
                }
            }
            else if (IsDrawKeyUp())
            {
                if (whiteboard)
                {
                    whiteboard.ToggleTouch(false);
                    whiteboard = null;
                }
            }
        }

        #region Editor
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (tfTip)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(tfTip.position, tfTip.position + tfTip.up);
            }
        }
#endif
        #endregion
    }
}