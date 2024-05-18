using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_PIPELINE_URP
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#endif
namespace Threeyes.ModuleHelper
{
    public static class LazyExtension_URP
    {
#if UNITY_PIPELINE_URP
        /// <summary>
        /// 尝试添加并返回指定模块
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="volumeProfile"></param>
        /// <param name="component"></param>
        /// <returns></returns>
        public static bool TryGetOrAdd<T>(this VolumeProfile volumeProfile, out T component/*, bool isActive, bool removeOnDeactive = false*/) where T : VolumeComponent
        {
            if (!volumeProfile.TryGet(out component))
            {
                component = volumeProfile.Add<T>();
            }
            return component != null;
        }
#endif
    }
}