using System.Collections;
using System.Collections.Generic;
using System.Text;
using Threeyes.Core;
using Threeyes.Core.Editor;
using Threeyes.Data;
using UnityEngine;
using Type = System.Type;
namespace Threeyes.Persistent
{
    /// <summary>
    /// 针对UnityObject（如组件）
    ///  
    /// Warning：
    /// -针对运行时会变化的类，建议使用PersistentData_Object代替
    /// 【待完善】
    /// 
    /// ToTest
    /// </summary>
    public class PersistentData_UnityObject : PersistentDataComplexBase<Object, UnityObjectEvent, DataOption_UnityObject>, IPersistentData_UnityObject
    {
        public override bool IsValid { get { return dataOption.objectTypeFullName.NotNullOrEmpty() && base.IsValid; } }

        public override Type ValueType
        {
            get
            {
                return Type.GetType(dataOption.objectTypeFullName);
            }
        }
        
        public override Object TargetValue { get { return targetValue; } set { targetValue = value; } }
        [SerializeField] protected Object targetValue;//目标值

        protected override Object CloneInst(Object origin)
        {
            return ReflectionTool.DeepCopy(origin);
        }


#if UNITY_EDITOR
        [ContextMenu("ManualSetType")]
        void ManualSetType()
        {
            if (TargetValue == null)
            {
                Debug.LogError("TargetValue is null!");
                return;
            }
            dataOption.objectTypeFullName = TargetValue.GetType().FullName;
            UnityEditor.EditorUtility.SetDirty(this);
        }

        //——MenuItem——
        static string instName = "UnityObjectPD ";
        [UnityEditor.MenuItem(strMenuItem_Root_Object + "UnityObject", false, intBasicMenuOrder + 1)]
        public static void CreateInst()
        {
            EditorTool.CreateGameObjectAsChild<PersistentData_UnityObject>(instName);
        }

        //——Hierarchy GUI——
        public override string ShortTypeName { get { return "UObject"; } }

        //——Inspector GUI——
        public override void SetInspectorGUIHelpBox_Error(StringBuilder sB)
        {
            base.SetInspectorGUIHelpBox_Error(sB);

            if (dataOption.objectTypeFullName.IsNullOrEmpty())//针对引用类型，一定要保证有值
            {
                sB.Append("dataOption.objectTypeFullName can't be null!");
                sB.Append("\r\n");
            }

            /// PS：因为targetValue需要在运行时读写，因此暂不作为判断依据
            //if (TargetValue == null)//针对引用类型，一定要保证有值
            //    {
            //        sB.Append("TargetValue can't be null!");
            //        sB.Append("\r\n");
            //    }
        }
#endif
    }
}