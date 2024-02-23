using Threeyes.Core;
using Threeyes.Core.Editor;
using UnityEngine;

namespace Threeyes.Persistent
{
    public class PersistentData_Vector3 : PersistentDataBase<Vector3, Vector3Event>
    {
#if UNITY_EDITOR

        //！！MenuItem！！
        static string instName = "Vector3PD ";
        [UnityEditor.MenuItem(strMenuItem_Root_Basic + "Vector3", false, intBasicMenuOrder + 5)]
        public static void CreateInst()
        {
            EditorTool.CreateGameObjectAsChild<PersistentData_Vector3>(instName);
        }

        //！！Hierarchy GUI！！
        public override string ShortTypeName { get { return "Vector3"; } }

#endif

    }
}