using UnityEngine;
using Threeyes.Data;
using System;
using System.Text;
namespace Threeyes.Persistent
{
    /// <summary>
    /// 针对普通的数据类
    ///【待完善】
    ///
    /// ToDo:
    /// +应该从PersistentData_SO中共用大量参数及父类，减少RuntimeEdit得工作量
    ///-通过辅助类来赋值（参考RTS_Controller）
    /// Warning:
    /// -Type.GetType只能获取已知的类型，因此针对Mod定义的未知的类型，或命名空间外的类型无效（解决办法：搜索：type.gettype from another assembly，结果是建议带上Assembly）
    /// -问题是UMod的包，其Assembly名是自定义且可变的（通过Test_脚本可打印，结果如：umod-compiled-86a7224b-13e3-4533-a8f8-1b129c623f9c），要使用该类，只能在运行时组合该objectTypeFullName，或者使用某种Action，让其进行预先处理，替换占位符umod为对应命名空间
    /// -解决命名空间不一致：【优先】【ToTest】：可以通过GetType的其他参数（assemblyResolver）来处理命名空间不一致的问题（比如先按照默认名称找，如果找不到再通过UMod的接口获取其命名空间并在其中找）
    /// -序列化变种实现：【ToTest】：参考RTS_Component，直接序列化成string，这样可以避免type不一致的问题
    /// -综上所述，目前只支持已经的代码类，或者运行时不变的代码类
    /// 
    /// ToFix：
    /// -初次通过编辑器打开时，图片没有酒啊在
    /// </summary>
    public class PersistentData_Object : PersistentDataComplexBase<object, ObjectEvent, DataOption_Object>, IPersistentData_Object
    {
        public Component compInitializer;//（可空，如果为空则尝试从身上获取）继承IPersistentData_ObjectInitializer的组件，用于初始化具体Object（如类型、TargetValue）

        public override bool IsValid { get { return GetInitializer() != null && base.IsValid; } }

        /// <summary>
        /// PS：
        /// -【运行时设置】作用是在序列化/反序列化时表明该实例的真正类型
        /// 
        /// Warning:
        /// -因为Type.GetType默认只能是在该代码自身的命名空间中获取，所以改为运行时由外部对应的Initializer传入并设置
        /// </summary>
        public override Type ValueType { get { return valueType; } set { valueType = value; } }
        Type valueType;

        public override object TargetValue { get { return targetValue; } set { targetValue = value; } }

        protected object targetValue;//目标值

        public override void Init()
        {
            //初始化必要字段及链接，避免报错
            IPersistentData_ObjectInitializer persistentData_ObjectInitializer = GetInitializer();
            if (persistentData_ObjectInitializer != null)
            {
                ValueType = persistentData_ObjectInitializer.ValueType;
                TargetValue = persistentData_ObjectInitializer.BaseTargetValue;//需要通过外部来设置targetValue，因为object类在Inspector不可见或序列化
            }
            else
            {
                Debug.LogError($"Can't find {nameof(IPersistentData_ObjectInitializer)} in this GameObject [{gameObject.name}]!");
            }


            base.Init();
        }

        #region Utility
        /// <summary>
        /// 获取默认的Initializer
        /// </summary>
        /// <returns></returns>
        IPersistentData_ObjectInitializer GetInitializer()
        {
            if (compInitializer)
            {
                return compInitializer.GetComponent<IPersistentData_ObjectInitializer>();
            }
            return GetComponent<IPersistentData_ObjectInitializer>();
        }

        #endregion

#if UNITY_EDITOR
        //——MenuItem——
        static string instName = "ObjectPD ";
        [UnityEditor.MenuItem(strMenuItem_Root_Object + "Object", false, intBasicMenuOrder + 0)]
        public static void CreateInst()
        {
            Editor.EditorTool.CreateGameObjectAsChild<PersistentData_Object>(instName);
        }

        //——Hierarchy GUI——
        public override string ShortTypeName { get { return "Object"; } }

        //——Inspector GUI——
        public override void SetInspectorGUIHelpBox_Error(StringBuilder sB)
        {
            base.SetInspectorGUIHelpBox_Error(sB);

            if (GetInitializer() == null)
            {
                sB.Append("You need to specify a Component that inherits IPersistentData_ObjectInitializer! (Which hanging on this GameObject or as a value for compInitializer)");//你需要指定一个继承IPersistentData_ObjectInitializer的组件（挂在该物体，或作为compInitializer的值）
                sB.Append("\r\n");
            }
            /// PS：因为targetValue需要在运行时读写，因此暂不作为判断依据
            //if (TargetValue == null)//针对引用类型，一定要保证有值
            //{
            //    sB.Append("TargetValue can't be null!");
            //    sB.Append("\r\n");
            //}
        }
#endif
    }
}