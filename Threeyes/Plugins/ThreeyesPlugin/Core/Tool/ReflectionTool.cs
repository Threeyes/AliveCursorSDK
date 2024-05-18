using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Threeyes.Core;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Threeyes.Core
{
    /// <summary>
    /// 反射克隆类的工具
    /// Ref: Cinemachine
    /// 
    /// Todo：
    /// -参考其他实现进行优化：https://github.com/Burtsev-Alexey/net-object-deep-copy/tree/master
    /// </summary>
    public static class ReflectionTool
    {
        public static Type[] GetGenericInterfaceArgumentTypes(Type objType, Type genericInterfaceType)
        {
            Type[] arrArgument = new Type[] { };
            Type interfaceType = GetGenericInterfaceType(objType, genericInterfaceType);
            if (interfaceType != null)
            {
                arrArgument = interfaceType.GetGenericArguments();
            }
            return arrArgument;
        }
        public static Type GetGenericInterfaceType(Type objType, Type genericInterfaceType)
        {
            if (objType == null)
                return null;

            return objType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericInterfaceType);
        }

        /// <summary>
        /// 获取继承IList（如Array、IList<>）的对应元素类型
        /// </summary>
        /// <param name="type">type that inheric from IList</param>
        /// <returns></returns>
        public static Type GetCollectionElementType(Type type)
        {
            if (type.IsInherit(typeof(IList)))
            {
                if (type.IsArray)
                    return type.GetElementType();
                else
                {
                    Type genericIListType = GetGenericInterfaceType(type, typeof(IList<>));
                    if (genericIListType != null)
                    {
                        return genericIListType.GetGenericArguments().FirstOrDefault();
                    }
                }
            }
            return null;
        }

        /// <summary>Copy the fields from one object to another</summary>
        /// <param name="src">The source object to copy from</param>
        /// <param name="dst">The destination object to copy to</param>
        /// <param name="bindingAttr">The mask to filter the attributes.
        /// <paramref name="funcCopyFilter"/>Decide whethre the field should copy<paramref name="funcCopyFilter"/>
        /// Only those fields that get caught in the filter will be copied</param>
        public static void CopyFields(object src, object dst, BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, Func<Type, MemberInfo, bool> funcCopyFilter = null)
        {
            if (src != null && dst != null)
            {
                Type type = src.GetType();
                FieldInfo[] fields = type.GetFields(bindingAttr);

                Func<Type, MemberInfo, bool> customFilter = null;
                if (dst is ICopyFilter copyFilter)//自定义的筛选器
                    customFilter = copyFilter.ShouldCopy;

                for (int i = 0; i < fields.Length; ++i)
                    if (!fields[i].IsStatic)
                    {
                        if (customFilter != null)//自定义的筛选器
                            if (!customFilter(type, fields[i]))
                                continue;

                        if (funcCopyFilter != null && !funcCopyFilter(type, fields[i]))//忽略不能复制的
                            continue;

                        fields[i].SetValue(dst, fields[i].GetValue(src));
                    }
            }
        }

        #region Get Value
        public static object GetFieldOrPropertyValue(object obj, string fieldOrPropertyName, Type targetType = null, object defaultValue = null)
        {
            object value = GetFieldValue(obj, fieldOrPropertyName, targetType, defaultValue);
            if (value == null)
                value = GetPropertyValue(obj, fieldOrPropertyName, targetType, defaultValue);
            return value;
        }
        public static T GetFieldValue<T>(object obj, string propertyName, T defaultValue = null)
           where T : class
        {
            return GetFieldValue(obj, propertyName, typeof(T), defaultValue) as T;
        }
        public static object GetFieldValue(object obj, string fieldName, Type targetType = null, object defaultValue = null)
        {
            //ToAdd:针对是否需要Instance字段进行处理（如GetValue时传入的值、
            object result = defaultValue;
            if (obj == null || fieldName.IsNullOrEmpty())
                return result;

            Type objType = obj.GetType();
            FieldInfo fieldInfo = objType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (fieldInfo != null)
            {
                if (!(targetType != null && !fieldInfo.FieldType.IsInherit(targetType)))//ToTest
                    result = fieldInfo.GetValue(obj);
            }
            return result != null ? result : defaultValue;
        }

        public static T GetPropertyValue<T>(object obj, string propertyName, T defaultValue = null)
             where T : class
        {
            return GetPropertyValue(obj, propertyName, typeof(T), defaultValue) as T;
        }

        /// <summary>
        /// 获取Property值，支持引用类型（因为IsInherit）及值类型
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <param name="targetType">确认</param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static object GetPropertyValue(object obj, string propertyName, Type targetType = null, object defaultValue = null)
        {
            object result = defaultValue;
            if (obj == null || propertyName.IsNullOrEmpty())
                return result;

            //ToAdd
            Type objType = obj.GetType();
            PropertyInfo propertyInfo = objType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (propertyInfo != null && propertyInfo.CanRead)
            {
                if (propertyInfo.PropertyType.IsValueType)//值类型：直接获取对应值
                {
                    if (!(targetType != null && propertyInfo.PropertyType != targetType))//如果propertyType不为空，则确认类型是否相同
                        result = propertyInfo.GetValue(obj);
                }
                else
                {
                    if (propertyInfo.PropertyType.IsInherit(targetType))//引用类型：确认是否为子类
                    {
                        if (!(targetType != null && !propertyInfo.PropertyType.IsInherit(targetType)))
                            result = propertyInfo.GetValue(obj);
                    }
                }
            }
            return result != null ? result : defaultValue;
        }

        #endregion

        #region  GetInfo 【优先使用】【Ref: NaughtAttribute】
        //——从自身及父类的某个匹配Info——
        public static List<bool> GetConditionValues(object target, string[] conditions)
        {
            Type objType = target.GetType();
            List<bool> conditionValues = new List<bool>();
            foreach (var condition in conditions)
            {

                FieldInfo conditionField = GetField(objType, condition);
                if (conditionField != null &&
                    conditionField.FieldType == typeof(bool))
                {
                    conditionValues.Add((bool)conditionField.GetValue(target));
                }

                PropertyInfo conditionProperty = GetProperty(objType, condition);
                if (conditionProperty != null &&
                    conditionProperty.PropertyType == typeof(bool))
                {
                    conditionValues.Add((bool)conditionProperty.GetValue(target));
                }

                MethodInfo conditionMethod = GetMethod(objType, condition);
                if (conditionMethod != null &&
                    conditionMethod.ReturnType == typeof(bool) &&
                    conditionMethod.GetParameters().Length == 0)
                {
                    conditionValues.Add((bool)conditionMethod.Invoke(target, null));
                }
            }

            return conditionValues;
        }
        public static Enum GetEnumValue(object target, string enumName)
        {
            Type objType = target.GetType();
            FieldInfo enumField = GetField(objType, enumName);
            if (enumField != null && enumField.FieldType.IsSubclassOf(typeof(Enum)))
            {
                return (Enum)enumField.GetValue(target);
            }

            PropertyInfo enumProperty = GetProperty(objType, enumName);
            if (enumProperty != null && enumProperty.PropertyType.IsSubclassOf(typeof(Enum)))
            {
                return (Enum)enumProperty.GetValue(target);
            }

            MethodInfo enumMethod = GetMethod(objType, enumName);
            if (enumMethod != null && enumMethod.ReturnType.IsSubclassOf(typeof(Enum)))
            {
                return (Enum)enumMethod.Invoke(target, null);
            }

            return null;
        }
        public static FieldInfo GetField(Type objType, string fieldName)
        {
            return GetAllFields(objType, f => f.Name.Equals(fieldName, StringComparison.Ordinal)).FirstOrDefault();
        }

        public static PropertyInfo GetProperty(Type objType, string propertyName)
        {
            return GetAllProperties(objType, p => p.Name.Equals(propertyName, StringComparison.Ordinal)).FirstOrDefault();
        }

        public static MethodInfo GetMethod(Type objType, string methodName)
        {
            return GetAllMethods(objType, m => m.Name.Equals(methodName, StringComparison.Ordinal)).FirstOrDefault();
        }

        public static IEnumerable<FieldInfo> GetAllFields(Type objType, Func<FieldInfo, bool> predicate)
        {
            if (objType == null)
            {
                Debug.LogError("The target object type is null. Check for missing scripts.");
                yield break;
            }

            List<Type> types = GetSelfAndBaseTypes(objType);

            for (int i = types.Count - 1; i >= 0; i--)
            {
                IEnumerable<FieldInfo> fieldInfos = types[i]
                    .GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(predicate);

                foreach (var fieldInfo in fieldInfos)
                {
                    yield return fieldInfo;
                }
            }
        }
        public static IEnumerable<PropertyInfo> GetAllProperties(Type objType, Func<PropertyInfo, bool> predicate)
        {
            if (objType == null)
            {
                Debug.LogError("The target object type is null. Check for missing scripts.");
                yield break;
            }

            List<Type> types = GetSelfAndBaseTypes(objType);

            for (int i = types.Count - 1; i >= 0; i--)
            {
                IEnumerable<PropertyInfo> propertyInfos = types[i]
                    .GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)//DeclaredOnly:指定只考虑在所提供类型的层次结构级别上声明的成员。不考虑继承的成员。
                    .Where(predicate);

                foreach (var propertyInfo in propertyInfos)
                {
                    yield return propertyInfo;
                }
            }
        }
        public static IEnumerable<MethodInfo> GetAllMethods(Type objType, Func<MethodInfo, bool> predicate)
        {
            if (objType == null)
            {
                Debug.LogError("The target object type is null. Check for missing scripts.");
                yield break;
            }

            List<Type> types = GetSelfAndBaseTypes(objType);

            for (int i = types.Count - 1; i >= 0; i--)
            {
                IEnumerable<MethodInfo> methodInfos = types[i]
                    .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(predicate);

                foreach (var methodInfo in methodInfos)
                {
                    yield return methodInfo;
                }
            }
        }
        /// <summary>
        ///		Get type and all base types of target, sorted as following:
        ///		<para />[target's type, base type, base's base type, ...]
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private static List<Type> GetSelfAndBaseTypes(Type objType)
        {
            List<Type> types = new List<Type>() { objType };

            while (types.Last().BaseType != null)
            {
                types.Add(types.Last().BaseType);
            }

            return types;
        }

        #endregion


        #region GetInfo

        /// <summary>
        /// Get Unique path for fieldInfo (Mainly for marking the fieldInfo from nested classes)
        /// </summary>
        /// <param name="parentChain"></param>
        /// <param name="fieldInfo"></param>
        /// <param name="indexInCollectionValue">the index in list (if in list)</param>
        /// <returns></returns>
        public static string GetFieldPathChain(string parentChain, FieldInfo fieldInfo, int? indexInCollectionValue = null)
        {
            string result = "";
            if (parentChain.NotNullOrEmpty())
                result = parentChain + ".";
            result += fieldInfo.Name;
            if (indexInCollectionValue != null)
            {
                result += $"[{indexInCollectionValue.Value}]";
            }
            return result;
        }
        #endregion

        /// <summary>Search the assembly for all types that match a predicate</summary>
        /// <param name="assembly">The assembly to search</param>
        /// <param name="predicate">The type to look for</param>
        /// <returns>A list of types found in the assembly that inherit from the predicate</returns>
        public static IEnumerable<Type> GetTypesInAssembly(Assembly assembly, Predicate<Type> predicate)
        {
            if (assembly == null)
                return null;

            Type[] types = new Type[0];
            try
            {
                types = assembly.GetTypes();
            }
            catch (Exception)
            {
                // Can't load the types in this assembly
            }
            types = (from t in types
                     where t != null && predicate(t)
                     select t).ToArray();
            return types;
        }

        /// <summary>
        ///  Get a type from a name
        ///  仅匹配名字，不匹配全名（容易导致混淆，获取到错误的重名）
        /// </summary>
        /// <param name="typeName">The name of the type to search for</param>
        /// <returns>The type matching the name, or null if not found</returns>
        public static Type GetTypeInAllLoadedAssemblies(string typeName)
        {
            foreach (Type type in GetTypesInAllLoadedAssemblies(t => t.Name == typeName))
                return type;
            return null;
        }

        /// <summary>Search all assemblies for all types that match a predicate</summary>
        /// <param name="predicate">The type to look for</param>
        /// <returns>A list of types found in the assembly that inherit from the predicate</returns>
        public static IEnumerable<Type> GetTypesInAllLoadedAssemblies(Predicate<Type> predicate)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<Type> foundTypes = new List<Type>(100);
            foreach (Assembly assembly in assemblies)
            {
                foreach (Type foundType in GetTypesInAssembly(assembly, predicate))
                    foundTypes.Add(foundType);
            }
            return foundTypes;
        }

        /// <summary>call GetTypesInAssembly() for all assemblies that match a predicate</summary>
        /// <param name="assemblyPredicate">Which assemblies to search</param>
        /// <param name="predicate">What type to look for</param>
        public static IEnumerable<Type> GetTypesInLoadedAssemblies(
            Predicate<Assembly> assemblyPredicate, Predicate<Type> predicate)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            assemblies = assemblies.Where((assembly)
                    =>
            { return assemblyPredicate(assembly); }).OrderBy((ass)
         =>
            { return ass.FullName; }).ToArray();

            List<Type> foundTypes = new List<Type>(100);
            foreach (Assembly assembly in assemblies)
            {
                foreach (Type foundType in GetTypesInAssembly(assembly, predicate))
                    foundTypes.Add(foundType);
            }

            return foundTypes;
        }

        /// <summary>Is a type defined and visible</summary>
        /// <param name="fullname">Fullly-qualified type name</param>
        /// <returns>true if the type exists</returns>
        public static bool TypeIsDefined(string fullname)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                        if (type.FullName == fullname)
                            return true;
                }
                catch (Exception) { } // Just skip uncooperative assemblies
            }
            return false;
        }

        /// <summary>Cheater extension to access internal field of an object</summary>
        /// <param name="type">The type of the field</param>
        /// <param name="obj">The object to access</param>
        /// <param name="memberName">The string name of the field to access</param>
        /// <returns>The value of the field in the objects</returns>
        public static T AccessInternalField<T>(this Type type, object obj, string memberName)
        {
            if (string.IsNullOrEmpty(memberName) || type == null)
                return default;

            BindingFlags bindingFlags = BindingFlags.NonPublic;
            if (obj != null)
                bindingFlags |= BindingFlags.Instance;
            else
                bindingFlags |= BindingFlags.Static;

            FieldInfo field = type.GetField(memberName, bindingFlags);
            if (field != null && field.FieldType == typeof(T))
                return (T)field.GetValue(obj);
            else
                return default;
        }

        /// <summary>Get the object owner of a field.  This method processes
        /// the '.' separator to get from the object that owns the compound field
        /// to the object that owns the leaf field</summary>
        /// <param name="path">The name of the field, which may contain '.' separators</param>
        /// <param name="obj">the owner of the compound field</param>
        public static object GetParentObject(string path, object obj)
        {
            var fields = path.Split('.');
            if (fields.Length == 1)
                return obj;

            var info = obj.GetType().GetField(
                    fields[0], BindingFlags.Public
                        | BindingFlags.NonPublic
                        | BindingFlags.Instance);
            obj = info.GetValue(obj);

            return GetParentObject(string.Join(".", fields, 1, fields.Length - 1), obj);
        }

        /// <summary>Returns a string path from an expression - mostly used to retrieve serialized properties
        /// without hardcoding the field path. Safer, and allows for proper refactoring.</summary>
        public static string GetFieldPath<TType, TValue>(Expression<Func<TType, TValue>> expr)
        {
            MemberExpression me;
            switch (expr.Body.NodeType)
            {
                case ExpressionType.MemberAccess:
                    me = expr.Body as MemberExpression;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            var members = new List<string>();
            while (me != null)
            {
                members.Add(me.Member.Name);
                me = me.Expression as MemberExpression;
            }

            var sb = new StringBuilder();
            for (int i = members.Count - 1; i >= 0; i--)
            {
                sb.Append(members[i]);
                if (i > 0) sb.Append('.');
            }
            return sb.ToString();
        }


        /// <summary>
        /// Create instance for given type
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public static TValue CreateInstance<TValue>()
        {
            return (TValue)CreateInstance(typeof(TValue));
        }
        public static object CreateInstance(Type type)
        {
            object valueInst = null;
            if (type != null)
            {
                try
                {

                    if (type == typeof(string))
                        valueInst = "";
                    else
                        valueInst = Activator.CreateInstance(type);//创建一个新的非空值，适用于VaueType及ClassType
                }
                catch (Exception e)
                {
                    Debug.LogError($"CreateInstance for type [{type}] failed!\r\n" + e);
                }
            }
            return valueInst;
        }


        static readonly MethodInfo CloneMethod = typeof(object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);

        //DeepCopy （https://stackoverflow.com/questions/39092168/c-sharp-copying-unityevent-information-using-reflection）

        /// <summary>
        /// Perform a deep copy of the class.
        /// 
        /// PS：兼容UnityObject
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The object.</param>
        /// <returns>A deep copy of obj.</returns>
        /// <exception cref="ArgumentNullException">Object cannot be null</exception>
        public static T DeepCopy<T>(T obj)
        {
            if (obj == null)
            {
                Debug.LogError("Object cannot be null");
                return default;
            }
            return (T)DoCopy(obj);
        }

        #region Constructor 【ToTest】(Ref: Newtonsoft.Json.Utilities.ExpressionReflectionDelegateFactory[MIT])
        public static bool HasDefaultConstructor(Type t, bool nonPublic = true)//检查是否有默认构造函数
        {
            ValidationUtils.ArgumentNotNull(t, nameof(t));

            if (t.IsValueType)
            {
                return true;
            }

            return (GetDefaultConstructor(t, nonPublic) != null);
        }
        public static ConstructorInfo GetDefaultConstructor(Type t, bool nonPublic)
        {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public;
            if (nonPublic)
            {
                bindingFlags = bindingFlags | BindingFlags.NonPublic;
            }

            return t.GetConstructors(bindingFlags).SingleOrDefault(c => !c.GetParameters().Any());//获取首个无参数构造函数
        }
        public static Func<object> CreateDefaultConstructor(Type type)
        {
            return CreateDefaultConstructor<object>(type);
        }
        public static Func<T> CreateDefaultConstructor<T>(Type type)
        {
            ValidationUtils.ArgumentNotNull(type, "type");

            // avoid error from expressions compiler because of abstract class
            if (type.IsAbstract)
            {
                return () => (T)Activator.CreateInstance(type)!;
            }

            try
            {
                Type resultType = typeof(T);

                Expression expression = Expression.New(type);

                expression = EnsureCastExpression(expression, resultType);

                LambdaExpression lambdaExpression = Expression.Lambda(typeof(Func<T>), expression);

                Func<T> compiled = (Func<T>)lambdaExpression.Compile();
                return compiled;
            }
            catch
            {
                // an error can be thrown if constructor is not valid on Win8
                // will have INVOCATION_FLAGS_NON_W8P_FX_API invocation flag
                return () => (T)Activator.CreateInstance(type)!;
            }
        }

        static Expression EnsureCastExpression(Expression expression, Type targetType, bool allowWidening = false)
        {
            Type expressionType = expression.Type;

            // check if a cast or conversion is required
            if (expressionType == targetType || (!expressionType.IsValueType && targetType.IsAssignableFrom(expressionType)))
            {
                return expression;
            }

            if (targetType.IsValueType)
            {
                Expression convert = Expression.Unbox(expression, targetType);

                if (allowWidening && targetType.IsPrimitive())
                {
                    MethodInfo toTargetTypeMethod = typeof(Convert).GetMethod("To" + targetType.Name, new[] { typeof(object) });

                    if (toTargetTypeMethod != null)
                    {
                        convert = Expression.Condition(Expression.TypeIs(expression, targetType), convert, Expression.Call(toTargetTypeMethod, expression));
                    }
                }

                return Expression.Condition(Expression.Equal(expression, Expression.Constant(null, typeof(object))), Expression.Default(targetType), convert);
            }

            return Expression.Convert(expression, targetType);
        }
        //——Utility——
        internal static class ValidationUtils
        {
            public static void ArgumentNotNull(/*[NotNull]*/object value, string parameterName)
            {
                if (value == null)
                {
                    Debug.LogError($"{parameterName} is null!");
                }
            }
        }
        #endregion

        /// <summary>
        /// Does the copy.
        /// 克隆所有必要字段
        /// 
        /// PS：
        /// -（与UnityObjectTool.DoCopy相比）：可克隆所有的字段（ToAdd：Action），适用于整个数据类进行复制）
        /// 
        /// Todo：
        /// -优化参考（https://github.com/Burtsev-Alexey/net-object-deep-copy）：
        ///     -创建类：通过MemberwiseClone实现
        ///     -剔除Delegate等类
        /// -参考Newtown.Json
        ///     -创建类：ExpressionReflectionDelegateFactory.CreateDefaultConstructor
        /// Bug:
        /// -尚未针对Action等进行处理，建议改用UnityObjectTool
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Unknown type</exception>
        public static object DoCopy(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            // Value type
            var type = obj.GetType();
            if (type.IsValueType || type == typeof(string))
            {
                return obj;
            }

            // Array
            if (type.IsArray)
            {
                Type elementType = type.GetElementType();
                var array = obj as Array;
                Array copied = Array.CreateInstance(elementType, array.Length);
                for (int i = 0; i < array.Length; i++)
                {
                    copied.SetValue(DoCopy(array.GetValue(i)), i);
                }
                return Convert.ChangeType(copied, obj.GetType());
            }

            if (typeof(Delegate).IsAssignableFrom(type))//忽略委托
                return null;

            // Unity Object的子类：返回原引用，避免引用丢失
            if (type.IsInherit(typeof(UnityEngine.Object)))
            {
                return obj;
            }

            // Class -> Recursion
            if (type.IsClass)
            {
                //PS：目前仅支持默认构造函数
                //Todo:要判断是否有默认结构，否则会报错（需要忽略Action、UnityAction等）
                object copy = null;

                if (TryCopyUnityEngineSpeicalType(obj, ref copy))//尝试克隆Unity定义的特殊类（不继承UnityEngine.Object）
                {
                    return copy;
                }

                try
                {
                    //#1 克隆引用（更优，UnityObjectTool应参考该实现）
                    if (HasDefaultConstructor(type))
                        copy = Activator.CreateInstance(obj.GetType());//Warnging：需要该类有对应的无参构造函数，否则会报错（包括根object）
                    else
                        copy = CloneMethod.Invoke(obj, null);//使用反射来调用protected方法

                    //#2 复制字段
                    var fields = type.GetAllFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    foreach (FieldInfo field in fields)
                    {
                        var fieldValue = field.GetValue(obj);
                        if (fieldValue != null)
                        {
                            field.SetValue(copy, DoCopy(fieldValue));
                        }
                    }
                }
                catch (MissingMemberException mme)
                {
                    Debug.LogError($"找不到 [{type}] 对应的无参构造函数！\r\n" + mme);//
                }
                catch (Exception e)
                {
                    Debug.LogError("DoCopy 出错！\r\n" + e);
                }
                return copy;
            }

            // Fallback
            else
            {
                throw new ArgumentException("Unknown type");
            }
        }

        /// <summary>
        /// 克隆部分UnityEngine命名空间定义的特殊类
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="type"></param>
        /// <param name="result"></param>
        /// <returns>是否克隆成功</returns>
        static bool TryCopyUnityEngineSpeicalType(object obj, ref object result)
        {
            Type type = obj.GetType();
            //PS：AnimationCurve等会包含m_Ptr，直接拷贝拷贝该字段会导致引用相同而闪退！因此需要重新创建
            if (obj is AnimationCurve animationCurve)
            {
                result = new AnimationCurve(animationCurve.keys);
                return true;
            }
            return false;
        }


        static List<string> listIgnoreFieldTypeName
      = new List<string>()
      {
            //忽略delegate（因为委托是与实例绑定的，拷贝后会导致SO的Action被覆盖掉）

            //以下有可能是泛型，因为通过名称判断
            "System.Action",
            "UnityEngine.Events.UnityAction",
            "Dictionary",

          //event对应EventInfo暂时不考虑，后续可加上
      };
        //static bool IsTypeCopiable(FieldInfo fieldInfo)
        //{
        //    if (fieldInfo == null)
        //        return false;
        //    Type fieldType = fieldInfo.FieldType;

        //    //#忽略Object中声明的字段，如name、m_InstanceID等，同时避免拷贝m_InstanceID导致指向同一个实例（PS：不能整合到IsTypeCopiable中，否则会导致DeepInstantiate失败）
        //    if (fieldInfo.DeclaringType == UnityObjectType)
        //        return false;

        //    return
        //        !listIgnoreFieldTypeName.Any((s) => fieldType.ToString().Contains(s)) &&
        //        !listIgnoreFieldType.Any(t => t == fieldType);
        //}


#if false
        /// <summary>Cheater extension to access internal property of an object</summary>
        /// <param name="type">The type of the field</param>
        /// <param name="obj">The object to access</param>
        /// <param name="memberName">The string name of the field to access</param>
        /// <returns>The value of the field in the objects</returns>
        public static T AccessInternalProperty<T>(this Type type, object obj, string memberName)
        {
            if (string.IsNullOrEmpty(memberName) || (type == null))
                return default(T);

            System.Reflection.BindingFlags bindingFlags = System.Reflection.BindingFlags.NonPublic;
            if (obj != null)
                bindingFlags |= System.Reflection.BindingFlags.Instance;
            else
                bindingFlags |= System.Reflection.BindingFlags.Static;

            PropertyInfo pi = type.GetProperty(memberName, bindingFlags);
            if ((pi != null) && (pi.PropertyType == typeof(T)))
                return (T)pi.GetValue(obj, null);
            else
                return default(T);
        }

       
    //public static void CopyPropertiesTo<T, TU>(this T source, TU dest)
    //{
    //    var sourceProps = typeof(T).GetProperties().Where(x => x.CanRead).ToList();
    //    var destProps = typeof(TU).GetProperties()
    //            .Where(x => x.CanWrite)
    //            .ToList();

    //    foreach (var sourceProp in sourceProps)
    //    {
    //        if (destProps.Any(x => x.Name == sourceProp.Name))
    //        {
    //            var p = destProps.First(x => x.Name == sourceProp.Name);
    //            if (p.CanWrite)
    //            { // check if the property can be set or no.
    //                p.SetValue(dest, sourceProp.GetValue(source, null), null);
    //            }
    //        }
    //    }
    //}
#endif
    }
    public static class ReflectionToolExtension
    {
        #region Copy

        public static T DeepCopyReflection<T>(this T other)
        {
            return ReflectionTool.DeepCopy(other);
        }

        #endregion

        #region Type
        public static bool IsInheritOrEqual(this Type objType, Type targetType)
        {
            if (targetType == null)
                return false;
            if (objType == targetType)
                return true;

            return targetType.IsAssignableFrom(objType);
        }
        public static bool IsInherit(this Type objType, Type targetType)
        {
            if (targetType == null)
                return false;
            return targetType.IsAssignableFrom(objType);
        }

        public static FieldInfo GetFieldWithAttribute<TAttribute, TFieldType>(this Type objType, string name)
        where TAttribute : Attribute
        {
            FieldInfo fieldInfo = GetFieldWithAttribute<TAttribute>(objType, name);
            if (fieldInfo == null)
                return null;
            if (fieldInfo.FieldType != typeof(TFieldType))
                return null;
            return fieldInfo;
        }
        public static FieldInfo GetFieldWithAttribute<TAttribute>(this Type objType, string name)
            where TAttribute : Attribute
        {
            if (objType == null)
                return null;

            FieldInfo fieldInfo = objType.GetField(name);
            if (fieldInfo == null)
                return null;

            var targetAttribute = fieldInfo.GetCustomAttribute<TAttribute>();
            if (targetAttribute == null)
                return null;

            return fieldInfo;
        }
        public static FieldInfo GetFieldWithAttribute<TAttribute>(this Type objType)
               where TAttribute : Attribute
        {
            if (objType == null)
                return null;

            foreach (FieldInfo fi in objType.GetFields())
            {
                if (fi.GetCustomAttribute<TAttribute>() != null)
                    return fi;
            }
            return null;
        }

        public static MethodInfo GetMethod(this Type type, string methodName, BindingFlags flags, bool isGenericMethod = false, Type[] argTypes = null)
        {
            return type.GetMethods(flags).FirstOrDefault(
                mI =>
             {
                 bool isMatch = mI.Name == methodName;
                 if (isGenericMethod)
                     isMatch &= mI.IsGenericMethod;

                 if (argTypes != null)//判断参数是否相同
                 {
                     var curMethodInfoParams = mI.GetParameters();
                     if (curMethodInfoParams.Length != argTypes.Length)
                         return false;

                     for (int i = 0; i < argTypes.Length; i++)
                     {
                         if (curMethodInfoParams[i].ParameterType != argTypes[i])
                             return false;
                     }
                 }


                 return isMatch;
             });
        }
        /// <summary>
        /// Gets all fields from an instance and its hierarchy inheritance （Except System.Object).
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="flags">The flags.</param>
        /// <returns>All fields of the type.</returns>
        public static List<FieldInfo> GetAllFields(this Type type, BindingFlags flags)
        {
            return type.GetAllObjectElement(flags, (t, bF) => t.GetFields(bF));
        }

        public static List<PropertyInfo> GetAllPropertys(this Type type, BindingFlags flags)
        {
            return type.GetAllObjectElement(flags, (t, bF) => t.GetProperties(bF));
        }
        public static List<MemberInfo> GetAllMembers(this Type type, BindingFlags flags)
        {
            return type.GetAllObjectElement(flags, (t, bF) => t.GetMembers(bF));
        }
        public static List<MethodInfo> GetAllMethods(this Type type, BindingFlags flags)
        {
            return type.GetAllObjectElement(flags, (t, bF) => t.GetMethods(bF));
        }
        static List<TMemberInfo> GetAllObjectElement<TMemberInfo>(this Type type, BindingFlags flags, Func<Type, BindingFlags, TMemberInfo[]> actGetMembers)
        {
            // Early exit if Object type
            if (type == typeof(System.Object))
            {
                return new List<TMemberInfo>();
            }

            // Recursive call(找到基类所有的指定元素）
            var fields = type.BaseType.GetAllObjectElement(flags, actGetMembers);
            TMemberInfo[] arrFI = actGetMembers(type, flags | BindingFlags.DeclaredOnly);
            fields.AddRange(arrFI);
            return fields;
        }

        #endregion

        #region MemberInfo

        //Ref: http://www.java2s.com/example/csharp/system.reflection/get-actual-type-from-memberinfo.html
        public static Type GetActualType(this MemberInfo methodInfo)//ToTest
        {
            Type rawType;
            if (!(methodInfo is Type))
            {
                rawType = methodInfo.GetVariableType();
            }
            else
            {
                rawType = (Type)methodInfo;
            }

            Type memberType;
            if (rawType.IsArray)
            {
                memberType = rawType.GetElementType();
                if (memberType == null)
                {
                    throw new Exception(string.Format("Unable to get Type of Array {0} ({1}).", rawType, methodInfo.GetFullMemberName()));
                }
            }
            else
            {
                memberType = rawType;
            }
            return memberType;
        }
        /// <summary>
        /// Find the real type of MemberInfo, get value's type
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        public static Type GetVariableType(this MemberInfo methodInfo)
        {
            if (methodInfo != null)
            {
                if (methodInfo is FieldInfo)
                {
                    return ((FieldInfo)methodInfo).FieldType;
                }
                if (methodInfo is PropertyInfo)
                {
                    return ((PropertyInfo)methodInfo).PropertyType;
                }
                else
                {
                    UnityEngine.Debug.LogError("Can only get VariableType of Fields and Properties!");
                }
            }
            return null;
        }
        public static string GetFullMemberName(this MemberInfo member)//ToTest
        {
            var str = member.DeclaringType.FullName + "." + member.Name;
            if (member is MethodInfo)
            {
                str += "(" + ((MethodInfo)member).GetParameters().Select(param => param.ParameterType.Name).ConnectToString(", ") + ")";
            }
            return str;
        }

        #endregion

        #region MethodInfo

        public static string GetUniqueID(this MethodInfo methodInfo, Type objType)
        {
            //https://stackoverflow.com/questions/11193616/how-to-get-a-unique-id-for-a-method-based-on-its-signature-in-c
            return objType.FullName + "."
                + methodInfo.Name + " "
                + methodInfo.ReturnType + "(" +
                methodInfo.GetParameters().ConnectToString(",") + ")";
        }

        #endregion

        #region Utility

        public static void ForEachMember<TAttribute>(this Type type, BindingFlags flags, Action<object> act)
            where TAttribute : Attribute
        {

        }

        public static void ForEachMemberWithInterface<TInterface>(this object obj, Action<TInterface> act, bool isRecursive = true, bool includeSelf = true, int maxDepth = 7)
        {
            if (obj == null)
                return;

            if (includeSelf)
                if (obj is TInterface inst)
                    act.Execute(inst);

            if (maxDepth == -1)
                return;
            //PS:功能比较复杂，不适合使用AlgorithmTool.Recursive

            foreach (FieldInfo fieldInfo in obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (!fieldInfo.FieldType.IsClass)
                    continue;
                object objField = fieldInfo.GetValue(obj);
                if (objField == null)
                    continue;

                if (isRecursive)
                    objField.ForEachMemberWithInterface(act, true, true, --maxDepth);
                else
                {
                    if (objField is TInterface inst)
                        act.Execute(inst);
                }
            }
        }

        #endregion
    }
}
