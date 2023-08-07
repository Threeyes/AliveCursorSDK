using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Threeyes.Steamworks
{
    public class SceneManagerSimulator : HubSceneManagerBase<SceneManagerSimulator>
    {
        protected virtual void Start()
        {
            InitAsync();
        }

        async void InitAsync()
        {
            await Task.Yield();//等待Config初始化完成

            //找到ModScene
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

            //调用初始化代码
            ModEntry modEntry = curModScene.GetComponents<ModEntry>().FirstOrDefault();
            if (!modEntry)
            {
                Debug.LogError($"Can't find {nameof(ModEntry)} in Mod Scene!");
                return;
            }
            InitMod(modEntry);
        }
    }
}