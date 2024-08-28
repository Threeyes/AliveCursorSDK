using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Threeyes.Config;

namespace Threeyes.GameFramework
{
    /// <summary>
    /// Ref:https://git.fh-aachen.de/MakeItTrue2/VR/-/blob/ea8b3e6728db29fd229603b12b3a5f88c315fa54/unity-game/Assets/Scripts/WhiteBoard/Whiteboard.cs
    /// 
    /// ToUpdate:
    /// -Support UGUI-Image:https://forum.unity.com/threads/how-to-raycast-onto-a-unity-canvas-ui-image.855259/
    /// </summary>
    public class Whiteboard : ConfigurableInstanceBase<Whiteboard, SOWhiteboardConfig, Whiteboard.ConfigInfo>
    {
        //Runtime
        private Texture2D texture;
        private Color[] arrPenColor;
        private int penSize = 10;
        private EffectType effectType = EffectType.None;
        private bool touching, touchingLast;
        private float posX, posY;
        private float lastX, lastY;

        #region Public
        public virtual void Clear()
        {
            Color32[] resetColorArray = texture.GetPixels32();
            for (int i = 0; i < resetColorArray.Length; i++)
            {
                resetColorArray[i] = Color.clear;
            }
            texture.SetPixels32(resetColorArray);
            texture.Apply();
        }
        public virtual void ToggleTouch(bool touching)
        {
            this.touching = touching;
        }
        public virtual void SetTouchPosition(float x, float y)
        {
            posX = x;
            posY = y;
        }
        public virtual void SetPenSize(int size)
        {
            penSize = size;
        }
        public virtual void SetPenColor(Color color)
        {
            arrPenColor = Enumerable.Repeat(color, penSize * penSize).ToArray();
        }
        public virtual void SetEffect(EffectType effectType)
        {
            this.effectType = effectType;
        }
        #endregion

        protected virtual void Awake()
        {
            //Init whiteboard texture
            Renderer renderer = GetComponent<Renderer>();
            texture = new Texture2D((int)Config.textureSize.x, (int)Config.textureSize.y, TextureFormat.RGBA32, false);
            texture.name = "Runtime Whiteboard";
            Clear();//Default is transparent
            renderer.material.mainTexture = texture;

            //确保白板与贴图尺寸一致
            Vector3 curScale = transform.localScale;
            curScale.x = curScale.y * texture.width / texture.height;
            transform.localScale = curScale;
        }

        protected virtual void Update()
        {
            // Transform textureCoords into "pixel" values
            int x = (int)(posX * texture.width - penSize / 2);
            int y = (int)(posY * texture.height - penSize / 2);

            x = Mathf.Clamp(x, 0, texture.width - 1);
            y = Mathf.Clamp(y, 0, texture.height - 1);

            // Only set the pixels if we were touching last frame
            if (touchingLast)
            {
                // Set base touch pixels
                SetTexturePixels(x, y, penSize, penSize, arrPenColor);

                // Interpolate pixels from previous touch
                for (float t = 0.01f; t < 1.00f; t += 0.01f)
                {
                    int lerpX = (int)Mathf.Lerp(lastX, x, t);
                    int lerpY = (int)Mathf.Lerp(lastY, y, t);
                    SetTexturePixels(lerpX, lerpY, penSize, penSize, arrPenColor);
                }
            }

            // If currently touching, apply the texture
            if (touching)
            {
                texture.Apply();
            }

            lastX = x;
            lastY = y;
            touchingLast = touching;
        }
        Color[] arrColor;
        protected virtual void SetTexturePixels(int x, int y, int blockWidth, int blockHeight, Color[] colors)
        {
            ///ToAdd:增加像素的混合模式设置
            if (effectType == EffectType.Smear)
            {
                //涂抹效果（原因是不断采集上一帧的颜色，而上一帧的颜色有重叠）
                arrColor = texture.GetPixels(x, y, blockWidth, blockHeight);
                for (int i = 0; i != arrColor.Length; i++)
                {
                    colors[i] = Color.Lerp(colors[i], arrColor[i], 0.5f);//Mix two colors
                }
            }

            //ToAdd：EffectType.None时实现正常颜色混合功能：在鼠标按下前先提前缓存好Texture所有的Color，然后才能正常计算混合；或克隆贴图作为置底图层，当前鼠标按下绘画的内容绘制到临时Texture上，鼠标抬起后才混合到新Texture上

            texture.SetPixels(x, y, blockWidth, blockHeight, colors);
        }

        #region Define
        /// <summary>
        /// PS:
        /// -Reload the mod after you change these settings
        /// </summary>
        [System.Serializable]
        public class ConfigInfo : SerializableDataBase
        {
            public Vector2 textureSize = new Vector2(2880, 1440);//默认2：1，能够覆盖大多数16屏幕
            public Color defaultPenColor = new Color(1, 1, 1, 0.5f);//默认半透明，避免遮挡背景
            public int defaultPenSize = 5;
            public Vector2 penSizeRange = new Vector2(1, 50);
        }
        public enum PenType
        {
            Pen = 0,
            Eraser = 10
        }
        public enum EffectType
        {
            None = 0,
            Smear,//涂抹
        }
        #endregion

    }
}