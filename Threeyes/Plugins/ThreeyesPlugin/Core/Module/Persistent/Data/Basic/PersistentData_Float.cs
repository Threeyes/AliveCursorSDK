using Threeyes.Data;
using UnityEngine;

namespace Threeyes.Persistent
{
    public class PersistentData_Float : PersistentRangeNumberDataBase<float, FloatEvent, DataOption_Float>
    {
        protected override void OnValidate_OnMinValueChanged()
        {
            if (dataOption.MinValue >= dataOption.MaxValue)
            {
                dataOption.MinValue = dataOption.MaxValue - 0.02f;
            }
        }
        protected override void OnValidate_OnMaxValueChanged()
        {
            if (dataOption.MinValue >= dataOption.MaxValue)
            {
                dataOption.MaxValue = dataOption.MinValue + 0.02f;
            }
        }
        protected override void OnValidate_UpdateDefaultValue()
        {
            defaultValue = Mathf.Clamp(defaultValue, dataOption.MinValue, dataOption.MaxValue);
        }

#if UNITY_EDITOR

        //！！MenuItem！！
        static string instName = "FloatPD ";
        [UnityEditor.MenuItem(strMenuItem_Root_Basic + "Float", false, intBasicMenuOrder + 2)]
        public static void CreateInst()
        {
            Editor.EditorTool.CreateGameObjectAsChild<PersistentData_Float>(instName);
        }

        //！！Hierarchy GUI！！
        public override string ShortTypeName { get { return "Float"; } }

#endif
    }
}