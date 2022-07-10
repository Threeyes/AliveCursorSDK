using UnityEngine;
using Threeyes.IO;
using Threeyes.Decoder;
using System.Reflection;
using UnityEngine.Events;

namespace Threeyes.External
{
    /// <summary>
    /// Load Asset from exernal path
    /// 
    ///功能：
    ///1.能判断文件是否存在，已经自动兼容Web及本地格式加载
    ///2.封装IO及Decoder
    ///
    /// Ref：Resources
    /// </summary>
    public static class ExternalResources
    {
        public static bool CanLoad(System.Type assetType)
        {
            return DecoderManager.CanDecode(assetType);
        }

        //Load Using Callback
        public static void LoadEx<T>(string path, UnityAction<LoadResult<T>> actResult, ReadFileOption readFileOption = null, IDecodeOption decodeOption = null)
    where T : Object
        {
            LoadResult<T> loadResult = new LoadResult<T>();

            System.Type assetType = typeof(T);
            if (!CanLoad(assetType))
            {
                actResult.Execute(loadResult);
                return;
            }

            //#1 Read file->bytes
            UnityAction<ReadFileResult> actResultTemp =
            (fileIOResult) =>
            {
                if (fileIOResult.errorInfo != null)
                    loadResult.errorInfo = fileIOResult.errorInfo;
                if (fileIOResult.HasValue)
                {
                    //#2 Decode bytes->Assets
                    DecodeResult<T> decodeResult = DecoderManager.DecodeEx<T>(fileIOResult.value, decodeOption);
                    loadResult.value = decodeResult.value;
                    if (decodeResult.errorInfo != null)
                        loadResult.errorInfo += decodeResult.errorInfo;
                }
                actResult.Execute(loadResult);
            };
            FileIO.ReadAllBytesEx(path, actResultTemp);
        }

        public static object Load(string path, System.Type assetType, IDecodeOption option = null)
        {
            return LoadEx(path, assetType, option).ObjValue;
        }

        public static T Load<T>(string path, IDecodeOption decodeOption = null) where T : Object
        {
            return LoadEx<T>(path, decodeOption).value;
        }


        public static LoadResult LoadEx(string path, System.Type assetType, IDecodeOption decodeOption = null)
        {
            MethodInfo methodInfo = typeof(ExternalResources).GetMethod(nameof(LoadEx), BindingFlags.Static | BindingFlags.Public, null, new System.Type[] { typeof(string), typeof(IDecodeOption) }, null);
            LoadResult loadResult = methodInfo.MakeGenericMethod(assetType).Invoke(null, new object[] { path, decodeOption }) as LoadResult;
            return loadResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="decodeOption"></param>
        /// <returns>The loadResult will always not null, you can safely access the value field</returns>
        public static LoadResult<T> LoadEx<T>(string path, IDecodeOption decodeOption = null)
            where T : Object
        {
            LoadResult<T> loadResult = new LoadResult<T>();

            System.Type assetType = typeof(T);
            if (!CanLoad(assetType))
                return loadResult;

            //#1 Read file->bytes
            var fileIOResult = new ReadFileResult();
            fileIOResult = FileIO.ReadAllBytesEx(path);
            loadResult.FileBytesCount = fileIOResult.value != null ? fileIOResult.value.Length : 0;

            if (fileIOResult.errorInfo != null)
                loadResult.errorInfo = fileIOResult.errorInfo;
            if (!fileIOResult.HasValue)
                return loadResult;

            //#2 Decode bytes->Assets
            DecodeResult<T> decodeResult = DecoderManager.DecodeEx<T>(fileIOResult.value, decodeOption);
            loadResult.value = decodeResult.value;
            if (decodeResult.errorInfo != null)
                loadResult.errorInfo += decodeResult.errorInfo;

            return loadResult;
        }


        [System.Serializable]
        public class LoadResult
        {
            public virtual bool HasValue { get { return ObjValue != null; } }
            public virtual object ObjValue { get { return null; } set { } }
            //private object objValue = null;//PS:为了兼容性及反射方便，使用object存储真正的值（ToDelete：改为属性，由value负责存储具体值。参考DecodeResultBase）

            public int FileBytesCount { get; set; }//The count of file bytes
            public string errorInfo = null;

            public LoadResult() { }
            public LoadResult(LoadResult other)
            {
                ObjValue = other.ObjValue;
                errorInfo = other.errorInfo;
            }
        }

        public class LoadResult<T> : LoadResult
            where T : Object
        {
            public override object ObjValue { get { return value; } set { this.value = value as T; } }
            public T value;

            public LoadResult() { }
            public LoadResult(LoadResult<T> other)
            {
                ObjValue = other.ObjValue;
                errorInfo = other.errorInfo;
            }

            /// <summary>
            /// Get first valid asset
            /// </summary>
            /// <param name="defaultAsset"></param>
            /// <returns></returns>
            public T GetActiveAsset(T defaultAsset) { return value ? value : defaultAsset; }

        }
    }
}