using UnityEngine;

namespace Threeyes.Persistent
{
    public class PersistentData_Vector2 : PersistentDataBase<Vector2, Vector2Event>
    {

#if UNITY_EDITOR

        //！！MenuItem！！
        static string instName = "Vector2PD ";
        [UnityEditor.MenuItem(strMenuItem_Root_Basic + "Vector2", false, intBasicMenuOrder + 4)]
        public static void CreateInst()
        {Editor.EditorTool.CreateGameObjectAsChild<PersistentData_Vector2>(instName);
        }

        //！！Hierarchy GUI！！
        public override string ShortTypeName { get { return "Vector2"; } }

#endif

    }
}