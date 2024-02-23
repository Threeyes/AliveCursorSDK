using System;
using UnityEngine;
using System.Text;
using Threeyes.Data;
using Threeyes.Core.Editor;
using Threeyes.Core;
#if USE_NaughtyAttributes
using NaughtyAttributes;
#endif
namespace Threeyes.Persistent
{
    /// <summary>
    /// 针对通用的Enum
    /// 
    /// Warning:
    /// ——(序列化时需要存储为string形式（eg：Newtonsoft.Json-for-Unity的设置中，启用StringEnumConverter）
    /// ——EnumEvent需要配合ReflectionValueChanger_Enum使用
    /// </summary>
    public class PersistentData_Enum : PersistentDataBase<Enum, EnumEvent, DataOption_Enum>
    {
        //因为Enum不能序列化，所以通过string缓存
        public override Type ValueType { get { return DefaultValue != null ? DefaultValue.GetType() : null; } }
        public override Enum DefaultValue { get { return dataOption.Parse(serializeDefaultValue); } set { serializeDefaultValue = value.ToString(); } }
        public override Enum PersistentValue { get { return dataOption.Parse(serializePersistentValue); } set { serializePersistentValue = value.ToString(); } }
#if USE_NaughtyAttributes
        [AllowNesting]
        [ReadOnly]
#endif
        [SerializeField] protected string serializeDefaultValue;

#if USE_NaughtyAttributes
        [AllowNesting]
        [ReadOnly]
#endif
        [SerializeField] protected string serializePersistentValue;

        public override bool IsValid
        {
            get
            {
                return dataOption.EnumType != null && base.IsValid;
            }
        }


#if UNITY_EDITOR

        //——MenuItem——
        static string instName = "EnumPD ";
        [UnityEditor.MenuItem(strMenuItem_Root_Basic + "Enum", false, intBasicMenuOrder + 6)]
        public static void CreateInst()
        {
            EditorTool.CreateGameObjectAsChild<PersistentData_Enum>(instName);
        }

        //——Hierarchy GUI——
        public override string ShortTypeName { get { return "Enum"; } }

        //——Inspector GUI——
        public override void SetInspectorGUIHelpBox_Error(StringBuilder sB)
        {
            base.SetInspectorGUIHelpBox_Error(sB);
            if (dataOption.EnumType == null)
            {
                sB.Append("Please set the corrent enumTypeFullName in Option!");
                sB.Append("\r\n");
            }
        }
#endif
    }
}