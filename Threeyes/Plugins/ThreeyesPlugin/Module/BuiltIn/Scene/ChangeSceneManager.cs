using Threeyes.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Threeyes.BuiltIn
{
    public class ChangeSceneManager : MonoBehaviour
    {
        public static ChangeSceneManager Instance;
        public KeyCode keyCodeLoadPrevious = KeyCode.None;
        public KeyCode keyCodeLoadNext = KeyCode.None;
        public StringEvent onSceneNameChanged = new StringEvent();

        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
                Scene curScene = SceneManager.GetActiveScene();
                SetSceneName(curScene);
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Update()
        {
            if (keyCodeLoadPrevious != KeyCode.None && UnityEngine.Input.GetKeyDown(keyCodeLoadPrevious))
            {
                LoadPrevious();
            }
            if (keyCodeLoadNext != KeyCode.None && UnityEngine.Input.GetKeyDown(keyCodeLoadNext))
            {
                LoadNext();
            }
        }

        public void LoadPrevious()
        {
            LoadScene(-1);
        }
        public void LoadNext()
        {
            LoadScene(1);
        }

        float lastChangeTime = 0;
        private void LoadScene(int deltaScene)
        {
            if (deltaScene != 0)
            {
                if (Time.time - lastChangeTime < 0.3f)
                    return;

                deltaScene = (int)Mathf.Sign(deltaScene);
                Scene curScene = SceneManager.GetActiveScene();
                int nextSceneID = curScene.buildIndex + deltaScene;
                if (nextSceneID < 0)
                    nextSceneID += SceneManager.sceneCountInBuildSettings;
                else if (nextSceneID >= SceneManager.sceneCountInBuildSettings)
                    nextSceneID -= SceneManager.sceneCountInBuildSettings;
                Scene nextScene = SceneManager.LoadScene(nextSceneID, new LoadSceneParameters(LoadSceneMode.Single));
                SetSceneName(nextScene);
                lastChangeTime = Time.time;
            }
        }

        void SetSceneName(Scene scene)
        {
            onSceneNameChanged.Invoke(scene.name);
        }
    }
}