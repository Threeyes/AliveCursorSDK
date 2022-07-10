using UnityEngine;

namespace Threeyes.Persistent
{
    public class PersistentData_Color : PersistentDataBase<Color, ColorEvent>
    {
        private void Reset()
        {
            defaultValue = Color.white;
        }

#if UNITY_EDITOR

        //！！MenuItem！！
        static string instName = "ColorPD ";
        [UnityEditor.MenuItem(strMenuItem_Root_Basic + "Color", false, intBasicMenuOrder + 8)]
        public static void CreateInst()
        {
            Editor.EditorTool.CreateGameObjectAsChild<PersistentData_Color>(instName);
        }

        //！！Hierarchy GUI！！
        public override string ShortTypeName { get { return "Color"; } }

#endif
    }
}