using Threeyes.Core;
using Threeyes.Core.Editor;

namespace Threeyes.Persistent
{
    public class PersistentData_String : PersistentDataBase<string, StringEvent>
    {
#if UNITY_EDITOR
        //——MenuItem——
        static string instName = "StringPD ";
        [UnityEditor.MenuItem(strMenuItem_Root_Basic + "String", false, intBasicMenuOrder + 3)]
        public static void CreateInst()
        {
            EditorTool.CreateGameObjectAsChild<PersistentData_String>(instName);
        }

        //——Hierarchy GUI——
        public override string ShortTypeName { get { return "String"; } }
#endif
    }
}