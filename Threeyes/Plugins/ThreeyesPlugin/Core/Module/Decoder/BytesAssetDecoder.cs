using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Threeyes.Decoder
{
    public class BytesAssetDecoder : DecoderBase<SOBytesAsset>
    {
        #region Interface
        public override DecodeResult<SOBytesAsset> DecodeEx(byte[] data, IDecodeOption option = null)
        {
            return DecodeEx(data, option as DecodeOption);
        }
        #endregion

        public static DecodeResult DecodeEx(byte[] data, DecodeOption option = null)
        {
            DecodeResult decodeResult = new DecodeResult();
            if (option == null)
                option = DecodeOption.Default;

            if (data != null && data.Length > 0)
            {
                decodeResult.value = SOBytesAsset.CreateFromBytes(data);
            }
            else
            {
                decodeResult.errorInfo = "The data is empty!";
            }
            return decodeResult;
        }

        [System.Serializable]
        public class DecodeOption : IDecodeOption
        {
            public static DecodeOption Default { get { return new DecodeOption(); } }

            //ToAdd
        }
        public class DecodeResult : DecodeResult<SOBytesAsset> { }
    }
}