using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using Threeyes.Cache;
using Threeyes.External;
using Threeyes.Data;
#if USE_NaughtyAttributes
using NaughtyAttributes;
#endif

namespace Threeyes.Persistent
{

    /// <summary>
    /// Load Files from extern storage and convert to Asset type
    /// Value represent external asset's relate/absolute path (eg: D:/A.png or ../A.png)
    /// 
    /// PS：
    /// 1.DefaultValue should be DefaultAsset's filename with extension, so that when the Controller invoke SaveDefaultValue, the default persistent file will contain the correct external asset's relate path.（SubClass can set the field on Reset, see PersistentData_FileImage for more detail)
    /// 2.路径格式要求：使用/分割
    /// 
    /// Warning：
    /// 1.子类如果需要与UI等进行绑定，可以增加相应的helper，而不是直接在子类中实现
    /// </summary>
    /// <typeparam name="TAsset">Asset Type</typeparam>
    /// <typeparam name="TAssetEvent"></typeparam>
    /// <typeparam name="TOption"></typeparam>
    public abstract class PersistentData_FileBase<TAsset, TAssetEvent, TOption> : PersistentDataBase<string, StringEvent, TOption>, IPersistentData_File<TAsset>
    where TAsset : Object
    where TOption : DataOption_File<TAsset>
    where TAssetEvent : UnityEvent<TAsset>
    {
        public override bool IsValid { get { return DefaultAsset && base.IsValid; } }
        public virtual TAsset DefaultAsset { get { return defaultAsset; } set { defaultAsset = value; } }
        [SerializeField] protected TAsset defaultAsset = default(TAsset);//默认的内置资源
        public UnityEvent<TAsset> EventOnAssetChanged { get { return onAssetChanged; } }//PS:因为携程的关系，可能会比onValueChanged的调用延后

        [SerializeField] protected TAssetEvent onAssetChanged;//对应加载的资源，便于外部调用其他组件        

        public TAsset PersistentAsset { get { return persistentAsset; } set { persistentAsset = value; } }
#if USE_NaughtyAttributes
        [AllowNesting]
        [ReadOnly]
#endif
        [SerializeField] protected TAsset persistentAsset;

        //Config
        public virtual bool UseCache { get { return useCache; } set { useCache = value; } }//Cache Asset
        public bool IsLoadAssetOnValueChanged { get { return isLoadAssetOnValueChanged; } set { isLoadAssetOnValueChanged = value; } }
        [SerializeField] protected bool useCache = false;//PS：默认不使用Cache，否则容易加载到缓存数据（如果只在开始时加载，或多次加载统一资源，则可使用cache）
        [SerializeField] protected bool isLoadAssetOnValueChanged = true;//Load Asset when value changed

        public override void OnValueChanged(string value, PersistentChangeState persistentChangeState)
        {
            base.OnValueChanged(value, persistentChangeState);
            if (IsLoadAssetOnValueChanged)
                LoadAsset(value);
        }

        #region Load from extern
        public virtual FilePathModifier FilePathModifier { get { if (filePathModifier_PD == null) filePathModifier_PD = new FilePathModifier_PD(this); return filePathModifier_PD; } set { Debug.LogError("This property can't set!"); /*暂时不允许设置，避免用户魔改*/} }
        private FilePathModifier_PD filePathModifier_PD;

        public event UnityAction<ExternalResources.LoadResult, object> AssetChanged;//Param:(loadAssetResult, DefaultAsset)
        public ExternalResources.LoadResult LoadResult { get { return loadResult; } }
        ExternalResources.LoadResult<TAsset> loadResult = new ExternalResources.LoadResult<TAsset>();

        protected ObjectCache<string, TAsset> ObjectCache
        {
            get
            {
                if (objectCache == null)
                    objectCache = new ObjectCache<string, TAsset>(LoadAssetFromFile);
                return objectCache;
            }
        }
        static ObjectCache<string, TAsset> objectCache;//Use static to cache this type gereric instance
        public override void Dispose()
        {
            ///PS：
            ///1.如果加载的Asset不属于Unity管理，那就需要在ObjectCache.OnDestroy中传入调用其Dispose的方法
            ///2.不能调用ObjectCache属性，否则会重新创建一个新的实例
            objectCache?.Dispose();
        }


        protected virtual TAsset LoadAssetFromFile(string filePath)
        {
            if (filePath.NotNullOrEmpty())//只有路径非空时，才尝试加载文件
            {
                //ToUse: Option.ReadFileOption参数，支持异步调用（非必须，先占坑）
                loadResult = ExternalResources.LoadEx<TAsset>(filePath, DataOption.DecodeOption);
                return loadResult.value;
            }
            return null;
        }

        /// <summary>
        /// 尝试加载资源，如果加载失败则使用DefaultAsset代替
        /// 
        /// PS：以下方法与Controller的读写方式无关，所以放这里
        /// </summary>
        /// <param name="filePath"></param>
        protected void LoadAsset(string value)
        {
            loadResult = new ExternalResources.LoadResult<TAsset>();//Reset（为了避免Cache导致未更新）
            TAsset asset = null;
            if (value.NotNullOrEmpty())
            {
                //从缓存或本地读取资源
                ///PS：
                ///1.Cache如果读取为null，则不算入缓存，而是重复调用，直到加载成功，便于每次都获得对应的errorInfo
                ///2.暂不支持http图片加载，等后期使用异步加载时一起实现
                ///3.Cache的用途：UI重用同一个素材，可以避免重复加载
                ///ToFix：使用Cache时，读取相对同名文件会导致不更新。
                ///——解决办法：针对非通用素材不使用Cache；或要同时比较路径及file bytes的总长度，两者都不同才使用cache（可以尝试使用MultiKeyDictionary）
                asset = ObjectCache.Get(FilePathModifier.GetAbsPath(value), UseCache);
                if (asset)
                {
                    //PS：因为使用了Cache后，ExternalResources.LoadEx只会调用一次，因此需要统一在这里更新loadResult，便于RuntimeEdit更新
                    loadResult.value = asset;
                }
            }

            if (!asset)//Fallback: 如果读取外部资源失败，就使用默认的资源
                asset = DefaultAsset;

            if (loadResult.errorInfo.NotNullOrEmpty())
                Debug.LogError(loadResult.errorInfo);

            persistentAsset = asset;
            AssetChanged.TryExecute(loadResult, DefaultAsset);//为了避免绑定方法被销毁（如UI被销毁），需要catch
            NotifyAssetChanged(asset);
        }
        #endregion

        protected virtual void NotifyAssetChanged(TAsset asset)
        {
            onAssetChanged.Execute(asset);
        }

        public override void OnDefaultValueSaved()
        {
            base.OnDefaultValueSaved();
            SaveDefaultAssetFunc();
        }

        protected virtual void SaveDefaultAssetFunc()
        {
            //【Editor Only】SaveDefaultValue时，需要同时将DefaultAssets保存到本地中，便于修改
#if UNITY_EDITOR
            if (DefaultAsset)
            {
                try
                {
                    //PS：将项目内部的Asset拷贝到指定目录中
                    string assetAbsPath = Editor.EditorPathTool.GetAssetAbsPath(DefaultAsset);
                    if (assetAbsPath.NotNullOrEmpty())
                    {
                        string destFilePath = null;
                        if (DefaultValue.NotNullOrEmpty())//使用Default配置的路径
                        {
                            destFilePath = FilePathModifier.GetAbsPath(DefaultValue);
                        }
                        else
                        {
                            destFilePath = Path.Combine(PersistentDirPath, Key + assetAbsPath.GetFileExtension());//文件名使用Key，后缀不变
                        }
                        PathTool.GetOrCreateFileParentDir(destFilePath);
                        File.Copy(assetAbsPath, destFilePath, true);
                        UnityEditor.AssetDatabase.Refresh();
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("SaveDefaultAsset failed:\r\n" + e);
                }
            }
#endif
        }

        #region Editor
#if UNITY_EDITOR
        //——Inspector GUI——
        public override void SetInspectorGUIHelpBox_Error(StringBuilder sB)
        {
            base.SetInspectorGUIHelpBox_Error(sB);
            if (!DefaultAsset)//(Warning:这里不能用DefaultAsset==null代替，因为Object重载了bool operator （https://forum.unity.com/threads/null-null.327473/）
            {
                sB.Append("DefaultAsset can't be null!");
                sB.Append("\r\n");
            }
        }

#endif
        #endregion
    }
}