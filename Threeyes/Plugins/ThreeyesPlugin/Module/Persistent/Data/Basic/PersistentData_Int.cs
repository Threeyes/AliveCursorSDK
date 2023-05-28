using UnityEngine;
using Threeyes.Data;

namespace Threeyes.Persistent
{
    public class PersistentData_Int : PersistentRangeNumberDataBase<int, IntEvent, DataOption_Int>
    {
        protected override void OnValidate_OnMinValueChanged()
        {
            if (dataOption.MinValue >= dataOption.MaxValue)
            {
                dataOption.MinValue = dataOption.MaxValue - 1;
            }
        }
        protected override void OnValidate_OnMaxValueChanged()
        {
            if (dataOption.MinValue >= dataOption.MaxValue)
            {
                dataOption.MaxValue = dataOption.MinValue + 1;
            }
        }
        protected override void OnValidate_UpdateDefaultValue()
        {
            defaultValue = Mathf.Clamp(defaultValue, dataOption.MinValue, dataOption.MaxValue);
        }

#if UNITY_EDITOR

        //——MenuItem——
        static string instName = "IntPD ";
        [UnityEditor.MenuItem(strMenuItem_Root_Basic + "Int", false, intBasicMenuOrder + 1)]
        public static void CreateInst()
        {
            Editor.EditorTool.CreateGameObjectAsChild<PersistentData_Int>(instName);
        }

        //——Hierarchy GUI——
        public override string ShortTypeName { get { return "Int"; } }

#endif
    }
}