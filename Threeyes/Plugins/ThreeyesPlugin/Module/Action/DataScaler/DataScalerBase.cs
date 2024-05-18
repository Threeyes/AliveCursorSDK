using System.Collections.Generic;
using UnityEngine;
using System;

namespace Threeyes.Action
{
    /// <summary>
    /// Scale specify types with various input
    /// </summary>
    public abstract class DataScalerBase<TInst, TParam>
    where TInst : DataScalerBase<TInst, TParam>, new()
    {
        public static TInst Instance = new TInst();

        //——用于后续为Action提供支持的Modifier信息——
        /// <summary>
        /// Main supported scale type
        /// </summary>
        public static Type MainScaleType { get { return typeof(TParam); } }
        /// <summary>
        /// Fallback supported scale type
        /// </summary>
        public static List<Type> FallbackScaleTypes { get { return Instance.InternalFallbackScaleTypes; } }
        public static List<Type> SupportScaleTypes
        {
            get
            {
                List<Type> listResult = new List<Type>();
                listResult.Add(MainScaleType);
                listResult.AddRange(FallbackScaleTypes);
                return listResult;
            }
        }

        protected abstract List<Type> InternalFallbackScaleTypes { get; }

        /// <summary>
        /// Can scale be parsed?
        /// </summary>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static bool CanParse(object scale)
        {
            if (scale == null)
                return false;
            Type scaleType = scale.GetType();
            if (scaleType == MainScaleType)
                return true;
            return FallbackScaleTypes.Contains(scaleType);
        }
        public static TParam Scale(TParam origin, object scale)
        {
            return Instance.ScaleFunc(origin, scale);
        }
        public static bool TryParse(object scale, out TParam realScale, bool fallback = true, bool logErrorIfConvertFailed = true)
        {
            return Instance.TryParseFunc(scale, out realScale, fallback, logErrorIfConvertFailed);
        }

        protected virtual TParam ScaleFunc(TParam origin, object scale)
        {
            if (scale == null)
                return origin;

            TParam realScale;
            if (TryParseFunc(scale, out realScale))
            {
                return ScaleWithSameTypeFunc(origin, realScale);
            }
            else
            {
                //Debug.LogError("No Scale define for type: {" + scale.GetType() + "}!");//(TryParseFallback已经有打印，不需要在此重新打印）
                return origin;
            }
        }

        /// <summary>
        /// Try convert the input scale to the supported type
        /// </summary>
        /// <param name="scale"></param>
        /// <param name="realScale">The scale with origin type</param>
        /// <param name="fallback">try convert scale from other valid type</param>
        /// <returns></returns>
        /// 
        protected virtual bool TryParseFunc(object scale, out TParam realScale, bool fallback = true, bool logErrorIfConvertFailed = true)
        {
            if (scale is TParam)//#1 Same type
            {
                realScale = (TParam)scale;
                return true;
            }
            else //scale is other type
            {
                realScale = default;
                if (fallback && TryParseFallback(scale, out realScale, logErrorIfConvertFailed))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// PS:
        /// -子类重载此方法，并调用上一级的TryParse，就能找到兼容的类（可能会降低精度）
        /// </summary>
        /// <param name="scale"></param>
        /// <param name="realScale"></param>
        /// <returns></returns>
        /// <param name="logErrorIfConvertFailed"></param>
        protected virtual bool TryParseFallback(object scale, out TParam realScale, bool logErrorIfConvertFailed = true)
        {
            if (logErrorIfConvertFailed)
                Debug.LogError($"Can't find matching type for Scale: {scale} ({(scale != null ? scale.GetType() : "Null")}!");//Fallback失败，会进行提示
            realScale = default;
            return false;
        }

        /// <summary>
        /// Scale origin with scale when they have the same type 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        protected abstract TParam ScaleWithSameTypeFunc(TParam origin, TParam scale);
    }

    ///// <summary>
    ///// 会根据scale的类型，一直Fallback直到找到合适的父类缩放方法
    ///// 
    ///// ToDelete:
    ///// -应该改为fallback+尝试转换object的类型，而不是直接指定Uppder类型，避免耦合
    ///// </summary>
    ///// <typeparam name="TInst"></typeparam>
    ///// <typeparam name="TParam"></typeparam>
    ///// <typeparam name="TUpperScaler"></typeparam>
    ///// <typeparam name="TUpperParam"></typeparam>
    //public abstract class DataScalerBase<TInst, TParam, TUpperScaler, TUpperParam> : DataScalerBase<TInst, TParam>
    //where TInst : DataScalerBase<TInst, TParam>, new()
    //where TUpperScaler : DataScalerBase<TUpperScaler, TUpperParam>, new()//Upper Compatible Scaler
    //{
    //    protected override bool TryParseFallback(object scale, out TParam realScale)
    //    {
    //        realScale = default;//可以给任意值，反正转换失败就不会用
    //        TUpperParam upperTypeScale;
    //        TUpperScaler upperScaler = new TUpperScaler();//ToUpdate: Use Instance instead
    //        if (upperScaler.TryParse(scale, out upperTypeScale))//Try using the upper scaler （会一直往上调用，直到找到匹配的类为止）
    //        {
    //            realScale = ConvertFromUpperScaleFunc(upperTypeScale);
    //            return true;
    //        }
    //        return base.TryParseFallback(scale, out realScale);
    //    }

    //    /// <summary>
    //    /// Conver the upperScale into TParam type for ScaleFunc method
    //    /// 将UpperScale转为TParam类型的缩放（可能会降低精度）
    //    /// </summary>
    //    /// <param name="upperScale"></param>
    //    /// 
    //    /// <returns></returns>
    //    protected abstract TParam ConvertFromUpperScaleFunc(TUpperParam upperScale);
    //}
}
