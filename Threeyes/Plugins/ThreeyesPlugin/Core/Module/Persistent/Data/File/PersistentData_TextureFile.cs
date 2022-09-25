using UnityEngine;
using UnityEngine.UI;
using Threeyes.Data;

namespace Threeyes.Persistent
{
	/// <summary>
	/// Image file
	/// </summary>
	public class PersistentData_TextureFile : PersistentData_FileBase<Texture, TextureEvent, DataOption_TextureFile>
    {
        void Reset()
        {
#if UNITY_EDITOR
            RawImage rawImage = gameObject.GetComponent<RawImage>();
            if (rawImage)
            {
                Editor.EditorTool.ForceUpdateObject(this);
                DefaultAsset = rawImage.texture;
                var tex = rawImage.texture;
                if (tex)
                {
                    Key = tex.name;
                    string filePath = UnityEditor.AssetDatabase.GetAssetPath(tex);
                    Debug.Log(filePath);
                    defaultValue = PathTool.GetFileName(filePath);
                }

                //ToAdd:增加针对Property的调用
                onAssetChanged.AddPersistentListenerOnce(rawImage, "texture", true);
                UnityEditor.EditorUtility.SetDirty(this);
            }

            //ToAdd:增加针对Renderer的通用RenderHelper中间组件，如果检查到有该组件则自动绑定；用户也可自行实现相关功能
#endif
        }

        #region Editor
#if UNITY_EDITOR

        //——MenuItem——
        static string instName = "TexturePD ";
        [UnityEditor.MenuItem(strMenuItem_Root_File + "Texture", false, intBasicMenuOrder + 1)]
        public static void CreateInst()
        {
            Editor.EditorTool.CreateGameObjectAsChild<PersistentData_TextureFile>(instName);
        }

        //——Hierarchy GUI——
        public override string ShortTypeName { get { return "Texture"; } }

#endif
        #endregion
    }
}