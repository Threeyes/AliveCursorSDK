using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Threeyes.External;
using System;
using Threeyes.Data;
using Threeyes.IO;
using UnityEngine.Events;
#if USE_NaughtyAttributes
using NaughtyAttributes;
#endif

namespace Threeyes.Persistent.Test
{
    [DefaultExecutionOrder(-30000)]//Make sure listen Actions in Awake run before PD get Loaded
    public class TestPersistent : TestPersistentBase<SOTestPersistent, TestPersistent.ConfigInfo>
    {
        public RawImage rawImage;
        public MeshRenderer meshRenderer;

        private void Awake()
        {
            //Todo:应等待监听完成后再Load
            Log("TestPersistent Listen UnityAction");
            Config.actionPersistentChanged += OnPersistentChanged;
            Config.actionTextureChanged += OnTextureChanged;
            Config.actionMaterialOptionChanged += OnMaterialOptionChanged;

            LoadUrlImage("https://lmg.jj20.com/up/allimg/tx01/0707201947657.jpg");
        }


        private void Start()
        {
            //测试Collection是否为null
            Log("arrString Length: " + Config.arrString.Length);
            Log("listString Count: " + Config.listString.Count);
        }

        async void LoadUrlImage(string url)
        {
            var result = await FileIO.ReadAllBytesFromWebExAsync(url);
        }
        private void OnTextureChanged(Texture texture)
        {
            rawImage.texture = texture;
            Log("OnTextureChanged");
        }
        private void OnMaterialOptionChanged()
        {
            meshRenderer.material = Config.curOptionMaterial;
            Log("OnMaterialOptionChanged");
        }

        private void OnPersistentChanged()
        {
            Log("OnPersistentChanged");
        }

        [System.Serializable]
        [PersistentChanged(nameof(ConfigInfo.OnPersistentChanged))]
        public class ConfigInfo
        {
            //Test NaughtyAttributes
            public bool isEnableFollowing = true;
            [EnableIf(nameof(isEnableFollowing))] public DataOption_OptionInfo optionInfo;

            //Test IList
            public List<ScriptableObject> listSO = new List<ScriptableObject>();
            public ScriptableObject[] arrSO = new ScriptableObject[] { };
            public List<string> listString = new List<string>();
            public string[] arrString = new string[] { };


            public UnityAction actionPersistentChanged;
            public UnityAction<Texture> actionTextureChanged;
            public UnityAction actionMaterialOptionChanged;

            [JsonIgnore] public Texture defaultTextureAsset;
            [JsonIgnore] [PersistentAssetChanged(nameof(OnTextureAssetChanged))] public Texture textureAsset;
            [JsonIgnore] public SOBytesAsset sOBytesAsset;
            [JsonIgnore] public TextAsset textAsset;
            [JsonIgnore] public List<Material> listOptionMaterial = new List<Material>();
            [JsonIgnore] public Material curOptionMaterial;

            [JsonIgnore] /*[HideInInspector]*/ [PersistentDirPath] public string PersistentDirPath;
            [PersistentAssetFilePath(nameof(textureAsset), true, nameof(TextureDataOption), nameof(OnTextureAssetLoad), nameof(defaultTextureAsset))] public string textureAssetPath;
            [PersistentAssetFilePath(nameof(sOBytesAsset), true, assetLoadedCallbackMethodName: nameof(OnBytesAssetLoad))] public string bytesAssetPath;
            [PersistentAssetFilePath(nameof(textAsset), true, assetLoadedCallbackMethodName: nameof(OnTextAssetLoad))] public string textAssetPath;
            DataOption_File TextureDataOption { get { return new DataOption_TextureFile() { OverrideFileFilterExtensions = new string[] { "*" } }; } }
            [PersistentOption(nameof(listOptionMaterial), nameof(curOptionMaterial))] [PersistentValueChanged(nameof(OnOptionMaterialChanged))] public int curOptionMaterialIndex;

            #region Callback

            /// <summary>
            /// Get invoked after asset field changed(PS:可以用PersistentValueChanged代替）
            /// </summary>
            /// <param name="texture"></param>
            /// <param name="persistentChangeState"></param>
            private void OnTextureAssetChanged(Texture texture, PersistentChangeState persistentChangeState)
            {
                Log("OnTextureAssetChanged");
                actionTextureChanged.Execute(texture);
            }

            void OnPersistentChanged(PersistentChangeState persistentChangeState)
            {
                Log("OnPersistentChanged");
                actionPersistentChanged.Execute();
            }
            Texture OnTextureAssetLoad(ExternalResources.LoadResult<Texture> loadResult, PersistentChangeState persistentChangeState)
            {
                ///Warning:
                ///1. The loadRsult.value can be null
                ///2. 因为此时还未给对应的Asset字段赋值，如果此时获取Config.textureAsset，得到的是旧值
                ///3. 该回调方法主要是修改读取后的值（如：如果读取值为null，则使用默认图）
                Log("OnTextureAssetLoad: " + loadResult.value?.width);
                return loadResult.value;
            }

            SOBytesAsset OnBytesAssetLoad(ExternalResources.LoadResult<SOBytesAsset> loadResult, PersistentChangeState persistentChangeState)
            {
                Log("OnBytesAssetLoad: " + loadResult.value?.ToString());
                return loadResult.value;
            }

            TextAsset OnTextAssetLoad(ExternalResources.LoadResult<TextAsset> loadResult, PersistentChangeState persistentChangeState)
            {
                Log("OnTextAssetLoad: " + loadResult.value?.ToString());
                return loadResult.value;
            }

            void OnOptionMaterialChanged(int oldValue, int newValue, PersistentChangeState persistentChangeState)
            {
                actionMaterialOptionChanged.Execute();
            }

            #endregion
        }

        #region Utility

        public static void Log(string content)
        {
            Debug.Log($"<color=cyan>{content}</color>");
        }


        #endregion
    }


    public class TestPersistentBase<TSOConfig, TConfig> : MonoBehaviour
      where TSOConfig : SOTestPersistentBase<TConfig>
    {
        public TConfig Config { get { return SOOverrideConfig ? SOOverrideConfig.config : DefaultConfig; } }
        public TConfig DefaultConfig { get { return defaultConfig; } set { defaultConfig = value; } }
        public TSOConfig SOOverrideConfig { get { return soOverrideConfig; } set { soOverrideConfig = value; } }
        [Header("Config")]
        [SerializeField] protected TConfig defaultConfig;//Default config
#if USE_NaughtyAttributes
        [Expandable]
#endif
        [SerializeField] protected TSOConfig soOverrideConfig;//Override config
    }

}