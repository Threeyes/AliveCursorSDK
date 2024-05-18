using System;
using UnityEngine;
using System.Linq;
using System.Reflection;
using Threeyes.Data;
using Threeyes.External;
using System.Collections;
using Threeyes.Core;

namespace Threeyes.Persistent
{
    public abstract class FieldCallabackAttributeBase : Attribute
    {
        public string CallbackMethodName { get; set; }

        protected FieldCallabackAttributeBase(string callbackMethodName)
        {
            CallbackMethodName = callbackMethodName;
        }
        public abstract MethodInfo GetCallbackMethodInfo(FieldInfo fieldInfoAttribute);
    }

    //——Field——

    /// <summary>
    /// [Runtime] Cache the location of the Persistent Data Dir
    /// 
    /// 用途：
    /// -资源需要使用相对路径进行读写
    /// 
    /// PS:
    /// 1.valid for string field only
    /// 2.Need to mark as [HideInInspector] to avoid runtime edit
    /// 3.Need to mark as [JsonIgnore], because the value changes at runtime, which means it doesn't need to be saved. However PD_SO will force to copy this field to other SO for asset loading purpose.
    /// 4.Optional, if you prefer Abs path, then 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class PersistentDirPathAttribute : Attribute { }

    /// <summary>
    /// 【Todo】标记默认值字段，适用于普通类型
    /// 
    /// Ref：DefaultValueAttribute
    /// 用途：
    /// -重置时使用defaultValue
    /// -UIField_String中作为Holder的值
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class PersistentDefaultValueAttribute : Attribute
    {
        public string DefaultValueFieldName { get; set; }

    }

