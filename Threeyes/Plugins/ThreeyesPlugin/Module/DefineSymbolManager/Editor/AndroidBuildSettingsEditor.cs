#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Threeyes.Editor
{
    [InitializeOnLoad]
    public class AndroidBuildSettingsEditor
    {
        public static void UpdateKeyStoreSetting()
        {
            //设置keystore的密码
            if (SODefineSymbolManager.Instance.useCustomKeyStore)
            {
                bool isChanged = false;
                if (PlayerSettings.Android.keyaliasName != SODefineSymbolManager.Instance.keyaliasNameOverride)
                {
                    PlayerSettings.Android.keyaliasName = SODefineSymbolManager.Instance.keyaliasNameOverride;
                    isChanged = true;
                }
                if (PlayerSettings.Android.keystorePass != SODefineSymbolManager.Instance.keystorePassOverride)
                {
                    PlayerSettings.Android.keystorePass = SODefineSymbolManager.Instance.keystorePassOverride;
                    isChanged = true;
                }
                if (PlayerSettings.Android.keyaliasPass != SODefineSymbolManager.Instance.keyaliasPassOverride)
                {
                    PlayerSettings.Android.keyaliasPass = SODefineSymbolManager.Instance.keyaliasPassOverride;
                    isChanged = true;
                }
                //WritePermission设置为SDCard，便于读写文件
                if (PlayerSettings.Android.forceSDCardPermission != true)
                {
                    PlayerSettings.Android.forceSDCardPermission = true;
                    isChanged = true;
                }

                if (isChanged)
                    Debug.Log("设置keystore " + SODefineSymbolManager.Instance.keyaliasNameOverride + " 的密码");
            }
        }
        static AndroidBuildSettingsEditor()
        {
            UpdateKeyStoreSetting();
        }
    }
}
#endif