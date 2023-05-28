using UnityEngine;

namespace Threeyes.Persistent
{
    public class PersistentData_Gradient : PersistentDataBase<Gradient, GradientEvent>
    {
        private void Reset()
        {
            defaultValue = new Gradient();
            defaultValue.colorKeys = new GradientColorKey[]
            {
                new GradientColorKey(Color.white,0),
                new GradientColorKey(Color.black, 1)
            };
        }

#if UNITY_EDITOR

        //！！MenuItem！！
        static string instName = "GradientPD ";
        [UnityEditor.MenuItem(strMenuItem_Root_Basic + "Gradient", false, intBasicMenuOrder + 9)]
        public static void CreateInst()
        {
            Editor.EditorTool.CreateGameObjectAsChild<PersistentData_Gradient>(instName);
        }

        //！！Hierarchy GUI！！
        public override string ShortTypeName { get { return "Gradient"; } }

#endif
    }
}