    /// <summary>
    /// Cache asset fieldinfo's external file path, and optional load the asset
    /// [valid for string field only]
    /// 
    /// Warning:
    /// 1. 加载的资源要在退出前前Dispose
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class PersistentAssetFilePathAttribute : Attribute
    {
        //ToAvoid：PD_SO不会将Attribute的值拷贝到其他SO上，因此不能如下声明PersistentDirPath存储[Runtime]信息
        //public string PersistentDirPath { get; set; }//(Setup via runtime)

        public string AssetFieldName { get; set; }
        public bool IsAutoLoad { get; set; }
        public string AssetLoadedCallbackMethodName { get; set; }
        public string DataOption_FilePropertyName { get; set; }
        public string DefaultAssetFieldName { get; set; }

        /// <summary>
        /// 
        /// PS:
        /// 1.GetCustomAttribute返回的是一个新的实例Attribute，因此不能在此存储数据
        /// </summary>
        public static Action<object, FieldInfo, ExternalResources.LoadResult, object, string> AssetChanged;
        public string debugStr;

        /// <summary>
        /// Callback method when asset loaded.
        /// 
        /// Format: 
        /// 1.T OnAssetLoaded(ExternalResources.LoadResult<T> loadResult, PersistentChangeState persistentChangeState,,)) 
        /// 
        /// Usage:Generate [DataOption_File]
        /// 
        /// PS:
        /// -defaultAssetField主要用于保存重置时的默认贴图
        /// -建议使用额外的属性，来访问及返回需要的非空字段
        /// -assetField和defaultAssetField需要标记为[JsonIgnore]
        /// -如果不想用户在Editor中编辑assetField字段，可以标记为[HideInInspector]。如果希望调试，可以不加该Attribute
        /// </summary>
        /// <param name="assetFieldName">The name of the binding asset field</param>
        /// <param name="isAutoLoad">Auto load the asset</param>
        /// <param name="dataOption_FilePropertyName">The property to return DataOption_XXXFile instance. Note: the type of this instance should match the assetField, such as DataOption_TextureFile for Texture</param>
        /// <param name="assetLoadedCallbackMethodName">
        /// </param>
        /// <param name="defaultAssetFieldName">The name of the binding default asset field</param>
        public PersistentAssetFilePathAttribute(string assetFieldName, bool isAutoLoad = false, string dataOption_FilePropertyName = null, string assetLoadedCallbackMethodName = null, string defaultAssetFieldName = null)
        {
            AssetFieldName = assetFieldName;
            IsAutoLoad = isAutoLoad;
            AssetLoadedCallbackMethodName = assetLoadedCallbackMethodName;
            DataOption_FilePropertyName = dataOption_FilePropertyName;
            DefaultAssetFieldName = defaultAssetFieldName;
        }

        public object InvokeCallbackMethodInfo(FieldInfo fieldInfoAttribute, object obj, PersistentChangeState persistentChangeState, string fieldPathChain)
        {
            if (fieldInfoAttribute == null)
                return null;
            if (obj == null)
                return null;

            object objAsset = null;
            object objDefaultAsset = null;
            try
            {
                if (IsAutoLoad)//自动加载资源
                {
                    FieldInfo fieldInfoAsset = GetBindingAssetFieldInfo(fieldInfoAttribute);
                    if (ExternalResources.CanLoad(fieldInfoAsset.FieldType))
                    {
                        //ToUse:dataOption_File.ReadFileOption，支持async读取
                        DataOption_File dataOption_File = GetDataOption_FilePropertyValue(fieldInfoAttribute, obj);

                        string absFilePath = GetAbsPersistentAssetFilePath(fieldInfoAttribute, obj);
                        //if (absFilePath.IsNullOrEmpty())//Warning:如果这里直接返回，会导致重置时无法刷新（Todo：可以是返回null来提示，或者返回Dummy）(PS:""代表重置，可以继续执行)
                        //    return null;
                        ExternalResources.LoadResult loadResult = ExternalResources.LoadEx(absFilePath, fieldInfoAsset.FieldType, dataOption_File?.DecodeOption);//PS:其返回的原类型是LoadResult<T>，因此对应的绑定方法可以声明对应的参数类型

                        //绑定方法不为空则调用，用户可以在方法中修改值并返回新值；否则直接使用加载后的Asset
                        MethodInfo methodInfoCallback = GetCallbackMethodInfo(fieldInfoAttribute);
                        if (methodInfoCallback != null)
                            objAsset = methodInfoCallback.Invoke(obj, new object[] { loadResult, persistentChangeState });
                        else
                            objAsset = loadResult.ObjValue;

                        if (objAsset == null)//如果加载失败且绑定了DefaultAsset，则使用该值
                        {
                            if (DefaultAssetFieldName.NotNullOrEmpty())
                            {
                                //ToUpdate: 如果后期有需求，DefaultAssetFieldName也可以代表PropertyName，便于用户使用Property返回DefaultAsset
                                objDefaultAsset = ReflectionTool.GetFieldValue(obj, DefaultAssetFieldName, fieldInfoAsset.FieldType, null);
                                if (objDefaultAsset != null)
                                    objAsset = objDefaultAsset;
                            }
                        }

                        fieldInfoAsset.SetValue(obj, objAsset);//更新Asset

                        //#[PersistentAssetChangedAttribute]
                        //如果AssetField有[PersistentAssetChangedAttribute]，那就调用其绑定方法
                        PersistentAssetChangedAttribute persistentAssetChangedAttribute = fieldInfoAsset.GetCustomAttribute<PersistentAssetChangedAttribute>();
                        if (persistentAssetChangedAttribute != null)
                        {
                            persistentAssetChangedAttribute.InvokeCallbackMethodInfo(fieldInfoAsset, obj, objAsset, persistentChangeState);
                        }

                        //通知UIField等更新(其中obj及FieldInfo可理解为Key)
                        AssetChanged.Execute(obj, fieldInfoAsset, loadResult, objDefaultAsset, fieldPathChain);
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("Invoke callback method error:\r\n" + e);
            }
            return objAsset;
        }
        public object GetAssetFieldValue(FieldInfo fieldInfoAsset, object obj)
        {
            return ReflectionTool.GetFieldValue(obj, AssetFieldName, fieldInfoAsset.FieldType, null);
        }
        public object GetDefaultAssetFieldValue(FieldInfo fieldInfoAsset, object obj)
        {
            return ReflectionTool.GetFieldValue(obj, DefaultAssetFieldName, fieldInfoAsset.FieldType, null);
        }

        public DataOption_File GetDataOption_FilePropertyValue(FieldInfo fieldInfoAttribute, object obj)
        {
            //ToUpdate：这里先获取Property，然后通过DataOptionFactory获得
            DataOption_File dataOption_File = ReflectionTool.GetPropertyValue<DataOption_File>(obj, DataOption_FilePropertyName, null);

            //如果为空，则返回Asset对应的默认DataOption_File（如DataOption_ImageFile）
            if (dataOption_File == null)
            {
                FieldInfo fieldInfoAsset = GetBindingAssetFieldInfo(fieldInfoAttribute);
                dataOption_File = DataOptionFactory.CreateFile(fieldInfoAttribute, fieldInfoAsset?.FieldType);
            }

            return dataOption_File;
        }


        public string GetAbsPersistentAssetFilePath(FieldInfo fieldInfoAttribute, object obj)
        {
            if (obj == null || fieldInfoAttribute == null)
                return null;

            //#1 Get assetFilePath
            string strAssetFilePath = fieldInfoAttribute.GetValue(obj) as string;

            //#2 Get PersistentDirPath (can be null)
            string strPersistentDirPath = GetPersistentDirPath(obj);
            return PathTool.GetAbsPath(strPersistentDirPath, strAssetFilePath);
        }

        public string GetPersistentDirPath(object obj)//获取obj中唯一[PersistentDirPath]
        {
            FieldInfo fieldInfoPersistentDirPath = obj.GetType().GetFieldWithAttribute<PersistentDirPathAttribute>();
            if (fieldInfoPersistentDirPath != null)
                return fieldInfoPersistentDirPath.GetValue(obj) as string;
            return null;
        }

        #region Get

        /// <summary>
        /// 返回定义Asset对应的FieldInfo
        /// </summary>
        /// <param name="fieldInfoAttribute">string Field that has [PersistentAssetFilePath]</param>
        /// <returns>The relate asset fieldinfo</returns>
        public static FieldInfo GetBindingAssetFieldInfo(FieldInfo fieldInfoAttribute)
        {
            if (fieldInfoAttribute == null)
                return null;
            PersistentAssetFilePathAttribute attribute = fieldInfoAttribute.GetCustomAttribute<PersistentAssetFilePathAttribute>();
            if (attribute == null)
                return null;
            if (attribute.AssetFieldName.IsNullOrEmpty())
                return null;
            return fieldInfoAttribute.DeclaringType.GetField(attribute.AssetFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public static FieldInfo GetBindingDefaultAssetFieldInfo(FieldInfo fieldInfoAttribute)
        {
            if (fieldInfoAttribute == null)
                return null;
            PersistentAssetFilePathAttribute attribute = fieldInfoAttribute.GetCustomAttribute<PersistentAssetFilePathAttribute>();
            if (attribute == null)
                return null;
            if (attribute.DefaultAssetFieldName.IsNullOrEmpty())
                return null;
            return fieldInfoAttribute.DeclaringType.GetField(attribute.DefaultAssetFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }
        public static MethodInfo GetCallbackMethodInfo(FieldInfo fieldInfoAttribute)
        {
            if (fieldInfoAttribute == null)
                return null;
            PersistentAssetFilePathAttribute attribute = fieldInfoAttribute.GetCustomAttribute<PersistentAssetFilePathAttribute>();
            if (attribute == null)
                return null;
            if (attribute.AssetLoadedCallbackMethodName.IsNullOrEmpty())//方法为空：返回Null
                return null;

            Type objType = fieldInfoAttribute.DeclaringType;
            FieldInfo fieldInfoAsset = GetBindingAssetFieldInfo(fieldInfoAttribute);
            if (fieldInfoAsset == null)
                return null;

            Type assetType = fieldInfoAsset.FieldType;//AssetField的类型就是对应的参数类型
            MethodInfo methodInfo = ReflectionTool.GetMethod(objType, attribute.AssetLoadedCallbackMethodName);
            if (methodInfo != null && assetType != null)
            {
                ParameterInfo[] arrParameterInfo = methodInfo.GetParameters();

                if (methodInfo.ReturnType == assetType && arrParameterInfo.Length == 2)
                {
                    Type firstParamType = arrParameterInfo[0].ParameterType;
                    if (firstParamType.IsGenericType && firstParamType.GetGenericTypeDefinition() == typeof(ExternalResources.LoadResult<>) && firstParamType.GetGenericArguments().FirstOrDefault() == assetType && arrParameterInfo[1].ParameterType == typeof(PersistentChangeState))
                        return methodInfo;
                }
            }
            Debug.LogError($"Can't find matched method [{attribute.AssetLoadedCallbackMethodName}] in {objType.FullName}!");
            return null;
        }

        #endregion
    }

    /// <summary>
    /// Cache option list's selected index
    /// [valid for int field only]
    /// 
    /// Usage:Generate [DataOption_OptionInfo]
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class PersistentOptionAttribute : Attribute
    {
        public string ListOptionFieldName { get; set; }
        public string OptionTargetFieldName { get; set; }

        /// <summary>
        /// 
        /// ToUpdate:大于等于-1才有效（-1预留给全选，如果默认不选择任何选项，可以将初始值设置为-2）
        /// </summary>
        /// <param name="listOptionFieldName">List or Array field of options</param>
        /// <param name="optionTargetFieldName">Target option field to set when option index changed</param>
        public PersistentOptionAttribute(string listOptionFieldName, string optionTargetFieldName)
        {
            //Todo:listOptionFieldName支持IList，优点是不需要自行封装 DataOption_OptionInfo
            ListOptionFieldName = listOptionFieldName;
            OptionTargetFieldName = optionTargetFieldName;
        }
        //ToAdd:后期增加直接获取DataOption_OptionInfo属性的构造函数，参考PersistentAssetFilePathAttribute

        /// <summary>
        /// 
        /// 用途：RuntimeEdit
        /// </summary>
        /// <param name="fieldInfoAttribute"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public DataOption_OptionInfo GetDataOption_OptionInfo(FieldInfo fieldInfoAttribute, object obj)
        {
            DataOption_OptionInfo dataOption_OptionInfo = new DataOption_OptionInfo();
            var listOptionValue = ReflectionTool.GetFieldValue(obj, ListOptionFieldName);
            if (listOptionValue != null)
            {
                if (listOptionValue is IList listInst)//确保是List<T>或Array
                {
                    for (int i = 0; i != listInst.Count; i++)
                    {
                        DataOption_OptionInfo.OptionData optionData = new DataOption_OptionInfo.OptionData(i.ToString());
                        dataOption_OptionInfo.listOptionData.Add(optionData);
                    }
                }
                else
                {
                    Debug.LogError("The binding field is not IList: " + obj.GetType().FullName + "." + ListOptionFieldName);
                }
            }
            return dataOption_OptionInfo;
        }

        public void SetTargetOption(FieldInfo fieldInfoAttribute, object obj)
        {
            if (obj == null || fieldInfoAttribute == null || ListOptionFieldName == null || OptionTargetFieldName == null)
                return;

            //#1获取当前field的index
            int curIndex = (int)fieldInfoAttribute.GetValue(obj);

            //#2获取List对应的值
            var listOptionValue = ReflectionTool.GetFieldValue(obj, ListOptionFieldName);
            if (listOptionValue is IList listInst)
            {
                if (curIndex >= listInst.Count)
                {
                    Debug.LogError($"Index out of bounds! {curIndex + 1}>{listInst.Count}");
                }
                else
                {
                    object eleValue = listInst[curIndex];
                    FieldInfo fieldInfoOptionTarget = obj.GetType().GetField(OptionTargetFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (eleValue != null && fieldInfoOptionTarget != null &&
                    eleValue.GetType() == fieldInfoOptionTarget.FieldType)
                    {
                        //#3设置到target中
                        fieldInfoOptionTarget.SetValue(obj, eleValue);
                    }
                }
            }
        }

        public string GetBindingListOptionFieldValue(object obj)
        {
            FieldInfo fieldInfoPersistentDirPath = obj.GetType().GetFieldWithAttribute<PersistentDirPathAttribute>();//获取obj中唯一的Attribute
            if (fieldInfoPersistentDirPath != null)
                return fieldInfoPersistentDirPath.GetValue(obj) as string;
            return null;
        }
    }


    /// <summary>
    /// Invoke method when asset changed via [PersistentAssetFilePath]
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class PersistentAssetChangedAttribute : FieldCallabackAttributeBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="callbackMethodName">
        /// Callback method when field's value changed.
        /// Format: void OnPersistentChanged(T asset, PersistentChangeState persistentChangeState)
        /// </param>
        public PersistentAssetChangedAttribute(string callbackMethodName) : base(callbackMethodName) { }

        public void InvokeCallbackMethodInfo(FieldInfo fieldInfoAttribute, object obj, object value, PersistentChangeState persistentChangeState)
        {
            if (fieldInfoAttribute == null || obj == null)
                return;

            try
            {
                //绑定方法不为空则调用，用户可以在方法中修改值并返回新值；否则直接使用加载后的Asset
                MethodInfo methodInfoCallback = GetCallbackMethodInfo(fieldInfoAttribute);
                if (methodInfoCallback != null)
                {
                    methodInfoCallback.Invoke(obj, new object[] { value, persistentChangeState });
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("Invoke callback method error:\r\n" + e);
            }
        }

        public override MethodInfo GetCallbackMethodInfo(FieldInfo fieldInfoAttribute)
        {
            if (fieldInfoAttribute == null || CallbackMethodName.IsNullOrEmpty()) return null;

            Type objType = fieldInfoAttribute.DeclaringType;
            MethodInfo methodInfo = ReflectionTool.GetMethod(objType, CallbackMethodName);
            Type paramType = fieldInfoAttribute.FieldType;//该Field的类型就是对应的参数类型
            if (methodInfo != null && paramType != null)
            {
                ParameterInfo[] arrParameterInfo = methodInfo.GetParameters();
                if (methodInfo.ReturnType == typeof(void) && arrParameterInfo.Length == 2 && arrParameterInfo[0].ParameterType == paramType && arrParameterInfo[1].ParameterType == typeof(PersistentChangeState))
                    return methodInfo;
            }
            Debug.LogError($"Can't find matched method [{CallbackMethodName}] in {objType.FullName}!");
            return null;
        }
    }


    /// <summary>
    /// Invoke method when field's value changed
    /// ToUpdate: Add support for property
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class PersistentValueChangedAttribute : FieldCallabackAttributeBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="callbackMethodName">
        /// Callback method when field's value changed.
        /// Format: 
        /// #1 void OnPersistentValueChanged(T oldValue, T newValue, PersistentChangeState persistentChangeState)
        /// #2 void OnPersistentValueChanged(PersistentChangeState persistentChangeState)
        /// </param>
        public PersistentValueChangedAttribute(string callbackMethodName) : base(callbackMethodName) { }

        public void InvokeCallbackMethodInfo(FieldInfo fieldInfoAttribute, object obj, object oldValue, object newValue, PersistentChangeState persistentChangeState)
        {
            if (fieldInfoAttribute == null || obj == null)
                return;

            try
            {
                //绑定方法不为空则调用
                MethodInfo methodInfoCallback = GetCallbackMethodInfo(fieldInfoAttribute);
                if (methodInfoCallback != null)
                {
                    if (methodInfoCallback.GetParameters().Length == 3)
                        methodInfoCallback.Invoke(obj, new object[] { oldValue, newValue, persistentChangeState });//#1带值方法 用户可以在此方法中修改newValue并返回；否则直接使用加载后的Asset
                    else
                        methodInfoCallback.Invoke(obj, new object[] { persistentChangeState });//#2 不带值方法
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Invoke callback method error:\r\n" + e);
            }
        }

        public override MethodInfo GetCallbackMethodInfo(FieldInfo fieldInfoAttribute)
        {
            if (fieldInfoAttribute == null || CallbackMethodName.IsNullOrEmpty()) return null;

            Type objType = fieldInfoAttribute.DeclaringType;
            MethodInfo methodInfo = ReflectionTool.GetMethod(objType, CallbackMethodName);

            if (methodInfo != null)
            {
                ParameterInfo[] arrParameterInfo = methodInfo.GetParameters();
                if (arrParameterInfo.Length == 3)// #1 void OnPersistentValueChanged(T oldValue, T newValue, PersistentChangeState persistentChangeState)
                {
                    Type paramType = fieldInfoAttribute.FieldType;//该Field的类型就是对应的参数类型
                    if (paramType != null)
                    {
                        if (methodInfo.ReturnType == typeof(void) && arrParameterInfo[0].ParameterType == paramType && arrParameterInfo[1].ParameterType == paramType && arrParameterInfo[2].ParameterType == typeof(PersistentChangeState))
                            return methodInfo;
                    }
                }
                else if (arrParameterInfo.Length == 1)// #2 void OnPersistentValueChanged(PersistentChangeState persistentChangeState)
                {
                    if (methodInfo.ReturnType == typeof(void) && arrParameterInfo[0].ParameterType == typeof(PersistentChangeState))
                        return methodInfo;
                }
            }

            Debug.LogError($"Can't find matched method [{CallbackMethodName}] in {objType.FullName}!");
            return null;
        }
    }


    //——Class——

    /// <summary>
    /// Notify method when class instance changed (Get call after every field's attribute (eg: [PersistentValueChanged], [PersistentAssetChanged]))
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    public class PersistentChangedAttribute : Attribute
    {
        public string CallbackMethodName { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="callbackMethodName">
        /// Callback method when persistent changed.
        /// Format: void OnPersistentChanged(PersistentChangeState persistentChangeState)
        /// </param>
        public PersistentChangedAttribute(string callbackMethodName)
        {
            CallbackMethodName = callbackMethodName;
        }

        public void InvokeCallbackMethodInfo(object obj, PersistentChangeState persistentChangeState)
        {
            Type objType = obj.GetType();
            if (objType == null)
                return;

            MethodInfo methodInfoCallback = GetCallbackMethodInfo(objType);
            if (methodInfoCallback != null)
            {
                methodInfoCallback.Invoke(obj, new object[] { persistentChangeState });
            }
        }

        public static MethodInfo GetCallbackMethodInfo(Type objType)
        {
            if (objType == null)
                return null;
            PersistentChangedAttribute attribute = objType.GetCustomAttribute<PersistentChangedAttribute>();
            if (attribute == null)
                return null;
            if (attribute.CallbackMethodName.IsNullOrEmpty())
                return null;

            MethodInfo methodInfo = ReflectionTool.GetMethod(objType, attribute.CallbackMethodName);//PS:为了考虑继承，需要从自身及父类中查找相关方法
            if (methodInfo != null)
            {
                ParameterInfo[] arrParameterInfo = methodInfo.GetParameters();
                if (methodInfo.ReturnType == typeof(void) && arrParameterInfo.Length == 1 && arrParameterInfo[0].ParameterType == typeof(PersistentChangeState))
                    return methodInfo;
            }
            return null;
        }
    }
}
