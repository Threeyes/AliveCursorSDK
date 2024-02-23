using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using System.Linq;
using Threeyes.Data;
using System.Collections;
using Threeyes.Core;
using Threeyes.Core.Editor;
#if USE_NaughtyAttributes
using NaughtyAttributes;
#endif

namespace Threeyes.Persistent
{
    /// <summary>
    /// PersistentData for ScriptableObject
    /// 
    /// (ToAdd:增加一个组件，针对文件夹中相同类型，批量读取并放到List<SO>中）
    /// 
    ///PS：
    ///1.DefaultValue can be null，and will be the clone of TargetValue on game start
    ///2.因为TargetValue可能被多个场景实例引用，所以修改TargetValue即可同步修改场景引用
    ///3.程序退出时，如果是编辑器模式，则重置targetValue
    ///4.SO中的List<SO>会存储其原值而不是引用，因此其列表可以随意扩充
    ///
    /// Warning:
    ///1.如果本地没有PD，Load时使用的是从Asset克隆的内容；
    ///2.如果本地有PD，Load时使用的是通过序列化动态创建的内容，表现形式是SO中UnityObject相关字段显示（typemismatch）
    /// 
    /// </summary>
    public class PersistentData_SO : PersistentDataComplexBase<ScriptableObject, ScriptableObjectEvent, DataOption_SO>, IPersistentData_SO
    {
        public override bool IsValid { get { return TargetValue != null && base.IsValid; } }

        public override ScriptableObject TargetValue { get { return targetValue; } set { targetValue = value; } }

#if USE_NaughtyAttributes
        [Expandable]
#endif
        [SerializeField] protected ScriptableObject targetValue;//目标值

#if UNITY_EDITOR
        ScriptableObject cacheTargetValueClone;//【Editor only】保存TargetValue，便于退出时复原
#endif

        public override void Init()
        {
            base.Init();

#if UNITY_EDITOR
            cacheTargetValueClone = CloneInst(TargetValue); //PS:需要使用DeepInstantiate来Clone子SO，否则退出时无法正常还原List<SO>
#endif
        }

        public override void Dispose()
        {
#if UNITY_EDITOR
            /// 【编辑器模式】程序退出前重置TargetValue，避免其资源数值被修改。
            /// PS：
            /// -为了避免Texture等引用丢失，需要拷贝所有Fields
            /// -需要等Controller.SaveValue才还原
            if (cacheTargetValueClone != null)
            {
                UnityObjectTool.CopyFields(cacheTargetValueClone, TargetValue);//复制全部字段，不筛选（包括Asset引用）
            }
#endif
        }

        protected override ScriptableObject CloneInst(ScriptableObject origin)
        {
            return UnityObjectTool.DeepInstantiate(origin);
        }

#if UNITY_EDITOR
        //——MenuItem——
        static string instName = "SOPD ";
        [UnityEditor.MenuItem(strMenuItem_Root_Object + "ScriptableObject", false, intBasicMenuOrder + 2)]
        public static void CreateInst()
        {
            EditorTool.CreateGameObjectAsChild<PersistentData_SO>(instName);
        }

        //——Hierarchy GUI——
        public override string ShortTypeName { get { return "SO"; } }

        //——Inspector GUI——
        public override void SetInspectorGUIHelpBox_Error(StringBuilder sB)
        {
            base.SetInspectorGUIHelpBox_Error(sB);
            if (TargetValue == null)//针对引用类型，一定要保证有值
            {
                sB.Append("TargetValue can't be null!");
                sB.Append("\r\n");
            }
        }
#endif
    }
}
