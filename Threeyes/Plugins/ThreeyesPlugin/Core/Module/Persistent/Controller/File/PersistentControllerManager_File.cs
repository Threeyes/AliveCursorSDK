using UnityEngine;
namespace Threeyes.Persistent
{
    [RequireComponent(typeof(InstanceManager))]
    [DefaultExecutionOrder(-20000)]
    public class PersistentControllerManager_File : PersistentControllerManagerBase<PersistentControllerFactory_File, PersistentControllerOption_File>
    {
        //PS：
        //1.以下配置用于Runtime生成 ControllerOption.DirPath
        //2.配置不通用，所以不放到ControllerOption导致复杂性增加
        [Header("Controller Config")]
        public ExternalFileLocation externalFileLocation = ExternalFileLocation.CustomData;
        public string subDirPath;//The Abs/Relate dir path for each child Controller (eg:DataBase)

        protected override IPersistentController InitElement(IPersistentData persistentData, PersistentControllerOption_File controllerOption)
        {
            string absDirPath = PathTool.GetPath(externalFileLocation, subDirPath);
            PathTool.GetOrCreateDir(absDirPath);
            controllerOption.DirPath = absDirPath;
            return base.InitElement(persistentData, controllerOption);
        }
    }
}