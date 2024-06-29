using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Threeyes.Core;
using UnityEngine;
namespace Threeyes.Persistent
{
    public static class PersistentObjectTool
    {
        #region Define

        /// <summary>
        /// 在DefaultFilter的基础上，额外排除UnityObject
        /// 
        /// 适用于：
        /// -Texture、Audio等因为拷贝而丢失引用
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        public static bool FieldsCopyFilter_ExcludeUnityObject(Type objectType, MemberInfo memberInfo)
        {
            bool isValid = FieldsCopyFilter_Default(objectType, memberInfo);
            if (isValid)
            {
                bool isSrcInheritFromUnityObject = objectType.IsInherit(typeof(UnityEngine.Object));//确认是否继承Unity.Object
                if (isSrcInheritFromUnityObject)
                    isValid = false;
            }
            return isValid;
        }
        /// <summary>
        /// 排除：
        /// -[PersistentDirPath]等自定义的不需要序列化的内容
        /// -Newtonsoft.Json标记为不需要序列化的内容
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        public static bool FieldsCopyFilter_Default(Type objectType, MemberInfo memberInfo)
        {
            if (memberInfo != null)
            {
                //[PersistentDirPathAttribute]通常与[JsonIgnore]联合使用，因此需要剔除
                if (memberInfo.GetCustomAttribute<PersistentDirPathAttribute>() != null)
                    return true;
            }
            //PS：
            //1.使用JsonDotNetTool.ShouldSerialize作为filter的原因：标记为不需要序列化的字段，也没有拷贝的必要;
            //2.event等不应该拷贝，否则会覆盖掉原值
            //3.标记为[JsonIgnore]的Texture等不应该拷贝，因为是实时读取的
            return JsonDotNetTool.ShouldSerialize(objectType, memberInfo);
        }
        #endregion

        #region Utility

        /// <summary>
        /// 使用现有数据，强制初始化（如读取文件、调用更新事件）
        /// 
        /// 适用于：
        /// -自行管理数据的初始化，或者需要强制读取外部文件
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="originInst">源数据</param>
        /// <param name="newInst">传入的数据</param>
        public static void ForceInit<TValue>(TValue originInst, string persistentDirPath, TValue newInst = null)
            where TValue : class
        {
            if (originInst == null)
            {
                Debug.LogError("originInst is null!");
                return;
            }
            ///PS:模拟PersistentDataComplexBase，在初始化完成后调用PersistentObjectTool.CopyFiledsAndLoadAsset(PersistentChangeState.Load)，以此来强制读取外部数据
            if (newInst == null)
                newInst = UnityObjectTool.DeepCopy(originInst);//克隆一份，用作比对
            CopyFiledsAndLoadAsset(originInst, newInst, PersistentChangeState.Load, persistentDirPath);
        }

        //针对特殊Attribute进行处理
        static List<FieldInfoDetail> listPOFieldInfoDetail = new List<FieldInfoDetail>();//[PersistentOption]
        static List<FieldInfoDetail> listPAFPFieldInfoDetail = new List<FieldInfoDetail>();//[PersistentAssetFilePath]
        static List<FieldInfoDetail> listPVCFieldInfoDetail = new List<FieldInfoDetail>();//[PersistentValueChanged]


        /// <summary>
        /// 将 newInst 的特定字段更新到 TargetValue 中，然后加载 newInst 的特定资源。
        /// 
        /// 常用于：
        /// -通过UIObjectModifierManager临时编辑后同步更新数据类
        /// </summary>
        /// <param name="originInst">原对象</param>
        /// <param name="newInst">新实例，通常是UIBox中的临时实例对象</param>
        /// <param name="persistentChangeState">状态</param>
        /// <param name="persistentDirPath">持久化路径</param>
        /// <param name="funcCopyFilter">待复制成员筛选器，如果为空则使用默认的筛选器</param>
        public static void CopyFiledsAndLoadAsset<TValue>(TValue originInst, TValue newInst, PersistentChangeState persistentChangeState, string persistentDirPath, Func<Type, MemberInfo, bool> funcCopyFilter = null)
        {
            if (funcCopyFilter == null)
                funcCopyFilter = FieldsCopyFilter_Default;

            //#1 【Cache】缓存新旧object值及相关Attribute绑定方法
            SetupInstance(originInst, newInst, persistentChangeState, persistentDirPath, true);

            //#2 只复制newInst可持久化的字段到originInst，忽略其他字段，避免场景引用等信息丢失【主要是针对Load时的值同步】
            UnityObjectTool.CopyFields(newInst, originInst, funcCopyFilter: funcCopyFilter);
            //CopyInstance(newInst, originInst, funcCopyFilter);

            //#3 【Set】调用originInst中的相关Attribute的方法（如加载图片）
            SetupInstance(originInst, newInst, persistentChangeState, persistentDirPath, false);
        }


        /// <summary>
        /// 标记两个实例
        /// </summary>
        /// <param name="originInst"></param>
        /// <param name="newInst"></param>
        /// <param name="persistentChangeState"></param>
        /// <param name="isCacheOrSet"></param>
        static void SetupInstance<TValue>(TValue originInst, TValue newInst, PersistentChangeState persistentChangeState, string persistentDirPath, bool isCacheOrSet)
        {
            if (originInst == null || newInst == null)
                return;
            if (originInst.GetType() != newInst.GetType())
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
            RecursiveMember(originInst, newInst, persistentChangeState, persistentDirPath, isCacheOrSet);
        }

        static void RecursiveMember(object originObj, object newObj, PersistentChangeState persistentChangeState, string persistentDirPath, bool isCacheOrSet, int maxDepth = 7, string parentChain = "")
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
                    fieldInfoPersistentDirPath.SetValue(originObj, persistentDirPath);
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
                                RecursiveMember(originObjList[i], newObjList[i], persistentChangeState, persistentDirPath, isCacheOrSet, --maxDepth, GetFieldPathChain(parentChain, fieldInfo, i));
                            }
                        }
                    }
                    else
                    {
                        RecursiveMember(fieldInfo.GetValue(originObj), fieldInfo.GetValue(newObj), persistentChangeState, persistentDirPath, isCacheOrSet, --maxDepth, fieldPathChain);
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

                        if (pVCAttribute != null)//#[PersistentValueChanged]//需要最后调用，便于针对Field进行修改
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

            //#Set：针对[PersistentChanged]（Class）（最后调用，方便进行整体更新）
            if (!isCacheOrSet)
            {
                PersistentChangedAttribute pcAttribute = objType.GetCustomAttribute<PersistentChangedAttribute>();
                pcAttribute?.InvokeCallbackMethodInfo(originObj, persistentChangeState);
            }
        }

        ///// <summary>
        ///// 克隆必要的字段
        ///// </summary>
        ///// <param name="source"></param>
        ///// <param name="target"></param>
        ///// <param name="funcCopyFilter"></param>
        //static void CopyInstance(TValue source, TValue target, Func<Type, MemberInfo, bool> funcCopyFilter = null)
        //{
        //    UnityObjectTool.CopyFields(source, target, funcCopyFilter: funcCopyFilter);
        //}

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
        class FieldInfoDetail
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

    }
}