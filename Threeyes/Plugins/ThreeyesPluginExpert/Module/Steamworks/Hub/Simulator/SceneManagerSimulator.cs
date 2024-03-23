using System.Linq;
using System.Threading.Tasks;
using Threeyes.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using Threeyes.Core.Editor;
#endif
namespace Threeyes.Steamworks
{
    public class SceneManagerSimulator : HubSceneManagerBase<SceneManagerSimulator>
    {
        public bool HasSceneLoaded { get { return hasSceneLoaded; } }
        bool hasSceneLoaded = false;

        [SerializeField] protected SOWorkshopItemInfo curSOWorkshopItemInfo;
        [SerializeField] protected WorkshopItemInfo curWorkshopItemInfo;
        protected virtual void Start()
        {
            InitAsync();
        }

        async void InitAsync()
        {
#if UNITY_EDITOR
            await Task.Yield();//等待Config初始化完成

            //#0 找到ModScene
            for (int i = 0; i != SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene != hubScene)
                {
                    curModScene = scene;
                    break;
                }
            }
            if (!curModScene.IsValid())
            {
                Debug.LogError("Please add the Mod Scene before play!");
                return;
            }
            //#1 扫描获取对应的WorkshopItemInfo（实现方法：从该Scene的路径开始，往上搜索到Items文件夹，就能知道Item的名称，然后通过方法找到对应的WorkshopInfo）
            string sceneFilePath = curModScene.path;// Assets/Items/[Item Name]
            string itemName = sceneFilePath.Replace($"Assets/{ Steamworks_PathDefinition.ItemRootDirName}/", "");
            itemName = itemName.Substring(0, itemName.IndexOf("/"));
            string absItemInfoFilePath = SOWorkshopItemInfo.GetItemInfoFilePath(itemName);
            string soDir = EditorPathTool.AbsToUnityRelatePath(absItemInfoFilePath);
            curSOWorkshopItemInfo = AssetDatabase.LoadAssetAtPath<SOWorkshopItemInfo>(soDir);
            if (curSOWorkshopItemInfo == null)
            {
                Debug.LogError($"Can't find {nameof(SOWorkshopItemInfo)} for item {itemName}!");
                return;
            }
            curWorkshopItemInfo = curSOWorkshopItemInfo.BaseItemInfo;

            //#2 调用初始化代码
            ModEntry modEntry = curModScene.GetComponents<ModEntry>().FirstOrDefault();
            if (!modEntry)
            {
                Debug.LogError($"Can't find {nameof(ModEntry)} in Mod Scene!");
                return;
            }
            InitMod(modEntry);
            hasSceneLoaded = true;
#endif 
        }
    }
}
