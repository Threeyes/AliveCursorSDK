using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Threeyes.Core
{
    /// <summary>
    /// 
    /// </summary>
    public interface IMaterialProvider
    {
        /// <summary>
        /// Returns the first instantiated Material
        /// </summary>
        Material TargetMaterial { get; }

        /// <summary>
        /// The shared material
        /// </summary>
        Material TargetSharedMaterial { get; }//方便非运行模式时获取原资源文件
    }
}