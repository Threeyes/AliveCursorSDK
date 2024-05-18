using System;
using Threeyes.Core;
using UnityEngine;
namespace Threeyes.Decoder
{
    /// <summary>
    /// 
    /// PS:
    /// 1.使用Texture而不是Texture2D，因为很多场景都需要Texture（如RawImage），如果需要可以将值转为原来的Texture2D
    ///
    /// Warning: 
    /// 1.图片使用完成后要Destory，否则会导致多余的内存占用
    /// </summary>
    public class TextureDecoder : DecoderBase<Texture>
    {
        #region Interface
        public override DecodeResult<Texture> DecodeEx(byte[] data, IDecodeOption option = null)
        {
            return DecodeEx(data, option as DecodeOption);
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="markNonReadable"></param>
        /// <returns>null if data is empty</returns>
        public static DecodeResult DecodeEx(byte[] data, DecodeOption option = null)
        {
            DecodeResult decodeResult = new DecodeResult();
            if (option == null)
                option = DecodeOption.Default;

            //PS：只负责读取文件，不需要处理改名等实现
            if (data != null && data.Length > 0)
            {
                Texture2D tempTex = new Texture2D(2, 2, TextureFormat.RGBA32, option.mipChain, option.linear);
                try
                {
                    bool markNonReadable = option.markNonReadable && !option.compress;//(Will force make texture readable if compress is on, )
                    bool canLoad = ImageConversion.LoadImage(tempTex, data, markNonReadable);//Loads PNG or JPG image byte array into a texture.(https://docs.unity3d.com/ScriptReference/ImageConversion.LoadImage.html)

                    if (!canLoad)
                    {
                        decodeResult.errorInfo = $"Can't load image!";
                    }
                    else
                    {
                        if (option.compress)//Compress
                        {
                            tempTex.TryCompress(option.compressInHighQuality);
                        }
                    }

                    decodeResult.value = tempTex;
                }
                catch (Exception e)
                {
                    decodeResult.errorInfo = $"ConvertFromBytes error: " + e;
                }
            }
            else
            {
                decodeResult.errorInfo = "The data is empty!";
            }
            return decodeResult;
        }


        [Serializable]
        public class DecodeOption : IDecodeOption, IEquatable<DecodeOption>
        {
            public static DecodeOption Default { get { return new DecodeOption(); } }

            public bool mipChain = false;//Create mipmap (Useful for 3D texture, but will cost extra memory space)
            public bool linear = false;//true: linear color space; false: sRGB color space (https://docs.unity3d.com/ScriptReference/ImageConversion.LoadImage.html)(https://tiberius-viris.artstation.com/blog/3ZBO/color-space-management-srgb-linear-and-log#:~:text=Linear%20versus%20sRGB%20In%20a%20linear%20color-space%2C%20the,you%20double%20the%20number%2C%20you%20double%20the%20intensity.)
            public bool markNonReadable = false;//Set to false by default, pass true to optionally mark the texture as non-readable. (If true, Unity will stores an additional copy of this texture's pixel data in CPU-addressable memory.)
            public bool compress = false;//Compress texture at runtime to DXT/BCn or ETC formats, which will cost extra time (will force to make texture readable!)
            public bool compressInHighQuality = true;//Passing true for highQuality parameter will dither the source texture during compression, which helps to reduce compression artifacts but is slightly slower. This parameter is ignored for ETC compression.
            public DecodeOption()
            {
                mipChain = false;
                linear = false;
                markNonReadable = false;
                compress = false;
                compressInHighQuality = true;
            }
            public DecodeOption(bool mipChain = false, bool linear = false, bool markNonReadable = false, bool compress = false, bool compressInHighQuality = true)
            {
                this.mipChain = mipChain;
                this.linear = linear;
                this.markNonReadable = markNonReadable;
                this.compress = compress;
                this.compressInHighQuality = compressInHighQuality;
            }

            #region IEquatable
            public override bool Equals(object obj) { return Equals(obj as DecodeOption); }
            public override int GetHashCode() { return base.GetHashCode(); }
            public bool Equals(DecodeOption other)
            {
                if (other == null)
                    return false;
                return
                    mipChain.Equals(other.mipChain) &&
                    linear.Equals(other.linear) &&
                    markNonReadable.Equals(other.markNonReadable) &&
                    compress.Equals(other.compress) &&
                    compressInHighQuality.Equals(other.compressInHighQuality);
            }
            #endregion
        }

        public class DecodeResult : DecodeResult<Texture> { }
    }
}