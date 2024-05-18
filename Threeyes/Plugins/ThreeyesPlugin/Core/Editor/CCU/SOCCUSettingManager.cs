#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Threeyes.Core.Editor
{
    /// <summary>
    /// 
    /// PS:
    /// -为了避免名字过长，所以使用缩写
    /// Todo：
    /// -自动创建该Manager（可以由ConditionalCompilationUtility发起）
    /// -确认是否需要放在Editor文件夹外，避免无法正常加载
    /// -【V2】增加是否根据平台分别使用对应后缀的SO进行存储，参考SODefineSymbolManager_Wasted.Instance
    /// </summary>
    [CreateAssetMenu(menuName = "Threeyes/Manager/CCUSettingManager")]
    public class SOCCUSettingManager : SOInstanceBase<SOCCUSettingManager, SOCCUSettingManager.ManagerInfo>
    {
        #region Property & Field
        public bool Autoupdate
        {
            get
            {
                return autoupdate;
            }
            set
            {
                autoupdate = value;
                EditorUtility.SetDirty(Instance);
            }
        }
        [SerializeField] protected bool autoupdate = true;
        #endregion

        #region Defines
        public class ManagerInfo : SOInstacneInfo
        {
            public override string pathInResources { get { return "Threeyes"; } }
            public override string defaultName { get { return "ConditionalCompilationSetting"; } }
        }
        #endregion
    }
}
#endif