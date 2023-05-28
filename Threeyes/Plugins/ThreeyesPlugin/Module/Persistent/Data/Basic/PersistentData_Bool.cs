
namespace Threeyes.Persistent
{
    public class PersistentData_Bool : PersistentDataBase<bool, BoolEvent>
    {
#if UNITY_EDITOR

        //——MenuItem——
        static string instName = "BoolPD ";
        [UnityEditor.MenuItem(strMenuItem_Root_Basic + "Bool", false, intBasicMenuOrder + 0)]
        public static void CreateInst()
        {
            Editor.EditorTool.CreateGameObjectAsChild<PersistentData_Bool>(instName);
        }

        //——Hierarchy GUI——
        public override string ShortTypeName { get { return "Bool"; } }

#endif
    }
}