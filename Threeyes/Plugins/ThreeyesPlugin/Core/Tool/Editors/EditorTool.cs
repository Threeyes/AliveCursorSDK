#if UNITY_EDITOR
using System.Linq;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Threading;
using System.IO;

namespace Threeyes.Core.Editor
{
    /// <summary>
    /// 编辑器通用的方法
    /// PS:不能放在Editor文件夹下，因为外部代码可能访问不了，或者是因为资源加载顺序错误导致无法访问
    /// </summary>
    public static class EditorTool
    {
        #region GameObject
        /// <summary>
        /// Returns true if the given object is part of any kind of Prefab.
        /// Use this to check if a given object is part of any Prefab, regardless of whether it's a Prefab Asset or a Prefab instance.
        /// 
        /// Warning:
        /// -【在PrefabMode中，根Prefab会被全部实例化，此时它就不算时资源的一部分，这时候会返回false！因此需要额外处理！】 For Prefab contents loaded in Prefab Mode, this method will not check the Prefab Asset the loaded contents are loaded from, since these Prefab contents are loaded into a preview scene and are not part of an Asset while being edited in Prefab Mode. （https://docs.unity3d.com/ScriptReference/PrefabUtility.IsPartOfAnyPrefab.html）
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsPartOfAnyPrefab(Object obj)
        {
            return PrefabUtility.IsPartOfAnyPrefab(obj);
        }

        public static bool IsPrefabMode(GameObject go)
        {
            return PrefabStageUtility.GetCurrentPrefabStage() && PrefabStageUtility.GetPrefabStage(go);
        }

        /// <summary>
        /// 是否为场景中的实例物体
        /// 
        /// -否：在PrefabMode下或Prefab资源
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        public static bool IsInstanceGameObject(GameObject go)
        {
            ///#1检查当前物体是否在Prefab Mode，如果是就代表仍然是PrefabAsset（Ref:NetworkIdentity.SetupIDs）
            if (IsPrefabMode(go))
                return false;

            //#2 检查是否存储在磁盘上（资源）
            return !EditorUtility.IsPersistent(go);
            ///Warning:
            ///-当打开Prefab进行编辑时，以下属性会返回1（代表该预制物为Root）
            //return go.scene.rootCount != 0;
        }

        /// <summary>
        /// 是否为预制物（存储在磁盘中）
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        public static bool IsPersistentGameObject(GameObject go)
        {
            return EditorUtility.IsPersistent(go);
        }
        #endregion

        #region Asset
        /// <summary>
        /// 重命名资源，保留引用
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="newName">新名称（不带后缀）</param>
        /// <returns>An empty string, if the asset has been successfully renamed, otherwise an error
        //     message.</returns>
        public static string Rename(Object obj, string newName)
        {
            var relateObjPath = AssetDatabase.GetAssetPath(obj);
            return AssetDatabase.RenameAsset(relateObjPath, newName);
        }

