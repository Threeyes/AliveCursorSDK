using System;
using System.Text;

#if USE_JsonDotNet
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
#endif
/// <summary>
/// 定义组件的必要序列化字段
/// 
/// PS：只是临时定义，后续等RuntimeSceneSerialization完善后就替代为他们的方案
/// </summary>
namespace Threeyes.RuntimeSerialization
{
    public interface IPropertyBag
    {
        string ContainerTypeName { get; }
    }
    [Serializable]
    [JsonObject(MemberSerialization.Fields)]
    public class PropertyBag : IPropertyBag
    {
        ///PS：可通过参数可在序列化时自动添加"$type"字段，但只能为该类的名称。(开启方式：https://www.codeproject.com/Articles/5284591/Adding-type-to-System-Text-Json-serialization-like)
        public string ContainerTypeName { get { return containerTypeName; } }//Container的类型（比较时可通过GetTypeName将传入类转成string，然后进行比较）(用于后续自行查找代码，需要保留)
        [UnityEngine.HideInInspector] protected string containerTypeName;

        public PropertyBag()
        {
        }
        //public virtual void AcceptBase(ref UnityEngine.Object container)
        //{

        //}

        #region Utility
        /// <summary>
        /// 功能：获取类型的名称（默认缩略名称）
        /// 
        /// Ref: Newtonsoft.Json.Utilities.ReflectionUtils
        /// </summary>
        /// <param name="t"></param>
        /// <param name="assemblyFormat"></param>
        /// <param name="binder"></param>
        /// <returns></returns>
        public static string GetTypeName(Type t, TypeNameAssemblyFormatHandling assemblyFormat = TypeNameAssemblyFormatHandling.Simple, ISerializationBinder binder = null)
        {
            string fullyQualifiedTypeName = GetFullyQualifiedTypeName(t, binder);
            switch (assemblyFormat)
            {
                case TypeNameAssemblyFormatHandling.Simple:
                    return RemoveAssemblyDetails(fullyQualifiedTypeName);
                case TypeNameAssemblyFormatHandling.Full:
                    return fullyQualifiedTypeName;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        private static string GetFullyQualifiedTypeName(Type t, ISerializationBinder binder = null)
        {
            if (binder != null)
            {
                binder!.BindToName(t, out string assemblyName, out string typeName);
                return typeName + ((assemblyName == null) ? "" : (", " + assemblyName));
            }

            return t.AssemblyQualifiedName;
        }
        private static string RemoveAssemblyDetails(string fullyQualifiedTypeName)
        {
            StringBuilder stringBuilder = new StringBuilder();
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            foreach (char c in fullyQualifiedTypeName)
            {
                switch (c)
                {
                    case '[':
                        flag = false;
                        flag2 = false;
                        flag3 = true;
                        stringBuilder.Append(c);
                        break;
                    case ']':
                        flag = false;
                        flag2 = false;
                        flag3 = false;
                        stringBuilder.Append(c);
                        break;
                    case ',':
                        if (flag3)
                        {
                            stringBuilder.Append(c);
                        }
                        else if (!flag)
                        {
                            flag = true;
                            stringBuilder.Append(c);
                        }
                        else
                        {
                            flag2 = true;
                        }

                        break;
                    default:
                        flag3 = false;
                        if (!flag2)
                        {
                            stringBuilder.Append(c);
                        }

                        break;
                }
            }
            return stringBuilder.ToString();
        }
        #endregion
    }

    /// <summary>
    /// 适用于任意数据类
    /// </summary>
    /// <typeparam name="TContainer"></typeparam>
    [Serializable]
    public class PropertyBag<TContainer> : PropertyBag
          where TContainer : class
    {
        public PropertyBag()
        {
        }

        /// <summary>
        /// Save Comp info
        /// </summary>
        /// <param name="container"></param>
        public virtual void Init(TContainer container)
        {
            //PS：需要保证会被调用到
            containerTypeName = GetTypeName(typeof(TContainer));
        }
        //public override void AcceptBase(ref UnityEngine.Object container)
        //{
        //    TContainer containerReal = container as TContainer;
        //    Accept(ref containerReal);
        //}
        /// <summary>
        /// Restore Comp info
        /// </summary>
        /// <param name="container"></param>
        public virtual void Accept(ref TContainer container)
        {
        }
    }
}