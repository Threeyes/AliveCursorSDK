using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System;
using UnityEngine.Events;

namespace Threeyes.IO
{
    /// <summary>
    /// Support read files from both FileSystem and Web, in Synchronous/ Asynchronous way
    /// 
    /// ToUpdate：内部方法通过System.Uri统一处理路径，该类有判断是否为本地路径等的方法
    /// 
    ///参考：（UnifiedIO）https://assetstore.unity.com/packages/tools/input-management/unifiedio-cross-platform-io-26461#reviews (SDK命名规范按照File的方式。文档：看官网)

    /// </summary>
    public static class FileIO
    {
        //ToAdd:Write

        #region Common Enter 

        public static void ReadAllBytesEx(string filePathOrUrl, UnityAction<ReadFileResult> actResult, ReadFileOption option = null)
        {
            if (option == null)
                option = ReadFileOption.Default;

            if (option.isAsync)
                ReadAllBytesExAsync(filePathOrUrl, actResult);
            else
                actResult.Execute(ReadAllBytesEx(filePathOrUrl));
        }
        static async void ReadAllBytesExAsync(string filePathOrUrl, UnityAction<ReadFileResult> actResult)
        {
            var result = await ReadAllBytesExAsync(filePathOrUrl);
            actResult.Execute(result);
        }


        public static byte[] ReadAllBytes(string filePathOrUrl)
        {
            return ReadAllBytesEx(filePathOrUrl).value;
        }
        public static ReadFileResult ReadAllBytesEx(string filePathOrUrl)
        {
            if (IsAndroidStreamingAssetsPath(filePathOrUrl) || IsUriPath(filePathOrUrl))
            {
                return ReadAllBytesFromWebEx(filePathOrUrl);
            }
            else
            {
                return ReadAllBytesFromFileSystemEx(filePathOrUrl);
            }
        }

        public static async Task<ReadFileResult> ReadAllBytesExAsync(string filePathOrUrl)
        {
            return await (
                IsAndroidStreamingAssetsPath(filePathOrUrl) || IsUriPath(filePathOrUrl) ?
                    ReadAllBytesFromWebExAsync(filePathOrUrl) :
                   ReadAllBytesFromFileSystemExAsync(filePathOrUrl)
               );
        }

        #endregion


        #region Inner

        public static ReadFileResult ReadAllBytesFromFileSystemEx(string filePath)
        {
            ReadFileResult fileIOResult = new ReadFileResult();
            if (File.Exists(filePath))
            {
                try
                {
                    fileIOResult.value = File.ReadAllBytes(filePath);
                }
                catch (Exception e)
                {
                    string errorLog = $"ReadAllBytes error in [{filePath}]: " + e;
                    Debug.LogError(errorLog);
                    fileIOResult.errorInfo = errorLog;
                }
            }
            else
            {
                fileIOResult.errorInfo = $"File not exist in {filePath}!";
            }
            return fileIOResult;
        }
        public static async Task<ReadFileResult> ReadAllBytesFromFileSystemExAsync(string filePath)
        {
            ReadFileResult fileIOResult = new ReadFileResult();
            if (File.Exists(filePath))
            {
                FileInfo fileInfo = new FileInfo(filePath);
                using (FileStream sR = File.Open(fileInfo.FullName, FileMode.Open))
                {
                    fileIOResult.value = new byte[sR.Length];
                    await sR.ReadAsync(fileIOResult.value, 0, (int)sR.Length);
                }
            }
            else
            {
                //PS:不算错误，不需要调用LogError
                fileIOResult.errorInfo = $"File not exist in {filePath}!";
            }
            return fileIOResult;
        }

        //Bug:暂未实现功能
        public static ReadFileResult ReadAllBytesFromWebEx(string filePathOrUrl)
        {
            ReadFileResult fileIOResult = new ReadFileResult();
            string url = AddProtocol(filePathOrUrl);
            using (UnityWebRequest web = UnityWebRequest.Get(url))
            {
                try
                {
                    var request = web.SendWebRequest();
                    while (!request.isDone)
                    {
                        //Do nothing
                    }
                    fileIOResult.value = web.downloadHandler.data;
                }
                catch (OperationCanceledException)
                {
                    web.Abort();
                }
                catch (Exception e)
                {
                    string errorLog = $"ReadAllBytesByWeb error in [{url}]: " + e;
                    Debug.LogError(errorLog);
                    fileIOResult.errorInfo += errorLog;
                }
                finally { }
            }
            return fileIOResult;
        }


