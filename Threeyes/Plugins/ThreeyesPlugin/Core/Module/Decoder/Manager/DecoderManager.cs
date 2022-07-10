using System;
using System.Collections.Generic;
using System.Linq;
namespace Threeyes.Decoder
{
    public static class DecoderManager
    {
        public static List<DecoderBase> listDecoder = new List<DecoderBase>();
        public static List<Type> listSupportType = new List<Type>();

        public static bool CanDecode(Type assetType)
        {
            if (assetType == null)
                return false;
            return listSupportType.Contains(assetType);
        }

        public static DecoderBase FindDecoder(Type assetType)
        {
            if (assetType == null)
                return null;
            return listDecoder.FirstOrDefault(d => d.SupportType == assetType);
        }


        public static TAsset Decode<TAsset>(byte[] data, IDecodeOption option = null)
        {
            return DecodeEx<TAsset>(data, option).value;
        }

        public static DecodeResult<TAsset> DecodeEx<TAsset>(byte[] data, IDecodeOption option = null)
        {
            DecodeResult<TAsset> defaultDecodeResult = new DecodeResult<TAsset>();

            Type assetType = typeof(TAsset);
            if (!CanDecode(assetType))
                return defaultDecodeResult;

            DecoderBase targetDecoder = FindDecoder(assetType);
            if (targetDecoder is DecoderBase<TAsset> decoderReal)
            {
                return decoderReal.DecodeEx(data, option);
            }
            return defaultDecodeResult;
        }
    }
}