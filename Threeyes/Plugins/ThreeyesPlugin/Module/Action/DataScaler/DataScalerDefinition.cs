using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Threeyes.Action
{
    //——DataScalers with common value types. if they do not meet the requirements, you can implement them yourself——
    //——PS：目前暂无其他库需要用到此类，后续用到再单独挪到Threeyes.DataScaler命名空间中——
    /// <summary>
    /// Support scale type:
    /// -string
    /// -bool
    /// -int
    /// -float
    /// </summary>
    public class StringScaler : DataScalerBase<StringScaler, string>
    {
        protected override List<Type> InternalFallbackScaleTypes
        {
            get
            {
                return new List<Type>() {
                    typeof(bool),
                    typeof(int),
                    typeof(float)
                };
            }
        }
        protected override string ScaleFunc(string origin, object scale)
        {
            if (scale == null)
                return origin;

            //因为scale可能有其他用途或与origin相关，所以不能直接重载TryParse
            string result = origin;
            if (scale is string && TryParseFunc(scale, out string realScale))//String: Replace the origin
            {
                return ScaleWithSameTypeFunc(origin, realScale);
            }
            else//Other type: Repeat or reverse the string
            {
                if (scale is bool scale_Bool)//false: Reverse
                {
                    if (!scale_Bool)
                        result = Reverse(result);
                }
                else if (scale is int scale_Int)
                {
                    result = Repeat(result, scale_Int);

                    if (scale_Int < 0)//Reverse
                    {
                        result = Reverse(result);
                    }
                }
                else if (scale is float scale_Float)
                {
                    int intScale = (int)scale_Float;//Get the int number part (PS: use [(int)scale] will cause error: InvalidCastException: Specified cast is not valid.)
                    float smallNumScale = scale_Float - intScale;//Get the small number part (Range: (-1,1))
                    result = Repeat(origin, Mathf.Abs(intScale));//Repeat or revert using int rule

                    if (smallNumScale != 0)//Concat the last part（继续添加小数点对应字符串）
                        result += Trim(origin, Mathf.Abs(smallNumScale));

                    if (scale_Float < 0)//Reverse
                    {
                        result = Reverse(result);
                    }
                }
            }
            return result;
        }

        protected override string ScaleWithSameTypeFunc(string origin, string scale)
        {
            if (scale == null)
                scale = "";
            return scale;//直接替换origin
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
    public class ColorScaler : DataScalerBase<ColorScaler, Color>
    {
        protected override List<Type> InternalFallbackScaleTypes
        {
            get
            {
                return new List<Type>() {
                    typeof(bool),
                    typeof(int),
                    typeof(float)
                };
            }
        }
        protected override Color ScaleFunc(Color origin, object scale)
        {
            if (scale == null)
                return origin;

            //因为scale可能有其他用途或与origin相关，所以不能直接重载TryParse
            Color result = origin;
            Color realScale;
            if (scale is Color && TryParseFunc(scale, out realScale))//可用于缩放单个chanel，其他chanel保持不变（如Alpha）
            {
                return ScaleWithSameTypeFunc(origin, realScale);
            }
            else//Other type: Repeat or reverse
            {
                if (scale is bool scale_Bool)//false: Reverse
                {
                    if (!scale_Bool)
                        result = Reverse(result);
                }
                else if (scale is int scale_Int)
                {
                    result = Scale(result, scale_Int);

                    if (scale_Int < 0)//Reverse
                    {
                        result = Reverse(result);
                    }
                }
                else if (scale is float scale_Float)
                {
                    result = Scale(origin, scale_Float);
                }
            }
            return result;
        }
        protected override Color ScaleWithSameTypeFunc(Color origin, Color scale)
        {
            return origin * scale;
        }

        #region Utility
        Color Reverse(Color origin)
        {
            return Color.white - origin;
        }

        Color Scale(Color originStr, float scale)
        {
            return originStr * scale;
        }
        #endregion
    }

    public class BoolScaler : DataScalerBase<BoolScaler, bool>
    {
        protected override List<Type> InternalFallbackScaleTypes { get { return new List<Type>() { }; } }
        protected override bool ScaleWithSameTypeFunc(bool origin, bool scale)
        {
            return scale ? origin : !origin;//Reverse
        }
    }

    public class IntScaler : DataScalerBase<IntScaler, int>
    {
        protected override List<Type> InternalFallbackScaleTypes { get { return new List<Type>() { typeof(bool) }; } }
        protected override bool TryParseFallback(object scale, out int realScale, bool logErrorIfConvertFailed = true)
        {
            bool instScale;
            if (BoolScaler.TryParse(scale, out instScale, logErrorIfConvertFailed: logErrorIfConvertFailed))
            {
                realScale = instScale ? 1 : -1;
                return true;
            }
            return base.TryParseFallback(scale, out realScale, logErrorIfConvertFailed);
        }
        protected override int ScaleWithSameTypeFunc(int origin, int scale)
        {
            return origin * scale;
        }
    }

    public class FloatScaler : DataScalerBase<FloatScaler, float>
    {
        protected override List<Type> InternalFallbackScaleTypes { get { return IntScaler.SupportScaleTypes; } }
        protected override bool TryParseFallback(object scale, out float realScale, bool logErrorIfConvertFailed = true)
        {
            int instScale;
            if (IntScaler.TryParse(scale, out instScale, logErrorIfConvertFailed: logErrorIfConvertFailed))
            {
                realScale = instScale;
                return true;
            }
            return base.TryParseFallback(scale, out realScale, logErrorIfConvertFailed);
        }

        protected override float ScaleWithSameTypeFunc(float origin, float scale)
        {
            return origin * scale;
        }
    }

    public class Vector2Scaler : DataScalerBase<Vector2Scaler, Vector2>
    {
        protected override List<Type> InternalFallbackScaleTypes { get { return FloatScaler.SupportScaleTypes; } }
        protected override bool TryParseFallback(object scale, out Vector2 realScale, bool logErrorIfConvertFailed = true)
        {
            float instScale;
            if (FloatScaler.TryParse(scale, out instScale, logErrorIfConvertFailed: logErrorIfConvertFailed))
            {
                realScale = Vector2.one * instScale;
                return true;
            }
            return base.TryParseFallback(scale, out realScale, logErrorIfConvertFailed);
        }

        protected override Vector2 ScaleWithSameTypeFunc(Vector2 origin, Vector2 scale)
        {
            return origin * scale;
        }
    }

    public class Vector3Scaler : DataScalerBase<Vector3Scaler, Vector3>
    {
        protected override List<Type> InternalFallbackScaleTypes { get { return Vector2Scaler.SupportScaleTypes; } }
        protected override bool TryParseFallback(object scale, out Vector3 realScale, bool logErrorIfConvertFailed = true)
        {
            if (FloatScaler.TryParse(scale, out float instScale_Float, logErrorIfConvertFailed: false))//优先尝试转为float，而不是Vector2，避免损失z轴精度（logErrorIfConvertFailed需要为false，否则如果scale为Vector2就会导致报错）
            {
                realScale = Vector3.one * instScale_Float;
                return true;
            }
            else if (Vector2Scaler.TryParse(scale, out Vector2 instScale_Vector2, false, false))//不再尝试fallback，避免重复调用Float等
            {
                realScale = instScale_Vector2;
                realScale.z = 1;//z轴为0，因此要重置为1，否则该位会被裁剪
                return true;
            }
            return base.TryParseFallback(scale, out realScale, logErrorIfConvertFailed);
        }
        protected override Vector3 ScaleWithSameTypeFunc(Vector3 origin, Vector3 scale)
        {
            origin.Scale(scale);
            return origin;
        }
    }
    public class Vector4Scaler : DataScalerBase<Vector4Scaler, Vector4>
    {
        protected override List<Type> InternalFallbackScaleTypes { get { return Vector3Scaler.SupportScaleTypes; } }
        protected override bool TryParseFallback(object scale, out Vector4 realScale, bool logErrorIfConvertFailed = true)
        {
            if (FloatScaler.TryParse(scale, out float instScale_Float, logErrorIfConvertFailed: false))//优先尝试转为float，而不是Vector2，避免损失z轴精度
            {
                realScale = Vector3.one * instScale_Float;
                return true;
            }
            else if (Vector2Scaler.TryParse(scale, out Vector2 instScale_Vector2, false, false))//不再尝试fallback，避免重复调用Float等
            {
                realScale = instScale_Vector2;
                realScale.z = 1;//z轴为0，因此要重置为1，否则该位会被裁剪
                realScale.w = 1;//w轴为0，因此要重置为1，否则该位会被裁剪
                return true;
            }
            else if (Vector3Scaler.TryParse(scale, out Vector3 instScale_Vector3, false, false))//不再尝试fallback，避免重复调用Float等
            {
                realScale = instScale_Vector3;
                realScale.w = 1;//w轴为0，因此要重置为1，否则该位会被裁剪
                return true;
            }
            return base.TryParseFallback(scale, out realScale, logErrorIfConvertFailed);
        }
        protected override Vector4 ScaleWithSameTypeFunc(Vector4 origin, Vector4 scale)
        {
            origin.Scale(scale);
            return origin;
        }
    }
}