#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace Threeyes.Editor
{
    /// <summary>
    /// 为Project窗口选中的资源生成预览图
    /// </summary>
    public static class AssetMenuEditor_CreateAssetPrevew
    {
        [MenuItem(EditorDefinition.AssetMenuItemPrefix_Create + "Create Asset Preview")]
        private static void CreatePreview()
        {
            CreateAndSaveAssetPreview(Selection.objects);
        }

        /// <summary>
        /// [EDITOR ONLY] Create and save a preview for an object
        /// </summary>
        /// <param name="obj">Object to use to create preview</param>
        public static void CreateAndSaveAssetPreview(Object[] objs)
        {
            foreach (var obj in objs)
            {
                if (obj == null)
                {
                    Debug.LogError("Can't create preview for null object");
                    continue;
                }

                Texture2D preview = AssetPreview.GetAssetPreview(obj);

                //wait untill unity loads preview
                int tm = 0;
                while (preview == null)
                {
                    Thread.Sleep(100);
                    preview = AssetPreview.GetAssetPreview(obj);
                    tm += 100;
                    if (tm >= 3000) //3 sec countdown
                        break;
                }

                if (preview == null)
                {
                    Debug.LogError("Unable to create preview for object: " + obj.name);
                    continue;
                }


                var relateObjPath = AssetDatabase.GetAssetPath(obj);
                var relatePreviewTexturePath = relateObjPath.Substring(0, relateObjPath.LastIndexOf("/")) + "/Preview Texture";//Assets/XXX

                string savePath = Path.Combine(Application.dataPath, relatePreviewTexturePath.Replace("Assets/", string.Empty));

                CreateSaveFolder(savePath);

                //encode to png and then save to assets
                var bytes = preview.EncodeToPNG();
                string name = obj.name + ".png";
                string imagePath = relatePreviewTexturePath + "/" + name;


                if (File.Exists(name))
                    File.Delete(name);
                File.WriteAllBytes(imagePath, bytes);

                Debug.Log("创建预览图：\r\n" + imagePath);

                //refresh assets
                AssetDatabase.Refresh();

                //change from texture to sprite
                TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(imagePath);//Assets的目录
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                EditorUtility.SetDirty(importer);
                AssetDatabase.ImportAsset(imagePath);

                AssetDatabase.Refresh();

                var objTex = AssetDatabase.LoadAssetAtPath(imagePath, typeof(Object));
                Selection.objects = new Object[] { objTex };
            }
        }

        /// <summary>
        /// [EDITOR ONLY] Check if the save directory exitst. If no creates it
        /// </summary>
        static void CreateSaveFolder(string savePath)
        {
            if (!Directory.Exists(savePath))
            {
                Debug.Log("Created directory: " + savePath);
                Directory.CreateDirectory(savePath);
                AssetDatabase.Refresh();
            }
        }
    }
}
#endif