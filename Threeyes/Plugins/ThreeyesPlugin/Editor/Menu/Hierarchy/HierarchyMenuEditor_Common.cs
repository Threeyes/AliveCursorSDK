#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

namespace Threeyes.Editor
{
    //Todo:优化菜单，从Menu中移动到Hierarchy中
    /// <summary>
    /// 快捷键（菜单栏）
    /// </summary>
    public static class HierarchyMenuEditor_Common
    {

        #region Activation (显隐物体并激活特定组件相关功能）

        const string ActivationPrefix = EditorDefinition.HierarchyMenuItemPrefix + "Activation/";

        /// <summary>
        /// 单独显示/隐藏本层选中物体
        /// </summary>
        [MenuItem(ActivationPrefix + "ToggleActiveSolo #%t")]
        static void ToggleActiveSolo()
        {
            GameObject goCurSelect = Selection.activeGameObject;
            if (!goCurSelect)
                return;

            CanvasGroupHelper canvasGroupHelper = goCurSelect.GetComponent<CanvasGroupHelper>();
            if (canvasGroupHelper)//针对CanvasGroupGroupHelper，只显示不隐藏
            {
                canvasGroupHelper.Show();
                return;
            }

            bool isCurActive = goCurSelect.activeInHierarchy;
            Transform tfParent = goCurSelect.transform.parent;
            if (!tfParent)//没有父物体
            {
                Undo.RegisterFullObjectHierarchyUndo(goCurSelect, "ToggleActiveSolo " + goCurSelect);
                ToggleActiveFunc(goCurSelect, !goCurSelect.activeInHierarchy);
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(tfParent.gameObject, "ToggleActiveSolo " + tfParent.name);
            foreach (Transform tf in tfParent)
            {
                if (tf.gameObject == goCurSelect)
                {
                    tf.gameObject.SetActive(!isCurActive);
                }
                else
                {
                    tf.gameObject.SetActive(isCurActive);
                }
            }
        }

        [MenuItem(ActivationPrefix + "ToggleActive %t")]
        static void ToggleActive()
        {
            // Register the creation in the undo system
            Undo.RegisterCompleteObjectUndo(Selection.gameObjects, "ToggleActive " + Selection.gameObjects.Count());

            foreach (var go in Selection.gameObjects)
            {
                ToggleActiveFunc(go, !go.activeInHierarchy);
            }
        }

        static void ToggleActiveFunc(GameObject goCurSelect, bool isActive)
        {
            CanvasGroupHelper canvasGroupHelper = goCurSelect.GetComponent<CanvasGroupHelper>();
            if (canvasGroupHelper)
            {
                canvasGroupHelper.Show(isActive);
                return;
            }

            ShowAndHide showAndHide = goCurSelect.GetComponent<ShowAndHide>();
            //因为当前的判断方法为物体的激活状态，因此只针对物体，
            if (showAndHide)
            {
                if (showAndHide.hideType == ShowAndHide.HideType.GameObject)
                {
                    showAndHide.Show(isActive);
                    return;
                }
            }

            IShowHide showHideInterface = goCurSelect.GetComponent<IShowHide>();
            if (showHideInterface != null)
            {
                showHideInterface.IsShowing = isActive;//更新其状态（因为其显隐方法都不一样，而且非运行模式下可能无效，因此不直接调用）
            }
            goCurSelect.SetActive(isActive);
        }

        #endregion

        #region Duplicate

        const string DuplicatePrefix = EditorDefinition.HierarchyMenuItemPrefix + "Duplicate/";
        static List<GameObject> listGoSelect = new List<GameObject>();


        [MenuItem(DuplicatePrefix + "Common/" + "Cut  #%x")]
        static void Cut()
        {
            listGoSelect = Selection.gameObjects.ToList();
            Debug.Log("Cut");
        }

        [MenuItem(DuplicatePrefix + "Common/" + "Paste  #%v")]
        static void Paste()
        {
            if (listGoSelect.Count > 0)
            {
                foreach (GameObject goSelect in listGoSelect)
                {
                    Undo.RegisterFullObjectHierarchyUndo(goSelect, "Paste " + goSelect.name);
                    GameObject goParent = Selection.activeGameObject;
                    goSelect.transform.SetParent(goParent.transform, true);
                    Debug.Log("Paste");
                }
                Selection.objects = listGoSelect.ConvertAll(
                    new System.Converter<GameObject, Object>(
                        (go) =>
                        {
                            return go;
                        })).ToArray();
            }
        }

        #region TimeLine

        /// <summary>
        /// 复制Timeline并保持Binding
        /// 参考：https://forum.unity.com/threads/duplicating-a-timeline-loses-all-the-bindings-unity-v2017-2-0b6.488138/
        /// </summary>
        [MenuItem(DuplicatePrefix + "Timeline/" + "Duplicate With Bindings")]
        public static void DuplicatePlayableDirectorWithBindings()
        {
            if (UnityEditor.Selection.activeGameObject == null)
                return;

            var playableDirector = UnityEditor.Selection.activeGameObject.GetComponent<PlayableDirector>();
            if (playableDirector == null)
                return;

            var playableAsset = playableDirector.playableAsset;
            if (playableAsset == null)
                return;

            var path = AssetDatabase.GetAssetPath(playableAsset);
            if (string.IsNullOrEmpty(path))
                return;

            string newPath = path.Replace(".", " Clone.");
            if (!AssetDatabase.CopyAsset(path, newPath))
            {
                Debug.LogError("Couldn't Clone Asset");
                return;
            }

            var newPlayableAsset = AssetDatabase.LoadMainAssetAtPath(newPath) as PlayableAsset;

            var tempGo = GameObject.Instantiate(UnityEditor.Selection.activeGameObject);
            var tempPlayableDirector = tempGo.GetComponent<PlayableDirector>();
            var tempBindings = newPlayableAsset.outputs.ToArray();

            //针对选中的场景物体进行操作
            playableDirector.playableAsset = newPlayableAsset;
            for (int i = 0; i < tempBindings.Length; i++)
            {
                playableDirector.SetGenericBinding(newPlayableAsset.outputs.ToArray()[i].sourceObject, tempPlayableDirector.GetGenericBinding(tempPlayableDirector.playableAsset.outputs.ToArray()[i].sourceObject));
            }

            newPlayableAsset.name = playableAsset.name + " Clone";
            GameObject.DestroyImmediate(tempGo);
            Selection.activeObject = newPlayableAsset;
            Debug.Log("DuplicateWithBindings Completed");
        }

        #endregion

        public const int startOrder = 1000;

        /// <summary>
        /// 复制物体，但保持同名
        /// Ctrl + Shift +D
        /// </summary>
        [MenuItem(DuplicatePrefix + "Common/" + "Duplicate Advance #%d", false, startOrder)]
        static void DuplicateObjectAdvance()
        {
            if (Selection.gameObjects.Length > 0)
            {
                List<GameObject> listSelectedGO = Selection.gameObjects.ToList();//先缓存起来，否则Selection.gameObjects会动态发生变化

                List<GameObject> listCreatedGO = new List<GameObject>();

                foreach (var go in listSelectedGO)
                {
                    GameObject goCurSelect = go;
                    Selection.objects = new Object[] { goCurSelect };
                    DuplicateObjectAdvanceFunc(goCurSelect);
                    listCreatedGO.Add(Selection.activeGameObject);
                }

                //ToAdd：选中克隆后的对象
                Selection.objects = listCreatedGO.ToArray();
            }
        }

        /// <summary>
        /// 复制物体并按序号命名
        /// </summary>
        [MenuItem(DuplicatePrefix + "1 Times", false, startOrder + 1)]
        static void DuplicateObjectAdvanceOneTimesAndSetOrder()
        {
            DuplicateObjectAdvanceAndSetOrderFunc(1);
        }
        [MenuItem(DuplicatePrefix + "5 Times", false, startOrder + 2)]
        static void DuplicateObjectAdvanceFiveTimesAndSetOrder()
        {
            DuplicateObjectAdvanceAndSetOrderFunc(5);
        }
        [MenuItem(DuplicatePrefix + "10 Times", false, startOrder + 3)]
        static void DuplicateObjectAdvanceTenTimesAndSetOrder()
        {
            DuplicateObjectAdvanceAndSetOrderFunc(10);
        }
        static void DuplicateObjectAdvanceAndSetOrderFunc(int totalCount)
        {
            List<GameObject> listGOCreated = new List<GameObject>();
            if (Selection.gameObjects.Length > 0)
            {
                GameObject goCurSelect = Selection.gameObjects[0];

                //尝试获取文件末尾的序号
                string goOriginName = goCurSelect.name;

                int? indexInName = StringTool.GetIndex(goOriginName);
                int startNum = indexInName != null ? indexInName.Value : 0;
                string goNameWithoutNum = goOriginName.Remove(startNum.ToString());

                //从大到小生成，避免Hierarchy排序错乱
                for (int i = totalCount - 1; i != -1; i--)
                {
                    GameObject goDup = DuplicateObjectAdvanceFunc(goCurSelect);

                    if (goDup)
                    {
                        goDup.name = goNameWithoutNum + (startNum + i + 1).ToString();
                    }
                    listGOCreated.Add(goDup);
                }

                Selection.objects = listGOCreated.ToArray();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="goCurSelect"></param>
        /// <returns>克隆后的对象</returns>
        static GameObject DuplicateObjectAdvanceFunc(GameObject goCurSelect)
        {
            //bool isInit = false;
            bool isSameName = true;

            //【该方法有效但是容易误触，改为手动调用】Timeline(有自定义的复制逻辑，与下面不通用（主要是复制Timeline资源并保持绑定）)
            PlayableDirector playableDirector = goCurSelect.GetComponent<PlayableDirector>();
            if (playableDirector)
            {
                DuplicatePlayableDirectorWithBindings();
                return null;
            }

            //复制并修改
            Unsupported.DuplicateGameObjectsUsingPasteboard();
            GameObject goDuplicate = Selection.activeGameObject;
            goDuplicate.transform.SetSiblingIndex(goCurSelect.transform.GetSiblingIndex() + 1);//排在当前选中物体的后面

            //SimpleWaypoint
            SimpleWaypoint simpleWayPoint = goCurSelect.GetComponent<SimpleWaypoint>();
            if (simpleWayPoint)
            {
                SimpleWaypoint nextWayPoint = goDuplicate.GetComponent<SimpleWaypoint>();
                simpleWayPoint.nextWayPoint = nextWayPoint;
                //isInit = true;
                isSameName = false;//由SimpleWaypointGroup决定名称
            }

            //WayPoint
            WayPoint wayPoint = goCurSelect.GetComponent<WayPoint>();
            if (wayPoint)
            {
                WayPoint nextWayPoint = goDuplicate.GetComponent<WayPoint>();
                goDuplicate.name = WayPoint.defaultName;

                nextWayPoint.listNextWayPoint.Clear();
                if (wayPoint.listNextWayPoint.Count > 0 && wayPoint.listNextWayPoint[0] != null)//避免空引用
                {
                    nextWayPoint.listNextWayPoint.AddRange(wayPoint.listNextWayPoint);
                }
                wayPoint.listNextWayPoint.Clear();
                wayPoint.listNextWayPoint.Add(nextWayPoint);

                if (nextWayPoint.GetComponent<RemoteDestinationPoint>())
                {
                    nextWayPoint.GetComponent<RemoteDestinationPoint>().isTeleportOnGameStart = false;
                }
                //isInit = true;
            }

            UITips uITips = goCurSelect.GetComponent<UITips>();
            if (uITips)
            {
                UITips nextUITips = goDuplicate.GetComponent<UITips>();
                nextUITips.name = UITips.defaultName;
                //nextUITips.tipsInfo = null;
                //isInit = true;
            }

            LineRendererHelper lineRendererHelper = goCurSelect.GetComponent<LineRendererHelper>();//Cur
            if (lineRendererHelper)
            {
                LineRendererHelper nextLineRendererHelper = goDuplicate.GetComponent<LineRendererHelper>();

                nextLineRendererHelper.name = LineRendererHelper.defaultName;
                nextLineRendererHelper.Clear();
                lineRendererHelper.TargetPoint = nextLineRendererHelper.transform;
                //isInit = true;
            }

            if (isSameName)
                goDuplicate.name = goCurSelect.name;//同名

            return goDuplicate;
        }

        #endregion

        #region ReName

        const string RenamePrefix = EditorDefinition.HierarchyMenuItemPrefix + "Rename/";

        [MenuItem(RenamePrefix + "Remove Index", false, startOrder)]
        static void RenameObjectAndRemoveIndex()
        {
            for (int i = 0; i != Selection.gameObjects.Length; i++)
            {
                GameObject goCur = Selection.gameObjects[i];
                goCur.name = StringTool.RemoveStringIndex(goCur.name);
            }
        }

        /// <summary>
        /// 基于物体在层级中的序号，进行命名
        /// </summary>
        ///         
        [MenuItem(RenamePrefix + "By SiblingIndex from 0", false, startOrder)]
        static void RenameObjectAndSetOrderFromZero()
        {
            RenameObjectBySiblingIndexFunc(0);
        }
        [MenuItem(RenamePrefix + "By SiblingIndex from 1", false, startOrder)]
        static void RenameObjectAndSetOrderFromOne()
        {
            RenameObjectBySiblingIndexFunc(1);
        }

        static void RenameObjectBySiblingIndexFunc(int startIndex)
        {
            for (int i = 0; i != Selection.gameObjects.Length; i++)
            {
                GameObject goCur = Selection.gameObjects[i];
                goCur.name = StringTool.RemoveStringIndex(goCur.name);
                int indexInHier = goCur.transform.GetSiblingIndex();
                goCur.name += (startIndex + indexInHier).ToString();
            }
        }
        #endregion

        #region ReflectionProbe

        const string ReflectionProbePrefix = EditorDefinition.HierarchyMenuItemPrefix + "ReflectionProbe/";
        /// <summary>
        /// 将选中ReflectionProbe组件烘培后的反射图设置为自定义图，便于二次利用
        /// </summary>
        [MenuItem(ReflectionProbePrefix + "SetReflectionProbeToCustom")]
        static void SetReflectionProbeToCustom()
        {
            foreach (var g in Selection.gameObjects)
            {
                ReflectionProbe reflectionProbe = g.GetComponent<ReflectionProbe>();
                if (reflectionProbe)
                {
                    Undo.RegisterFullObjectHierarchyUndo(g, "SetReflectionProbeToCustom " + g.name);
                    reflectionProbe.customBakedTexture = reflectionProbe.bakedTexture;
                    reflectionProbe.mode = UnityEngine.Rendering.ReflectionProbeMode.Custom;
                }
            }
        }

        #endregion

        #region CrossSceneCopyer

        /// <summary>
        /// 将该场景（通常是用于提前制作素材）同名物体的参数复制到其他场景中
        /// </summary>
        [MenuItem(EditorDefinition.HierarchyMenuItemPrefix + "CrossSceneCopyer/" + "复制材质到其他场景 %#u", false, 0)]
        public static void CopyToOtherScene()
        {
            CopyToOtherSceneFunc(true);
        }

        [MenuItem(EditorDefinition.HierarchyMenuItemPrefix + "CrossSceneCopyer/" + "复制材质到本场景 %#i")]
        static void CopyToThisScene()
        {
            CopyToOtherSceneFunc(false);
        }


        public static void CopyToOtherSceneFunc(bool isToOtherScene)
        {
            //This Scene
            var listGoSelect = Selection.gameObjects.ToList();
            if (listGoSelect.Count == 0)
            {
                Debug.LogError("未选中任何物体！");
                return;
            }


            List<Scene> lisctActiveOtherScene = new List<Scene>();
            for (int i = 0; i != SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded)
                {
                    var rootGOs = scene.GetRootGameObjects();
                    var firstGoRootThisScene = listGoSelect.First().transform.root;
                    if (rootGOs.Contains(firstGoRootThisScene.gameObject))
                    {
                        //Debug.LogError("当前选中物体所在场景：" + scene.name);
                    }
                    else
                        lisctActiveOtherScene.Add(scene);
                }
            }

            foreach (var scene in lisctActiveOtherScene)
            {
                var otherSceneRootGOs = scene.GetRootGameObjects();
                foreach (var goThisSelect in listGoSelect)
                {
                    foreach (var otherRoot in otherSceneRootGOs)
                    {
                        var targetObj = otherRoot.transform.FindFirstChild(goThisSelect.name, true, true);
                        if (targetObj)
                        {
                            var targetMeshRenderer = targetObj.GetComponent<MeshRenderer>();
                            var thisMeshRenderer = goThisSelect.GetComponent<MeshRenderer>();
                            if (targetMeshRenderer)
                            {

                                if (isToOtherScene)
                                {
                                    Debug.Log("Copy" + thisMeshRenderer.sharedMaterial + "ToOtherScene");
                                    Undo.RegisterFullObjectHierarchyUndo(targetObj, "CopyToOtherScene " + targetObj.name);
                                    targetMeshRenderer.sharedMaterial = thisMeshRenderer.sharedMaterial;
                                }
                                else
                                {
                                    Debug.Log("Copy" + targetMeshRenderer.sharedMaterial + "ToOtherScene");
                                    Undo.RegisterFullObjectHierarchyUndo(goThisSelect, "CopyToThisScene " + goThisSelect.name);
                                    thisMeshRenderer.sharedMaterial = targetMeshRenderer.sharedMaterial;

                                }
                            }

                        }
                    }
                }
            }
        }

        #endregion

    }
}
#endif
