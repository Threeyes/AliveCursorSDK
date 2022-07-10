using System;
using UnityEngine;
using System.Text;
using Threeyes.Data;
#if USE_NaughtyAttributes
using NaughtyAttributes;
#endif
namespace Threeyes.Persistent
{
    //Enum to test:
    //  UnityEngine.TextAlignment
    //  UnityEngine.CameraType      (Flag)
    //  SOWorkshopItemInfo+ItemStyle        (Flag)

    /// <summary>
    /// ���ͨ�õ�Enum
    /// 
    /// Warning:
    /// ����(���л�ʱ��Ҫ�洢Ϊstring��ʽ��eg��Newtonsoft.Json-for-Unity�������У�����StringEnumConverter��
    /// ����EnumEvent��Ҫ���ReflectionValueChanger_Enumʹ��
    /// </summary>
    public class PersistentData_Enum : PersistentDataBase<Enum, EnumEvent, DataOption_EnumInfo>
    {
        //��ΪEnum�������л�������ͨ��string����
        public override Type RealValueType { get { return DefaultValue != null ? DefaultValue.GetType() : null; } }
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

        //����MenuItem����
        static string instName = "EnumPD ";
        [UnityEditor.MenuItem(strMenuItem_Root_Basic + "Enum", false, intBasicMenuOrder + 6)]
        public static void CreateInst()
        {
            Editor.EditorTool.CreateGameObjectAsChild<PersistentData_Enum>(instName);
        }

        //����Hierarchy GUI����
        public override string ShortTypeName { get { return "Enum"; } }

        //����Inspector GUI����
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