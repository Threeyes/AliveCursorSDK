using System;

namespace Threeyes.Decoder
{
    public class DecodeResultBase
    {
        public virtual object ObjValue { get { return null; } }

        public string errorInfo = null;
    }

    public class DecodeResult<TValue> : DecodeResultBase
    {
        public override object ObjValue { get { return value; } }

        public TValue value;
    }
}