        public static async Task<ReadFileResult> ReadAllBytesFromWebExAsync(string filePath)
        {
            ReadFileResult fileIOResult = new ReadFileResult();
            if (!IsUriPath(filePath))
                filePath = AddProtocol(filePath);
            string url = filePath;
            using (UnityWebRequest web = UnityWebRequest.Get(url))
            {
                try
                {
                    var request = web.SendWebRequest();
                    while (!request.isDone)
                    {
                        //PS：
                        await Task.Yield();
                    }
                    fileIOResult.value = web.downloadHandler.data;
                }
                catch (OperationCanceledException)
                {
                    web.Abort();
                }
                catch (Exception e)
                {
                    string errorLog = $"ReadAllBytesByWebAsync error in [{url}]: " + e;
                    Debug.LogError(errorLog);
                    fileIOResult.errorInfo = errorLog;
                }
                finally { }
            }
            return fileIOResult;
        }



        public static bool Exists(string filePath)
        {
            ///PS:
            ///1.不推荐使用Web读取本地文件/ it is not recommended to use UnityWebRequest for that. In most cases there are more efficient ways to read local files.（https://forum.unity.com/threads/unitywebrequest-for-local-files-on-android-failure.441655/）
            if (IsAndroidStreamingAssetsPath(filePath))
            {
                //ToUpdate:检查 StreamingAssets 中的资源是否存在，,参考或直接整合BetterStreamingAssets（https://github.com/gwiazdorrr/BetterStreamingAssets）
                return true;
            }
            return File.Exists(filePath);
        }

        public static bool IsUriPath(string filePath)
        {
            //ToUpdate：待使用更加通用的方法，兼容Uri的http/ftp等格式
            //Uri uri = new Uri(filePath);
            //uri.Scheme == Uri.UriSchemeHttp||

            //兼容本地文件以及http/https
            return filePath.StartsWith("file:///") || filePath.StartsWith("http");
        }

        public static bool IsAndroidStreamingAssetsPath(string filePath)
        {
            //return true;//Debug，ToDelete
            if (!Application.isEditor && Application.platform == RuntimePlatform.Android)
            {
                return filePath.StartsWith("jar:file://");
            }
            return false;
        }


        /// <summary>
        /// Add protocol in front of the path for UnityWebRequest
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string AddProtocol(string filePath)
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    // https://docs.unity3d.com/ScriptReference/Application-streamingAssetsPath.html
                    //Android: 
                    //only Application.streamingAssetsPath has protocol "jar:file://", because the folder is compressed into the .apk file.
                    // -> 
                    if (!filePath.StartsWith("jar:file://"))//PS:streamingAssetsPath已经自带jar:file://
                        filePath = "file://" + filePath;
                    break;

                // When using file protocol on Windows and Windows Store Apps for accessing local files, you have to specify file:/// (with three slashes).
                //Ref:https://docs.microsoft.com/en-us/dotnet/api/system.uri?redirectedfrom=MSDN&view=net-6.0
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WSAPlayerARM:
                case RuntimePlatform.WSAPlayerX86:
                case RuntimePlatform.WSAPlayerX64:
                    if (!filePath.StartsWith("file:///"))
                        filePath = "file:///" + filePath; break;

                //the other platform has same protocol "File://" such as IOS
                default:
                    if (!filePath.StartsWith("file://"))
                        filePath = "file://" + filePath; break;
            }
            return filePath;
        }
        #endregion

    }

    //PS：不能放到FileIO中，否则Modder无法访问
    public class ReadFileResult
    {
        public bool HasValue { get { return value != null && value.Length > 0; } }

        public byte[] value = new byte[] { };
        public string errorInfo = null;
    }

    [Serializable]
    public class ReadFileOption
    {
        public static ReadFileOption Default { get { return new ReadFileOption(); } }

        public bool isAsync = false;

        public ReadFileOption()
        {
        }

        public ReadFileOption(bool isAsync)
        {
            this.isAsync = isAsync;
        }
    }
}
