using Threeyes.Decoder;
using UnityEngine;

namespace Threeyes.UI
{
    /// <summary>
    /// Decode and play image or gif file
    /// </summary>
    [RequireComponent(typeof(GifPlayer))]
    public class ImagePlayer : ComponentHelperBase<GifPlayer>
    {
        public TextureDecoder.DecodeOption textureDecodeOption = new TextureDecoder.DecodeOption();
        /// <summary>
        /// Init with gif files
        /// (Invoked by PD_BytesFile)
        /// </summary>
        /// <param name="arrByteGif"></param>
        public void Init(byte[] arrByteGif)
        {
            Comp.Reset();//重置并禁用GifPlayer

            //#1 先尝试以图片的格式解压
            TextureDecoder.DecodeResult decodeResult = TextureDecoder.DecodeEx(arrByteGif, textureDecodeOption);
            if (decodeResult.errorInfo.IsNullOrEmpty())
            {
                Comp.onUpdateTexture.Invoke(decodeResult.value);//直接使用GifPlayer处理图片
            }
            else//#2 读取失败才调用GifPlayer 
            {
                Comp.Init(arrByteGif);
            }
        }
    }
}