        /// <summary>
        /// 将指定Asset的预览图保存为资源文件，默认存储在obj所在目录的Preview Texture文件夹下
        /// </summary>
        /// <param name="obj">Asset文件夹中的任意文件</param>
        /// <param name="overrideSavedPath">自定义预览图存储路径（包括文件名）</param>
        /// <returns></returns>
        public static Texture CreateAndSaveAssetPreview(Object obj, string overrideSavedPath = "")
        {
            if (obj == null)
            {
                Debug.LogError("Can't create preview for null object");
                return null;
            }
            var relateObjPath = AssetDatabase.GetAssetPath(obj);
            if (!relateObjPath.StartsWith("Assets"))
            {
                Debug.LogError("only support object insde Assets Folder!");
                return null;
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

            ///出错可能原因：
            ///-选中的Asset在只读路径中（如package）
            if (preview == null)
            {
                Debug.LogError($"Unable to create preview for object {obj.name}, perhaps the resource is not in the Asset folder.");
                return null;
            }

            var relatePreviewTexturePath = relateObjPath.Substring(0, relateObjPath.LastIndexOf("/")) + "/Preview Texture";//将预览图存在obj所在目录的Preview Texture文件夹下
            string savePath = EditorPathTool.UnityRelateToAbsPath(relatePreviewTexturePath);

            CreateSaveFolder(savePath);

            //encode to png and then save to assets
            var bytes = preview.EncodeToPNG();
            string name = obj.name + ".png";
            string imagePath = overrideSavedPath.NotNullOrEmpty() ? overrideSavedPath : relatePreviewTexturePath + "/" + name;


            if (File.Exists(imagePath))
                File.Delete(imagePath);
            File.WriteAllBytes(imagePath, bytes);

            //Debug.Log("Create Preview at: " + imagePath);

            //refresh assets
            AssetDatabase.Refresh();

            //change from texture to sprite
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(imagePath);//Assets的目录
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            EditorUtility.SetDirty(importer);
            AssetDatabase.ImportAsset(imagePath);
            AssetDatabase.Refresh();

            Texture objTex = AssetDatabase.LoadAssetAtPath<Texture>(imagePath);
            return objTex;
            //Selection.objects = new Object[] { objTex };
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
        #endregion

        #region Repaint

        public static void RepaintSceneView()
        {
#if UNITY_EDITOR
            SceneView.RepaintAll();
#endif
        }

        public static void RepaintAllViews()
        {
#if UNITY_EDITOR
            InternalEditorUtility.RepaintAllViews();
#endif
        }

        public static void RepaintHierarchyWindow()
        {

#if UNITY_EDITOR
            EditorApplication.RepaintHierarchyWindow();
#endif
        }

        #endregion

        #region Component


        /// <summary>
        /// 强制更新某个Object，常用于Inspector组件的初始化。
        /// 用途：
        ///     ——通过AddComponent添加组件/某个组件首次被添加因此Reset被调用后，组件此时因为不在Inspector显示因此不会被初始化，UnityEvent等类实例字段会维持null，因此要调用这个方法强制更新
        ///Ref：https://forum.unity.com/threads/unity-event-is-null-right-after-addcomponent.819402/#post-5427855 #2
        /// </summary>
        public static void ForceUpdateObject(Object obj)
        {
            SerializedObject so = new SerializedObject(obj);
            so.Update();
        }
        #endregion

        #region Hierarchy
        /// <summary>
        /// Expand Gameobjects on Hierarchy window
        /// 
        /// ref: 
        /// </summary>
        /// <param name="go"></param>
        /// <param name="expand"></param>
        public static void SetExpandedRecursive(GameObject go, bool expand)
        {
            System.Type type = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
            MethodInfo methodInfo = type.GetMethod("SetExpandedRecursive");

            EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");

            EditorWindow editorWindow = EditorWindow.focusedWindow;

            methodInfo.Invoke(editorWindow, new object[] { go.GetInstanceID(), expand });
        }
        #endregion

        #region Scene

        public static T CreateGameObjectAsChild<T>(string name) where T : Component
        {
            return CreateGameObject<T>(name, Selection.activeGameObject ? Selection.activeGameObject.transform : null, isSameLayer: false);
        }

        public static T CreateGameObject<T>(string name, Transform tfParent = null, bool isSameLayer = true, bool isSelect = true) where T : Component
        {
            GameObject go = new GameObject(name);
            T com = go.AddComponent<T>();
            ForceUpdateObject(com);

            if (!tfParent)
                tfParent = Selection.activeGameObject ? Selection.activeGameObject.transform.parent : null;//Try to find the relate gameObject, will be world if the value is null

            go.transform.parent = tfParent;
            if (isSameLayer && Selection.activeGameObject)
                go.transform.SetSiblingIndex(Selection.activeGameObject.transform.GetSiblingIndex() + 1);//如果是同级，那就设置排到下面

            if (tfParent)
            {
                go.transform.localPosition = default;
                go.transform.localRotation = default;
                go.transform.localScale = Vector3.one;
            }

            Undo.RegisterCreatedObjectUndo(go, "Create object");
            if (isSelect)
                Selection.activeGameObject = go;

            RepaintHierarchyWindow();
            return com;
        }

        /// <summary>
        /// 选择并高亮
        /// </summary>
        /// <param name="obj"></param>
        public static void SelectAndHighlight(Object obj)
        {
#if UNITY_EDITOR
            EditorGUIUtility.PingObject(obj);
            Selection.activeObject = obj;
#endif
        }

        /// <summary>
        /// 找到场景中指定名字的物体
        /// </summary>
        /// <param name="targetName"></param>
        /// <returns></returns>
        public static GameObject FindTargetInScene(string targetName)
        {
            Scene sceneTarget = SceneManager.GetActiveScene();
            GameObject goTarget = null;
            foreach (GameObject go in sceneTarget.GetRootGameObjects().ToList())
            {
                //修改：循环遍历
                go.transform.ForEachChildTransform(
                    (tf) =>
                    {
                        if (tf.gameObject.name == targetName)
                            goTarget = tf.gameObject;
                    },
                    true,
                    true);

                if (goTarget != null)
                    break;
            }
            return goTarget;
        }

        #endregion

        #region Others

        /// <summary>
        /// PlayClip on Editor (even not playing)
        /// </summary>
        /// <param name="clip"></param>
        public static void PlayClip(AudioClip clip)
        {
            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
            System.Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
            MethodInfo method = audioUtilClass.GetMethod(
                "PlayClip",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new System.Type[] {
                typeof(AudioClip)
                },
                null
            );
            method.Invoke(
                null,
                new object[] {
                clip
                }
            );
        } // PlayClip()

        /// <summary>
        /// 
        /// </summary>
        public static void StopAllClips()
        {
            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
            System.Type audioUtilClass =
                  unityEditorAssembly.GetType("UnityEditor.AudioUtil");
            MethodInfo method = audioUtilClass.GetMethod(
                "StopAllClips",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new System.Type[] { },
                null
            );
            method.Invoke(
                null,
                new object[] { }
            );
        }

        #endregion

        #region Selection

        /// <summary> 
        /// 得到选中文件的相对路径
        /// </summary>    
        /// <returns></returns>   
        public static List<string> GetSelectionAssetPaths()
        {
            List<string> assetPaths = new List<string>();
            foreach (var guid in Selection.assetGUIDs) // 这个接口才能取到两列模式时候的文件夹  
            {
                if (string.IsNullOrEmpty(guid))
                    continue;
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(path))
                { assetPaths.Add(path); }
            }
            return assetPaths;
        }

        #endregion

    }
}
#endif