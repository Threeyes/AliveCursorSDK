using System;
using System.Text;
using UnityEngine;
namespace Threeyes.Decoder
{
    public class TextAssetDecoder : DecoderBase<TextAsset>
    {
        #region Interface
        public override DecodeResult<TextAsset> DecodeEx(byte[] data, IDecodeOption option = null)
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
                decodeResult.value = new TextAsset(option.encoding.GetString(data));
            }
            else
            {
                decodeResult.errorInfo = "The data is empty!";
            }
            return decodeResult;
        }

        [System.Serializable]
        public class DecodeOption : IDecodeOption, IEquatable<DecodeOption>
        {
            public static DecodeOption Default { get { return new DecodeOption(Encoding.UTF8); } }

            public Encoding encoding;

            public DecodeOption()
            {
                this.encoding = Encoding.UTF8;
            }
            public DecodeOption(Encoding encoding)
            {
                this.encoding = encoding;
            }

            #region IEquatable
            public override bool Equals(object obj) { return Equals(obj as DecodeOption); }
            public override int GetHashCode() { return base.GetHashCode(); }
            public bool Equals(DecodeOption other)
            {
                if (other == null)
                    return false;
                return Equals(encoding, other.encoding);
            }
            #endregion
        }

        public class DecodeResult : DecodeResult<TextAsset> { }
    }
}