using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Threeyes.Action
{
    public class StringScaler : DataScalerBase<StringScaler, string>
    {
        public override string Scale(string origin, object scale)
        {
            if (scale == null)
                return origin;

            // Repeat or reverse the line
            string result = origin;
            string strRealScale;
            if (TryParse(scale, out strRealScale))
            {
                return ScaleFunc(origin, strRealScale);
            }
            else//Different handle for each type
            {
                if (scale is bool)
                {
                    result = Reverse(result);
                }
                else if (scale is int)
                {
                    int realScale = (int)scale;
                    result = ScaleFunc(result, realScale);
                    //Reverse
                    if (realScale < 0)
                    {
                        result = Reverse(result);
                    }
                }
                else if (scale is float)
                {
                    float realScale = (float)scale;

                    int intScale = (int)realScale;//Get the int number part (PS: use [(int)scale] will cause error: InvalidCastException: Specified cast is not valid.)
                    float smallNumScale = realScale - intScale;//Get the small number part (Range: (-1,1))
                    result = ScaleFunc(origin, Mathf.Abs(intScale));//Repeat or revert using int rule

                    if (smallNumScale != 0)
                        result += Trim(origin, Mathf.Abs(smallNumScale));//Concat the last part

                    if (realScale < 0)//Reverse
                    {
                        result = Reverse(result);
                    }
                }
                else
                {
                    Debug.LogError("No Scale define for type: {" + scale.GetType() + "}!");
                }
            }
            return result;
        }

        public override bool TryParse(object scale, out string realScale)
        {
            if (scale is string)//#1 Conver from origin type
            {
                realScale = (string)scale;
                return true;
            }
            else
            {
                //string as scale is useless 
                realScale = "";
                return false;
            }
        }

        string ScaleFunc(string origin, int scale)
        {
            origin = Repeat(origin, scale);
            return origin;
        }
        protected override string ScaleFunc(string origin, string scale)
        {
            //Will replace the origin if scale is not null
            if (!string.IsNullOrEmpty(scale))
                return scale;
            return origin;
        }

        #region Utility

        string Reverse(string originStr)
        {
            return string.Concat(originStr.Reverse());
        }

        string Repeat(string originStr, int repeatCount)
        {
            return string.Concat(Enumerable.Repeat(originStr, repeatCount));
        }

        string Trim(string originStr, float trimPercent)
        {
            int desireLength = (int)(originStr.Length * trimPercent);
            if (desireLength <= originStr.Length)
                originStr = originStr.Substring(0, desireLength);
            return originStr;
        }

        #endregion
    }

    public class BoolScaler : DataScalerBase<BoolScaler, bool>
    {
        public override bool TryParse(object scale, out bool realScale)
        {
            realScale = true;//Default is true
            if (scale is bool)
                realScale = (bool)scale;
            else//Can't convert
                return false;

            return true;
        }

        protected override bool ScaleFunc(bool origin, bool scale)
        {
            return scale ? origin : !origin;//Reverse
        }
    }

    public class IntScaler : DataScalerBase<IntScaler, int, BoolScaler, bool>
    {
        protected override int DefaultUnitScale { get { return 1; } }

        protected override int ConvertFromUpperScaleFunc(int scale, bool upperScale)
        {
            return upperScale ? scale : -scale;
        }

        protected override int ScaleFunc(int origin, int scale)
        {
            return origin * scale;
        }
    }

    public class FloatScaler : DataScalerBase<FloatScaler, float, IntScaler, int>
    {
        protected override float DefaultUnitScale { get { return 1; } }

        protected override float ConvertFromUpperScaleFunc(float scale, int upperScale)
        {
            return scale * upperScale;
        }

        protected override float ScaleFunc(float origin, float scale)
        {
            return origin * scale;
        }
    }

    public class Vector2Scaler : DataScalerBase<Vector2Scaler, Vector2, FloatScaler, float>
    {
        protected override Vector2 DefaultUnitScale { get { return Vector2.one; } }

        protected override Vector2 ConvertFromUpperScaleFunc(Vector2 scale, float upperScale)
        {
            return scale * upperScale;
        }

        protected override Vector2 ScaleFunc(Vector2 origin, Vector2 scale)
        {
            return origin * scale;
        }
    }

    public class Vector3Scaler : DataScalerBase<Vector3Scaler, Vector3, FloatScaler, float>//The Upper class can't be Vector2, or else it will trim the last value
    {
        protected override Vector3 DefaultUnitScale { get { return Vector3.one; } }

        protected override Vector3 ConvertFromUpperScaleFunc(Vector3 scale, float upperScale)
        {
            return scale * upperScale;
        }

        protected override Vector3 ScaleFunc(Vector3 origin, Vector3 scale)
        {
            origin.Scale(scale);
            return origin;
        }
    }
    public class Vector4Scaler : DataScalerBase<Vector4Scaler, Vector4, FloatScaler, float>//The Upper class can't be Vector2, or else it will trim the last value
    {
        protected override Vector4 DefaultUnitScale { get { return Vector4.one; } }

        protected override Vector4 ConvertFromUpperScaleFunc(Vector4 scale, float upperScale)
        {
            return scale * upperScale;
        }

        protected override Vector4 ScaleFunc(Vector4 origin, Vector4 scale)
        {
            origin.Scale(scale);
            return origin;
        }
    }

    public class ColorScaler : DataScalerBase<ColorScaler, Color, FloatScaler, float>
    {
        protected override Color DefaultUnitScale { get { return Color.white; } }

        protected override Color ScaleFunc(Color origin, Color scale)
        {
            return origin * scale;
        }

        protected override Color ConvertFromUpperScaleFunc(Color scale, float upperScale)
        {
            return scale * upperScale;
        }
    }
}
