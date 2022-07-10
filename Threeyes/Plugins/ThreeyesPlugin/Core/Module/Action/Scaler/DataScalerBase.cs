using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
namespace Threeyes.Action
{
    /// <summary>
    /// Scale specify types 
    /// </summary>
    public abstract class DataScalerBase<TInst, TParam>
    where TInst : DataScalerBase<TInst, TParam>, new()
    {
        public static TInst Instance = new TInst();
        public virtual TParam Scale(TParam origin, object scale)
        {
            if (scale == null)
                return origin;

            TParam realScale;
            if (TryParse(scale, out realScale))
            {
                return ScaleFunc(origin, realScale);
            }
            else
            {
                Debug.LogError("No Scale define for type: {" + scale.GetType() + "}!");
                return origin;
            }
        }

        /// <summary>
        /// Convert the input scale to TParam type
        /// </summary>
        /// <param name="scale"></param>
        /// <param name="realScale">The scale with origin type</param>
        /// <returns></returns>
        public abstract bool TryParse(object scale, out TParam realScale);

        /// <summary>
        /// Scale value using scale's origin type
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        protected abstract TParam ScaleFunc(TParam origin, TParam scale);
    }
    public abstract class DataScalerBase<TInst, TParam, TUpperScaler, TUpperParam> : DataScalerBase<TInst, TParam>
    where TInst : DataScalerBase<TInst, TParam>, new()
    where TUpperScaler : DataScalerBase<TUpperScaler, TUpperParam>, new()//Upper Compatible Scaler
    {
        /// <summary>
        /// The basic scale value(eg: 1 for float)
        /// </summary>
        protected abstract TParam DefaultUnitScale { get; }

        public override bool TryParse(object scale, out TParam realScale)
        {
            if (scale is TParam)//#1 Conver from origin type
                realScale = (TParam)scale;
            else//#2 Try Conver from upper type
            {
                realScale = DefaultUnitScale;
                TUpperParam upperTypeScale;
                TUpperScaler upperScaler = new TUpperScaler();//ToUpdate: Use Instance instead
                if (upperScaler.TryParse(scale, out upperTypeScale))//Try using the upper scaler
                {
                    realScale = ConvertFromUpperScaleFunc(DefaultUnitScale, upperTypeScale);
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Conver the upperScale into TParam type for ScaleFunc method
        /// </summary>
        /// <param name="scale">unit scale</param>
        /// <param name="upperScale"></param>
        /// <returns></returns>
        protected abstract TParam ConvertFromUpperScaleFunc(TParam scale, TUpperParam upperScale);
    }
}
