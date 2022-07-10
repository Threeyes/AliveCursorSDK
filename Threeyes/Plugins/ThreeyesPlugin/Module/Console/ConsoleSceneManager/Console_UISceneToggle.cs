using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ConsoleScene
{
    public class Console_UISceneToggle : ElementBase<SOSceneInfo>
    {
        public RawImage imageScene;
        public Text texName;
        public Toggle tog;
        public Image imaLoadingProgress;
        public Text textProgress;
        public Image imaOutline;

        public override void InitFunc(SOSceneInfo data)
        {
            base.InitFunc(data);
            imageScene.texture = data.icon;
            texName.text = data.displayName;

            imaLoadingProgress.enabled = false;
            textProgress.enabled = false;
            ToggleGroup toggleGroup = transform.FindFirstComponentInParent<ToggleGroup>();
            if (toggleGroup)
                tog.group = toggleGroup;


            var consoleSceneConfig = SOConsoleSceneManager.Instance.CurConsoleSceneConfig;
            if (consoleSceneConfig)
            {
                //标记为上次运行的场景
                if (consoleSceneConfig.sceneInfoLastLoaded == data)
                {
                    imaOutline.color *= 0.7f;
                }
            }
        }

        public void SetToggle(bool isOn)
        {
            tog.isOn = isOn;
        }

        [ContextMenu("LoadScene")]
        public void LoadScene()
        {
            if (!Console_UISceneGroup.Instance)
                return;

            if (Console_UISceneGroup.Instance.IsLoadingScene)
                return;

            Console_UISceneGroup.Instance.IsLoadingScene = true;
            Console_UISceneGroup.Instance.SetInteractable(false);//禁用UI交互
            StartCoroutine(IEBeginLoad());
        }

        IEnumerator IEBeginLoad()
        {
            imaLoadingProgress.enabled = true;
            textProgress.enabled = true;
            yield return new WaitForSeconds(1f);
            data.LoadSceneAsync(UnityEngine.SceneManagement.LoadSceneMode.Single, OnProgress);
        }

        bool isLoadCompleted = false;
        void OnProgress(float percent)
        {
            imaLoadingProgress.fillAmount = percent;
            textProgress.text = string.Format("{0:0}", percent * 100) + "%";

            if (Console_UISceneGroup.Instance)
                Console_UISceneGroup.Instance.OnLoadProgress(percent);

            //PS：因为安卓不能在加载完成后等待，所以只能提前渐隐
            if (!isLoadCompleted && percent >= 0.9f)
            {
                isLoadCompleted = true;
                OnLoadNearCompleted();
            }
        }

        static void OnLoadNearCompleted()
        {
            //调用FadeIn
            ConsoleSceneFadeManager.Fade(false);
            //if (Console_UISceneGroup.Instance)
            //    Console_UISceneGroup.Instance.OnLoadNearCompleted();
        }

        public void DeActive()
        {
            tog.interactable = false;
        }
    }

}
