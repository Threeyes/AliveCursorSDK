using System.Collections;
using System.Collections.Generic;
using Threeyes.Core;
using UnityEngine;
namespace Threeyes.ModuleHelper
{
    /// <summary>
    /// 管理RenderTexture的创建
    /// </summary>
    public class RenderTextureHelper : MonoBehaviour
    {
        //Config
        public int width = 800;
        public int height = 600;
        public int depth = 16;
        public RenderTextureFormat format = RenderTextureFormat.ARGB32;

        public TextureEvent onTextureCreated;//针对RenderHelper等接收Texture参数
        public RenderTextureEvent onRenderTextureCreated;//针对Camera等接收RenderTexture参数
                                 
        //Runtime
        RenderTexture renderTexture;
        private void Start()
        {
            CreateTexture();
        }

        public void CreateTexture()
        {
            TryDestroyRenderTexture();
            renderTexture = new RenderTexture(width, height, depth, format);
            if (renderTexture.Create())
            {
                onTextureCreated.Invoke(renderTexture);
                onRenderTextureCreated.Invoke(renderTexture);
            }
            else
            {
                Debug.LogError("Failed to create rendertexture!;");
            }
        }

        private void OnDestroy()
        {
            TryDestroyRenderTexture();
        }

        private void TryDestroyRenderTexture()
        {
            if (renderTexture)
                Destroy(renderTexture);
        }
    }
}