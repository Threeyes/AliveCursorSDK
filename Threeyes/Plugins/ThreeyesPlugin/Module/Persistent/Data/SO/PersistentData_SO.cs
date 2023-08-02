using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using System.Linq;
using Threeyes.Data;
using System.Collections;
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
    public class PersistentData_SO : PersistentDataBase<ScriptableObject, ScriptableObjectEvent, DataOption_SO>, IPersistentData_SO
    {
        public virtual FilePathModifier FilePathModifier { get { if (filePathModifier_PD == null) filePathModifier_PD = new FilePathModifier_PD(this); return filePathModifier_PD; } }
        private FilePathModifier_PD filePathModifier_PD;

        public override bool IsValid { get { return TargetValue != null && base.IsValid; } }
        public override Type ValueType { get { return TargetValue != null ? TargetValue.GetType() : null; } }

        public override ScriptableObject ValueToSaved { get { return TargetValue; } }//直接存储TargetValue，这样能确保即使不通过UIField_SO修改该SO，也能正常保存字段
        public ScriptableObject TargetValue { get { return targetValue; } set { targetValue = value; } }

#if USE_NaughtyAttributes
        [Expandable]
#endif
        [SerializeField] protected ScriptableObject targetValue;//目标值


        ScriptableObject cacheTargetValueClone;//【Editor only】保存TargetValue，便于退出时复原

        public override void Init()
        {
            base.Init();

            //需要使用DeepInstantiate来Clone子SO，否则退出时无法正常还原List<SO>,
            cacheTargetValueClone = ScriptableObjectTool.DeepInstantiate(TargetValue);

            //使用默认的TargetValue作为DefaultValue，便于Controller.Delete时重置
            if (defaultValue == null)
            {
                defaultValue = ScriptableObjectTool.DeepInstantiate(TargetValue);//PS:与cacheTargetValueClone不能是同一个引用，否则会导致重置时被修改
            }
        }

        //public override void SetDirty()//ToDelete
        //{
        //    base.SetDirty();

        //    //主动将TargetValue的值应用到PersistentValue中，常用于直接修改TargetValue的字段后主动调用。
        //    ScriptableObject tempSOClone = Instantiate(TargetValue);
        //    tempSOClone.name = tempSOClone.name.Replace("(Clone)", "") + "(Clone)";
        //    persistentValue = tempSOClone;
        //}

        public override void Dispose()
        {
            /// 【编辑器模式】程序退出前重置TargetValue，避免其资源数值被修改。PS：
            ///1.为了避免Texture等引用丢失，需要拷贝所有Fields
            ///2.需要等Controller.SaveValue才还原
            if (Application.isEditor && cacheTargetValueClone != null)
            {
                ScriptableObjectTool.Copy(cacheTargetValueClone, TargetValue);//全部复制，不筛选（包括Asset引用）
            }
        }

        /// <summary>
        /// 调用时机：
        /// 1.Load/Set/EditorReset
        ///2.PersistentControllerBase.NotifyValueChanged
        /// Warning：因为不一定通过RunEdit调用（如初始化），因此加载方法不能放到那个类中
        /// </summary>
        /// <param name="value"></param>
        public override void OnValueChanged(ScriptableObject value, PersistentChangeState persistentChangeState)
        {
            try
            {
                ///克隆值：value -> TargetValue

                //#1 缓存新旧SO值及相关Attribute绑定方法
                SetupSO(TargetValue, value, persistentChangeState, true);

                //#2 只复制可持久化的字段，忽略其他字段，避免场景引用等信息丢失【主要是针对Load时的值同步】
                Copy(value, TargetValue, SOFIeldCopyFilter);

                //#3 调用SO中的相关Attribute
                SetupSO(TargetValue, value, persistentChangeState, false);

                //#4 通知外界Event
                base.OnValueChanged(TargetValue, persistentChangeState);
            }
            catch (Exception e)
            {
                Debug.LogError("PersistentData_SO.OnValueChanged with error:\r\n" + e);
            }
        }

        #region Utility

        //针对特殊Attribute进行处理
        List<FieldInfoDetail> listPOFieldInfoDetail = new List<FieldInfoDetail>();//[PersistentOption]
        List<FieldInfoDetail> listPAFPFieldInfoDetail = new List<FieldInfoDetail>();//[PersistentAssetFilePath]
        List<FieldInfoDetail> listPVCFieldInfoDetail = new List<FieldInfoDetail>();//[PersistentValueChanged]
        void SetupSO(ScriptableObject originSO, ScriptableObject newSO, PersistentChangeState persistentChangeState, bool isCacheOrSet)
        {
            if (!originSO || !newSO || originSO.GetType() != newSO.GetType())
                return;

            if (isCacheOrSet)//Cache时重置信息
            {
                listPOFieldInfoDetail = new List<FieldInfoDetail>();
                listPAFPFieldInfoDetail = new List<FieldInfoDetail>();
                listPVCFieldInfoDetail = new List<FieldInfoDetail>();
            }
            ///原理：
            ///1.在Cache时，保存originSO及newSO对应Attribute的信息
            ///2.在Set时，调用Attribute的方法等
            RecursiveMember(originSO, newSO, persistentChangeState, isCacheOrSet);
        }

        private void RecursiveMember(object originObj, object newObj, PersistentChangeState persistentChangeState, bool isCacheOrSet, int maxDepth = 7, string parentChain = "")
        {
            if (maxDepth == -1) return;//限制迭代次数
            if (originObj == null || newObj == null) return;
            if (originObj.GetType() != newObj.GetType()) return;


            Type objType = originObj.GetType();

            //#针对[PersistentDirPath]（Class）（PS:需要优先设置，因为被其他路径相关的Attribute依赖）
            if (!isCacheOrSet)
            {
                FieldInfo fieldInfoPersistentDirPath = objType.GetFieldWithAttribute<PersistentDirPathAttribute>();
                if (fieldInfoPersistentDirPath != null)
                    fieldInfoPersistentDirPath.SetValue(originObj, PersistentDirPath);
            }

            foreach (FieldInfo fieldInfo in objType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                Type fieldType = fieldInfo.FieldType;
                string fieldPathChain = GetFieldPathChain(parentChain, fieldInfo);

                if (fieldType.IsClass && fieldType != typeof(string))//PS:string、gradient等 也是 class，因此不能跳过之后的操作
                {
                    object originObjField = fieldInfo.GetValue(originObj);
                    if (originObjField is IList originObjList)//List<T>、T[]等引用合集（建议用户声明List<T>）
                    {
                        object newObjObjField = fieldInfo.GetValue(newObj);
                        if (newObjObjField is IList newObjList && originObjList.Count == newObjList.Count)//需要两者不为空且数量一致
                        {
                            for (int i = 0; i != originObjList.Count; i++)
                            {
                                RecursiveMember(originObjList[i], newObjList[i], persistentChangeState, isCacheOrSet, --maxDepth, GetFieldPathChain(parentChain, fieldInfo, i));
                            }
                        }
                    }
                    else
                    {
                        RecursiveMember(fieldInfo.GetValue(originObj), fieldInfo.GetValue(newObj), persistentChangeState, isCacheOrSet, --maxDepth, fieldPathChain);
                    }
                }

                //#1 缓存特定FieldInfo(PS:以下2个Attribute可以并行）PS:以下Attribute只有变化了才调用，避免多余调用
                PersistentOptionAttribute pOAttribute = fieldInfo.GetCustomAttribute<PersistentOptionAttribute>();
                PersistentAssetFilePathAttribute pAFPAttribute = fieldInfo.GetCustomAttribute<PersistentAssetFilePathAttribute>();
                PersistentValueChangedAttribute pVCAttribute = fieldInfo.GetCustomAttribute<PersistentValueChangedAttribute>();
                if (pOAttribute != null || pAFPAttribute != null || pVCAttribute != null)
                {
                    if (isCacheOrSet)//Cache
                    {
                        FieldInfoDetail fieldInfoDetail = new FieldInfoDetail()
                        {
                            objType = objType,
                            fieldInfo = fieldInfo,
                            oldFieldValue = fieldInfo.GetValue(originObj),
                            newFieldValue = fieldInfo.GetValue(newObj),
                            persistentChangeState = persistentChangeState
                        };

                        //为避免多余调用，确定调用Attribute方法的时机：Load State || 其他State&值不相等
                        if (persistentChangeState == PersistentChangeState.Load || persistentChangeState != PersistentChangeState.Load && !fieldInfoDetail.IsValueEqual)
                        {
                            if (pOAttribute != null)
                                listPOFieldInfoDetail.Add(fieldInfoDetail);
                            if (pAFPAttribute != null)
                                listPAFPFieldInfoDetail.Add(fieldInfoDetail);
                            if (pVCAttribute != null)
                                listPVCFieldInfoDetail.Add(fieldInfoDetail);
                        }
                    }
                    else//Set
                    {
                        if (pOAttribute != null)//#[PersistentOption]
                        {
                            FieldInfoDetail targetFieldInfoDetail = listPOFieldInfoDetail.FirstOrDefault(mi => mi.UniqueID == FieldInfoDetail.GetUniqueID(fieldInfo));
                            if (targetFieldInfoDetail != null)//决定是否调用
                                pOAttribute.SetTargetOption(fieldInfo, originObj);
                        }

                        if (pAFPAttribute != null)//#[PersistentAssetFilePath]
                        {
                            FieldInfoDetail targetFieldInfoDetail = listPAFPFieldInfoDetail.FirstOrDefault(mi => mi.UniqueID == FieldInfoDetail.GetUniqueID(fieldInfo));
                            if (targetFieldInfoDetail != null)//决定是否调用
                            {
                                object objAsset = pAFPAttribute.InvokeCallbackMethodInfo(fieldInfo, originObj, persistentChangeState, fieldPathChain);//自动加载资源到对应字段（PS:因为内部逻辑比较复杂，因此封装成Attribute方法）
                            }
                        }

                        if (pVCAttribute != null)//#[PersistentValueChanged]//需要最后调用，便于针对Field进行最后的修改
                        {
                            FieldInfoDetail targetFieldInfoDetail = listPVCFieldInfoDetail.FirstOrDefault(mi => mi.UniqueID == FieldInfoDetail.GetUniqueID(fieldInfo));
                            if (targetFieldInfoDetail != null)
                            {
                                pVCAttribute.InvokeCallbackMethodInfo(fieldInfo, originObj, targetFieldInfoDetail.oldFieldValue, targetFieldInfoDetail.newFieldValue, persistentChangeState);
                            }
                        }
                    }
                }
            }

            //#针对[PersistentChanged]（Class）（最后调用）
            if (!isCacheOrSet)
            {
                PersistentChangedAttribute pcAttribute = objType.GetCustomAttribute<PersistentChangedAttribute>();
                pcAttribute?.InvokeCallbackMethodInfo(originObj, persistentChangeState);
            }
        }
        public static void Copy(ScriptableObject source, ScriptableObject target, Func<Type, MemberInfo, bool> funcCopyFilter = null)
        {
            ScriptableObjectTool.Copy(source, target, funcCopyFilter != null ? funcCopyFilter : SOFIeldCopyFilter);
        }
        static bool SOFIeldCopyFilter(Type objectType, MemberInfo memberInfo)
        {
            if (memberInfo != null)
            {
                //[PersistentDirPathAttribute]通常与[JsonIgnore]联合使用，因此需要剔除
                if (memberInfo.GetCustomAttribute<PersistentDirPathAttribute>() != null)
                    return true;
            }
            //PS：
            //1.使用JsonDotNetTool.ShouldSerialize作为filter的原因：标记为不需要序列化的字段，也没有拷贝的必要; Texture、Audio等可能会因为拷贝而丢失引用
            //2.event等不应该拷贝，否则会覆盖掉原值
            return JsonDotNetTool.ShouldSerialize(objectType, memberInfo);
        }


        /// <summary>
        /// 获取字段的唯一路径，用于对比不同字段
        /// </summary>
        /// <param name="parentChain"></param>
        /// <param name="subFieldName"></param>
        /// <returns></returns>
        static string GetFieldPathChain(string parentChain, FieldInfo fieldInfo, int? indexValue = null)
        {
            return ReflectionTool.GetFieldPathChain(parentChain, fieldInfo, indexValue);
        }

        /// <summary>
        /// 记录同一FieldInfo的新/旧信息
        /// </summary>
        public class FieldInfoDetail
        {
            public Type objType;
            public FieldInfo fieldInfo;
            public object oldFieldValue;
            public object newFieldValue;
            public PersistentChangeState persistentChangeState;
            public FieldInfoDetail()
            {
            }

            //PS:自定义类可以通过重载Equal来实现匹配。通常都是string等简单类型（File读写也是基于string）
            public bool IsValueEqual { get { return System.Object.Equals(oldFieldValue, newFieldValue); } }//PS:能够避免objA为null的情况

            //返回唯一ID，用于匹配FieldInfo
            public string UniqueID { get { return GetUniqueID(fieldInfo); } }
            public static string GetUniqueID(FieldInfo fieldInfo)
            {
                return fieldInfo.DeclaringType.FullName + "." + fieldInfo.Name;
            }

        }

        #endregion


#if UNITY_EDITOR

        //——MenuItem——
        static string instName = "SOPD ";
        [UnityEditor.MenuItem(strMenuItem_Root_SO + "ScriptableObject", false, intBasicMenuOrder + 0)]
        public static void CreateInst()
        {
            Editor.EditorTool.CreateGameObjectAsChild<PersistentData_SO>(instName);
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
