using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Threeyes.Decoder
{
    public abstract class DecoderBase
    {
        /// <summary>
        /// Support type to decode
        /// </summary>
        public abstract Type SupportType { get; }
    }

    /// <summary>
    /// 
    /// PS：
    /// 1.不将Option作为泛型类型参数，更便于外部调用
    /// </summary>
    /// <typeparam name="TAsset"></typeparam>
    public abstract class DecoderBase<TAsset> : DecoderBase
    {
        //To Rename
        public override Type SupportType { get { return typeof(TAsset); } }

        public virtual TAsset Decode(byte[] data, IDecodeOption option = null)
        {
            return DecodeEx(data, option).value;
        }
        public abstract DecodeResult<TAsset> DecodeEx(byte[] data, IDecodeOption option = null);
    }

    public interface IDecodeOption { }

}
