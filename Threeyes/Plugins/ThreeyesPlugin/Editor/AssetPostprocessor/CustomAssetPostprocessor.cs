#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Threeyes.Editor
{

    /// <summary>
    /// 导入文件后，自动处理
    /// Bug：会有频繁调用的官方Bug
    /// 参考：Do Tween\_DOTween.Assembly\DOTweenEditor\DOTweenUtilityWindow.cs
    /// </summary>
    //public class ColyuAssetPostprocessor : AssetPostprocessor
    //{
    //    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    //    {

    //        //代码导入完成后，自动搜寻并刷新
    //        SODefineSymbolManager.Instance.SearchAndRefresh();
    //    }
    //}
}
#endif