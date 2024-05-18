using System.Collections;
using System.Collections.Generic;
using Threeyes.Core;
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
            if (option == null)//ToUse
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
        public class DecodeOption : IDecodeOption, System.IEquatable<DecodeOption>
        {
            public static DecodeOption Default { get { return new DecodeOption(); } }

            //ToAdd

            #region IEquatable
            public override bool Equals(object obj) { return Equals(obj as DecodeOption); }
            public override int GetHashCode() { return base.GetHashCode(); }
            public bool Equals(DecodeOption other)
            {
                if (other == null)
                    return false;
                return true;
            }
            #endregion
        }
        public class DecodeResult : DecodeResult<SOBytesAsset> { }
    }
}