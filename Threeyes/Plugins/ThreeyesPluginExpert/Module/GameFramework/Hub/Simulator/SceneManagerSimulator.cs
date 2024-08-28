using System.Linq;
using System.Threading.Tasks;
using Threeyes.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

#if UNITY_EDITOR
using UnityEditor;
using Threeyes.Core.Editor;
#endif
namespace Threeyes.GameFramework
{
    public class SceneManagerSimulator : HubSceneManagerBase<SceneManagerSimulator>
    {
        [SerializeField] protected SOWorkshopItemInfo curSOWorkshopItemInfo;
        [SerializeField] protected WorkshopItemInfo curWorkshopItemInfo;

        protected override void SetInstanceFunc()
        {
            base.SetInstanceFunc();
            isChangingScene = true;//提前标记，避免其他组件提前初始化
        }
        protected virtual void Start()
        {
            InitAsync();
        }

        async void InitAsync()
        {
            await Task.Yield();//等待Config初始化完成
#if UNITY_EDITOR
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
                isChangingScene = false;
                return;
            }
            //#1 扫描获取对应的WorkshopItemInfo（实现方法：从该Scene的路径开始，往上搜索到Items文件夹，就能知道Item的名称，然后通过方法找到对应的WorkshopInfo）
            string sceneFilePath = curModScene.path;// Assets/Items/[Item Name]
            string itemName = sceneFilePath.Replace($"Assets/{GameFramework_PathDefinition.ItemRootDirName}/", "");
            itemName = itemName.Substring(0, itemName.IndexOf("/"));
            string absItemInfoFilePath = SOWorkshopItemInfo.GetItemInfoFilePath(itemName);
            string soDir = EditorPathTool.AbsToUnityRelatePath(absItemInfoFilePath);
            curSOWorkshopItemInfo = AssetDatabase.LoadAssetAtPath<SOWorkshopItemInfo>(soDir);
            if (curSOWorkshopItemInfo == null)
            {
                Debug.LogError($"Can't find {nameof(SOWorkshopItemInfo)} for item {itemName}!");
                isChangingScene = false;
                return;
            }
            curWorkshopItemInfo = curSOWorkshopItemInfo.BaseItemInfo;

            //#2 调用初始化代码
            ModEntry modEntry = curModScene.GetComponents<ModEntry>().FirstOrDefault();
            if (!modEntry)
            {
                Debug.LogError($"Can't find {nameof(ModEntry)} in Mod Scene!");
                isChangingScene = false;
                return;
            }

            InitMod(modEntry);
            isChangingScene = false;
#endif 
        }

        protected override void InitMod(ModEntry modEntry)
        {
            try
            {
                InitModFunc(modEntry);
            }
            catch (Exception e)
            {
                Debug.LogError($"Init Mod [{curWorkshopItemInfo.title}] with error: \r\n" + e);//写入到对应Log中
            }
        }

        protected virtual void InitModFunc(ModEntry modEntry)
        {
            //#1 PreInit Scene Scripts
            ManagerHolder.GetListManagerModPreInitOrder().ForEach(m => m.OnModPreInit(curModScene, modEntry));
            EventCommunication.SendMessage<IModPreHandler>((inst) => inst.OnModPreInit());

            //#2 Init Mod
            base.InitMod(modEntry);
        }
    }
}
