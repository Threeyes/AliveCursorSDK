using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
namespace Threeyes.Decoder
{
    public static class LazyExtension_Decoder
    {
        //——Gif——
        public static List<GifFrameData> ToListGifFrameData(this byte[] data, GifDecoder.DecodeOption option = null)
        {
            return data.ToListGifFrameDataEx(option).value;
        }
        public static GifDecoder.DecodeResult ToListGifFrameDataEx(this byte[] data, GifDecoder.DecodeOption option = null)
        {
            return GifDecoder.DecodeEx(data, option);
        }
        public static async Task<List<GifFrameData>> ToListGifFrameDataAsync(this byte[] data, CancellationToken cancellationToken, GifDecoder.DecodeOption option = null)
        {
            var result = await data.ToListGifFrameDataExAsync(cancellationToken, option);
            return result.value;
        }
        public static Task<GifDecoder.DecodeResult> ToListGifFrameDataExAsync(this byte[] data, CancellationToken cancellationToken, GifDecoder.DecodeOption option = null)
        {
            return GifDecoder.DecodeExAsync(data, cancellationToken, option);
        }

        //——Image——
        public static Texture ToTexture(this byte[] data, TextureDecoder.DecodeOption option = null)
        {
            return data.ToTextureEx(option).value;
        }
        public static TextureDecoder.DecodeResult ToTextureEx(this byte[] data, TextureDecoder.DecodeOption option = null)
        {
            return TextureDecoder.DecodeEx(data, option);
        }

        //——TextAsset——
        public static TextAsset ToTextAsset(this byte[] data, TextAssetDecoder.DecodeOption option = null)
        {
            return data.ToTextAssetEx(option).value;
        }
        public static TextAssetDecoder.DecodeResult ToTextAssetEx(this byte[] data, TextAssetDecoder.DecodeOption option = null)
        {
            return TextAssetDecoder.DecodeEx(data, option);
        }
    }
}