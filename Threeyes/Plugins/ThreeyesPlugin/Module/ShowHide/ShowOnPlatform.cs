using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Threeyes.ShowHide
{
    [DefaultExecutionOrder(-30000)]
    /// <summary>
    /// 只在指定平台上显示
    /// </summary>
    public class ShowOnPlatform : ShowAndHide
    {
        //在以下系统中激活
        public List<RuntimePlatform> listRunTime = new List<RuntimePlatform>() { RuntimePlatform.WindowsPlayer };

        public bool isShowOnEditor = true;//在编辑器中持续显示，true会复活最终的平台
        protected override void Awake()
        {
            base.Awake();

            //以下宏定义会根据当前Build Setttings中的设置，自动激活 file:///D:/Program%20Files/Unity%20Group/Unity_Latest/Editor/Data/Documentation/en/Manual/PlatformDependentCompilation.html

            bool shouldShow = false;
            //Debug.Log(Application.platform);
            //以下宏定义会根据当前Build Setttings中的设置，自动激活 file:///D:/Program%20Files/Unity%20Group/Unity_Latest/Editor/Data/Documentation/en/Manual/PlatformDependentCompilation.html

            shouldShow = listRunTime.Contains(Application.platform);
            //#if UNITY_STANDALONE
            //        shouldShow = listRunTime.Contains(RuntimePlatform.WindowsPlayer);
            //#elif UNITY_ANDROID
            //        shouldShow = listRunTime.Contains(RuntimePlatform.Android);
            //#elif UNITY_IOS
            //        shouldShow = listRunTime.Contains(RuntimePlatform.IPhonePlayer);
            //#endif

#if UNITY_EDITOR
            if (isShowOnEditor)
                shouldShow = true;
#endif
            Show(shouldShow);
        }

    }
}