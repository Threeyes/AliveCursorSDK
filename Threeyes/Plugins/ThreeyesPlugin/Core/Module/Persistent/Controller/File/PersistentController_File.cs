using System.IO;
using UnityEngine;
using System.Reflection;
using System;
using System.Collections;
using Threeyes.IO;
using System.Text;
namespace Threeyes.Persistent
{
    /// <summary>
    /// 本地读写PersistentData
    /// 
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class PersistentController_File<TValue> : PersistentControllerBase<TValue, PersistentControllerOption_File>
    {
        public IPersistentConverter PersistentConverter { get { if (persistentConverter == null) persistentConverter = new PersistentConverter_Json(); return persistentConverter; } }
        IPersistentConverter persistentConverter;

        public override bool HasKey { get { return base.HasKey && FileIO.Exists(FilePath); } }
        protected string FilePath { get { return Path.Combine(Option.DirPath, Key) + "." + Option.Extension; } }

        public PersistentController_File(IPersistentData<TValue> persistentData, PersistentControllerOption_File option) : base(persistentData, option) { }

        protected override void InitPersistentData(IPersistentData<TValue> persistentData)
        {
            base.InitPersistentData(persistentData);

            //PS:因为PersistentData是文件，所以可以将其存放的目录作为PersistentDirPath的对应目录
            persistentData.PersistentDirPath = Option.DirPath;
        }

        //ToUpdate:增加Json忽略自定义Attribute的办法（如[PersistentDirPath]）
        protected override TValue GetValueFunc()
        {
            try
            {
                //Deserialization 
                string textContent = Encoding.UTF8.GetString(FileIO.ReadAllBytes(FilePath));
                return PersistentConverter.Deserialize<TValue>(textContent, persistentData.RealValueType);//PS：TValue可能是通用基类(如Enum、SO），通过RealValueType可获得真实的类型
            }
            catch (Exception e)
            {
                Debug.LogError(Key + " JsonGetValueFunc error: " + e);
                HasChanged = true;//读取失败，设置该字段好让后续强制保存默认值
            }
            return DefaultValue;
        }


        protected override void SaveValueFunc(TValue value)
        {
            try
            {
                //Serialization
                string json = PersistentConverter.Serialize(value);
                PathTool.GetOrCreateFileParentDir(FilePath);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception e)
            {
                //PS：可能是文件名过长、文件名非法
                Debug.LogError("Set ValueFunc error for [" + Key + "] : \r\n" + e);
            }
        }

        protected override void DeleteKeyFunc()
        {
            try
            {
                File.Delete(FilePath);
            }
            catch (Exception e)
            {
                Debug.LogError("Set [" + Key + "] ValueFunc error: " + e);
            }
        }

    }

    [System.Serializable]
    public class PersistentControllerOption_File : PersistentControllerOption
    {
        public string DirPath { get { return dirPath; } set { dirPath = value; } }// Dir to save/read config
        [SerializeField] protected string dirPath;

        public string Extension { get { return extension; } set { extension = value; } }
        [SerializeField] protected string extension = "persistent"; //使用自定义后缀，便于遍历查询

        public PersistentControllerOption_File() : base()
        {
            extension = "persistent";
        }
        public PersistentControllerOption_File(string dirPath) : base()
        {
            this.dirPath = dirPath;
            extension = "persistent";
        }

        public PersistentControllerOption_File(string dirPath, string extension = "persistent", bool isSaveOnSet = false, bool isSaveOnDispose = true, bool onlySaveOnChanged = true) : base(isSaveOnSet, isSaveOnDispose, onlySaveOnChanged)
        {
            this.dirPath = dirPath;
            this.extension = extension;
        }
    }
    //Todo：参考Save System，定义ISerializer
    //PS:【非必须】后续需要序列化组件等复杂数据，可以考虑自己写个类似的Controller并使用Bayat - Save System实现（需要自己重写并以字符串原样保存的形式以便向下兼容）（默认的SaveGameJsonSerializer会为string增加转义字符，SaveGameBinarySerializer会增加多余的字符）
}