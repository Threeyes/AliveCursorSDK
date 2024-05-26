using System;
using System.Collections.Generic;
using System.Linq;
using Threeyes.Core;
using Threeyes.IO;
using Threeyes.Decoder;
using UnityEngine;
using System.Reflection;
#if USE_JsonDotNet
using Newtonsoft.Json;
#endif

#if USE_NaughtyAttributes
using NaughtyAttributes;
#endif

namespace Threeyes.Data
{
    /// <summary>
    /// 包含DataOption的定义
    /// </summary>
    public interface IDataOptionHolder
    {
        IDataOption BaseDataOption { get; }
    }
    public interface IDataOptionContainer<TDataOption> : IDataOptionHolder
        where TDataOption : IDataOption
    {
        TDataOption DataOption { get; }
    }

    public interface IDataOption
    {
        /// <summary>
        /// Init base on reflection info
        /// 根据Member的Attribute进行初始化（如Range、Enum类型等）
        /// </summary>
        /// <param name="memberInfo">relate member (Field or Property)</param>
        /// <returns></returns>
        IDataOption Init(MemberInfo memberInfo, object obj = null);
    }

#if USE_JsonDotNet
    [JsonObject(MemberSerialization.Fields)]//PS: Ignore Property
#endif
    public class DataOption : IDataOption
    {
        public DataOption() { }

        public virtual IDataOption Init(MemberInfo memberInfo, object obj = null) { return this; }
    }

    /// <summary>
    /// 带范围的值
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public abstract class DataOption_RangeBase<TValue> : DataOption
        , IEquatable<DataOption_RangeBase<TValue>>
    {
        public bool UseRange { get { return useRange; } set { useRange = value; } }
        public TValue MinValue { get { return minValue; } set { minValue = value; } }//如果不限制，则使用TValue类型的最小值表示
        public TValue MaxValue { get { return maxValue; } set { maxValue = value; } }//如果不限制，则使用TValue类型的最大值表示

        [SerializeField] protected bool useRange = false;
#if USE_NaughtyAttributes
        [ShowIf(nameof(useRange))]
        [AllowNesting]
#endif
        [SerializeField] protected TValue minValue;
#if USE_NaughtyAttributes
        [ShowIf(nameof(useRange))]
        [AllowNesting]
#endif
        [SerializeField] protected TValue maxValue;

        public DataOption_RangeBase(bool useRange = false, TValue minValue = default(TValue), TValue maxValue = default(TValue))
        {
            this.useRange = useRange;
            this.minValue = minValue;
            this.maxValue = maxValue;
        }

        #region IEquatable
        public override bool Equals(object obj) { return Equals(obj as DataOption_RangeBase<TValue>); }
        public override int GetHashCode() { return base.GetHashCode(); }
        public bool Equals(DataOption_RangeBase<TValue> other)
        {
            if (other == null)
                return false;
            return useRange.Equals(other.useRange) && Equals(minValue, other.minValue) && Equals(maxValue, other.maxValue);//比较必要字段。为了避免TValue为引用类型，需要使用Equals(a,b)方法
        }
        #endregion
    }
    [Serializable]
    public class DataOption_Int : DataOption_RangeBase<int>
    {
        public DataOption_Int() { }
        public DataOption_Int(bool useRange, int minValue = 0, int maxValue = 0) : base(useRange, minValue, maxValue) { }

        public override IDataOption Init(MemberInfo memberInfo, object obj = null)
        {
            if (memberInfo != null)
            {
                bool isUseRange = true;
                Vector2? tempRange = null;
                //# [Range]
                RangeAttribute rangeAttribute = memberInfo.GetCustomAttribute<RangeAttribute>();
                RangeExAttribute rangeExAttribute = memberInfo.GetCustomAttribute<RangeExAttribute>();
                if (rangeAttribute != null)//[Range]
                {
                    tempRange = new Vector2(rangeAttribute.min, rangeAttribute.max);
                }
                else if (rangeExAttribute != null && obj != null)//[RangeEx]获取动态的值
                {
                    float? minValue = rangeExAttribute.GetMinValue(obj);
                    float? maxValue = rangeExAttribute.GetMaxValue(obj);
                    bool? useRange = rangeExAttribute.GetUseRangeValue(obj);

                    if (minValue.HasValue && maxValue.HasValue)
                        tempRange = new Vector2(minValue.Value, maxValue.Value);
                    if (useRange.HasValue)
                        isUseRange = useRange.Value;
                }

                //#PS: 针对最大/最小值，如果另一端无效，则使用终端值代替
                if (tempRange == null)//Unity 
                {
                    MinAttribute minValueAttribute = memberInfo.GetCustomAttribute<MinAttribute>();
                    //MaxAttribute maxValueAttribute = memberInfo.GetCustomAttribute<MaxAttribute>();//Unity没有此字段
                    if (minValueAttribute != null)
                        tempRange = new Vector2(minValueAttribute.min, float.MaxValue);
                }
#if USE_NaughtyAttributes
                if (tempRange == null)//NaughtyAttributes [MinValue][MaxValue]
                {
                    MinValueAttribute minValueAttribute = memberInfo.GetCustomAttribute<MinValueAttribute>();
                    MaxValueAttribute maxValueAttribute = memberInfo.GetCustomAttribute<MaxValueAttribute>();
                    if (minValueAttribute != null || maxValueAttribute != null)
                        tempRange = new Vector2(minValueAttribute != null ? minValueAttribute.MinValue : int.MinValue, maxValueAttribute != null ? maxValueAttribute.MaxValue : int.MaxValue);
                }
#endif
                if (tempRange.HasValue)
                {
                    useRange = isUseRange;
                    minValue = (int)tempRange.Value.x;
                    maxValue = (int)tempRange.Value.y;
                }
            }
            return this;
        }
    }
    [Serializable]
    public class DataOption_Float : DataOption_RangeBase<float>
    {
        public DataOption_Float() { }
        public DataOption_Float(bool useRange, float minValue = 0, float maxValue = 0) : base(useRange, minValue, maxValue) { }

        public override IDataOption Init(MemberInfo memberInfo, object obj = null)
        {
            if (memberInfo != null)
            {
                bool isUseRange = true;
                Vector2? tempRange = null;
                RangeAttribute rangeAttribute = memberInfo.GetCustomAttribute<RangeAttribute>();
                RangeExAttribute rangeExAttribute = memberInfo.GetCustomAttribute<RangeExAttribute>();
                if (rangeAttribute != null)
                {
                    tempRange = new Vector2(rangeAttribute.min, rangeAttribute.max);
                }
                else if (rangeExAttribute != null && obj != null)
                {
                    float? minValue = rangeExAttribute.GetMinValue(obj);
                    float? maxValue = rangeExAttribute.GetMaxValue(obj);
                    bool? useRange = rangeExAttribute.GetUseRangeValue(obj);

                    if (minValue.HasValue && maxValue.HasValue)
                        tempRange = new Vector2(minValue.Value, maxValue.Value);
                    if (useRange.HasValue)
                        isUseRange = useRange.Value;
                }

                if (tempRange == null)//Unity 
                {
                    MinAttribute minValueAttribute = memberInfo.GetCustomAttribute<MinAttribute>();
                    //MaxAttribute maxValueAttribute = memberInfo.GetCustomAttribute<MaxAttribute>();//Unity没有此字段
                    if (minValueAttribute != null)
                        tempRange = new Vector2(minValueAttribute.min, float.MaxValue);
                }
#if USE_NaughtyAttributes
                if (tempRange == null)//NaughtyAttributes [MinValue][MaxValue]
                {
                    MinValueAttribute minValueAttribute = memberInfo.GetCustomAttribute<MinValueAttribute>();
                    MaxValueAttribute maxValueAttribute = memberInfo.GetCustomAttribute<MaxValueAttribute>();
                    if (minValueAttribute != null || maxValueAttribute != null)
                        tempRange = new Vector2(minValueAttribute != null ? minValueAttribute.MinValue : float.MinValue, maxValueAttribute != null ? maxValueAttribute.MaxValue : float.MaxValue);
                }
#endif
                if (tempRange.HasValue)
                {
                    useRange = isUseRange;
                    minValue = tempRange.Value.x;
                    maxValue = tempRange.Value.y;
                }
            }
            return this;
        }
    }
    [Serializable]
    public class DataOption_Gradient : DataOption
        , IEquatable<DataOption_Gradient>
    {
        public bool UseHDR { get { return useHDR; } set { useHDR = value; } }
        [SerializeField] protected bool useHDR = false;

        public DataOption_Gradient()
        {
            useHDR = false;
        }

        public DataOption_Gradient(bool useHDR)
        {
            this.useHDR = useHDR;
        }

        public override IDataOption Init(MemberInfo memberInfo, object obj = null)
        {
            if (memberInfo != null)
            {
                //基于[ColorUsage]或[ColorUsageEx]继续初始化
                GradientUsageAttribute gradientUsageAttribute = memberInfo.GetCustomAttribute<GradientUsageAttribute>();
                if (gradientUsageAttribute != null)
                {
                    useHDR = gradientUsageAttribute.hdr;
                }
            }
            return this;
        }

        #region IEquatable
        public override bool Equals(object obj) { return Equals(obj as DataOption_Gradient); }
        public override int GetHashCode() { return base.GetHashCode(); }
        public bool Equals(DataOption_Gradient other)
        {
            if (other == null)
                return false;
            return useHDR.Equals(other.useHDR);
        }
        #endregion
    }

    [Serializable]
    public class DataOption_Color : DataOption
        , IEquatable<DataOption_Color>
    {
        public bool UseAlpha { get { return useAlpha; } set { useAlpha = value; } }
        public bool UseHDR { get { return useHDR; } set { useHDR = value; } }

        [SerializeField] protected bool useAlpha = true;
        [SerializeField] protected bool useHDR = false;

        public DataOption_Color()
        {
            useAlpha = true;
        }
        public DataOption_Color(bool useAlpha, bool useHDR)
        {
            this.useAlpha = useAlpha;
            this.useHDR = useHDR;
        }

        public override IDataOption Init(MemberInfo memberInfo, object obj = null)
        {
            if (memberInfo != null)
            {
                //基于[ColorUsage]或[ColorUsageEx]继续初始化
                ColorUsageExAttribute colorUsageExAttribute = memberInfo.GetCustomAttribute<ColorUsageExAttribute>();
                ColorUsageAttribute colorUsageAttribute = memberInfo.GetCustomAttribute<ColorUsageAttribute>();
                if (colorUsageExAttribute != null && obj != null)//优先考虑[ColorUsageEx]，因为它能根据Option动态提供选项。[ColorUsage]通常只是方便在编辑器模式下提供所有选项
                {
                    bool? useAlphaValue = colorUsageExAttribute.GetUseAlphaValue(obj);
                    bool? useHDRValue = colorUsageExAttribute.GetUseHdrValue(obj);

                    if (useAlphaValue.HasValue)
                        useAlpha = useAlphaValue.Value;
                    if (useHDRValue.HasValue)
                        useHDR = useHDRValue.Value;
                }
                else if (colorUsageAttribute != null)
                {
                    useAlpha = colorUsageAttribute.showAlpha;
                    useHDR = colorUsageAttribute.hdr;
                }
            }
            return this;
        }

        #region IEquatable
        public override bool Equals(object obj) { return Equals(obj as DataOption_Color); }
        public override int GetHashCode() { return base.GetHashCode(); }
        public bool Equals(DataOption_Color other)
        {
            if (other == null)
                return false;
            return useAlpha.Equals(other.useAlpha) && useHDR.Equals(other.useHDR);
        }
        #endregion
    }

    /// <summary>
    /// 可选项
    /// </summary>
    [Serializable]
    public class DataOption_OptionInfo : DataOption
        , IEquatable<DataOption_OptionInfo>
    {
        public List<OptionData> listOptionData = new List<OptionData>();

        #region IEquatable
        public override bool Equals(object obj) { return Equals(obj as DataOption_OptionInfo); }
        public override int GetHashCode() { return base.GetHashCode(); }
        public bool Equals(DataOption_OptionInfo other)
        {
            if (other == null)
                return false;

            return listOptionData.IsSequenceEqual(other.listOptionData);
        }
        #endregion

        #region Define
        /// <summary>
        /// (PS: 为了兼容UGUI、TextMeshpro或以后的UIToolkit，需要定义对应的数据类）
        /// （Ref：Dropdown.OptionData）
        /// </summary>
        [Serializable]
        public class OptionData : IEquatable<OptionData>
        {
            public string text
            {
                get
                {
                    return m_Text;
                }
                set
                {
                    m_Text = value;
                }
            }
            public Sprite image
            {
                get
                {
                    return m_Image;
                }
                set
                {
                    m_Image = value;
                }
            }
            [SerializeField]
            private string m_Text;
            [SerializeField]
            private Sprite m_Image;


            public OptionData()
            {
            }
            public OptionData(string text)
            {
                this.m_Text = text;
            }
            public OptionData(Sprite image)
            {
                this.m_Image = image;
            }

            public OptionData(string text, Sprite image)
            {
                this.m_Text = text;
                this.m_Image = image;
            }

            #region IEquatable
            public override bool Equals(object obj) { return Equals(obj as OptionData); }
            public override int GetHashCode() { return base.GetHashCode(); }
            public bool Equals(OptionData other)
            {
                if (other == null)
                    return false;
                return Equals(m_Text, other.m_Text) && Equals(m_Image, other.m_Image);//可避免引用为空导致报错
            }
            #endregion
        }
        #endregion
    }

    /// <summary>
    /// 引用类型（需要可序列化）
    /// </summary>
    [Serializable]
    public class DataOption_Object : DataOption
    {
        public DataOption_Object() { }
    }

    [Serializable]
    public class DataOption_UnityObject : DataOption_Object
        , IEquatable<DataOption_UnityObject>
    {
        /// <summary>
        ///注意：
        ///-主要对内置的UnityObject有效，用于保存其具体类
        ///-如果是其他程序集中的类，需要带上其Assembly才能找到（https://stackoverflow.com/questions/3512319/resolve-type-from-class-name-in-a-different-assembly），如：
        /// “TestRuntimeEdit+TestConfig, AliveCursor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null”。针对这种类，通常是需要在运行时设置
        /// </summary>
        [Tooltip("The full name of object type (eg: UnityEngine.UI.Slider)")]
        public string objectTypeFullName;//枚举所在类型的FullName

        public DataOption_UnityObject() { }

        #region IEquatable
        public override bool Equals(object obj) { return Equals(obj as DataOption_UnityObject); }
        public override int GetHashCode() { return base.GetHashCode(); }
        public bool Equals(DataOption_UnityObject other)
        {
            if (other == null)
                return false;
            return Equals(objectTypeFullName, other.objectTypeFullName);//避免某个string为null
        }
        #endregion
    }

    /// <summary>
    /// 枚举
    /// </summary>
    [Serializable]
    public class DataOption_Enum : DataOption
        , IEquatable<DataOption_Enum>
    {
        ///命名规范：
        /// 对应Type.FullName
        /// 需要提供所在命名空间的完整路径，eg：UnityEngine.TextAlignment
        /// 如果是在类中定义的类型，那就需要用+，eg: UnityEngine.UI.Slider+Direction. (You need the '+' sign to get Nested Classes to be mapped using Assembly.GeType. https://stackoverflow.com/questions/376105/using-assembly-gettypemycompany-class1-class2-returns-null）
        /// PS：可以调用Enum.GetUnderlyingType知道枚举值的类型（int，byte，etc）(https://www.delftstack.com/howto/csharp/how-to-get-int-value-from-enum-in-csharp/#:~:text=Get%20Int%20Value%20From%20Enum%20in%20C%23%201,method%20of%20GetTypeCode%20%28%29.%203%20More%20Examples.%20)

        [Tooltip("The full name of enum type (eg: UnityEngine.UI.Slider+Direction)")]
        public string enumTypeFullName;//枚举所在类型的FullName

        public Type EnumType
        {
            get
            {
                if (enumTypeFullName.IsNullOrEmpty())
                    return null;
                Type enumType = null;//PS:因为要通过Inspector设置，因此该值不能缓存
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())//查找匹配的唯一Enum定义
                {
                    Type tempType = assembly.GetType(enumTypeFullName, false);
                    if (tempType != null && tempType.IsEnum)
                    {
                        enumType = tempType;
                        break;
                    }
                }
                return enumType;
            }
        }

        public override IDataOption Init(MemberInfo memberInfo, object obj = null)
        {
            Type variableType = memberInfo.GetVariableType();
            enumTypeFullName = variableType?.FullName;
            return this;
        }


        //——Warning:调用以下的属性之前，尽量先判断EnumType是否为空——
        public bool UseFlag { get { return EnumTool.HasFlagsAttribute(EnumType); } }
        /// <summary>
        /// 每个Enum对应的string名称（用于Inspector展示）
        ///
        /// 默认顺序：
        /// 0
        /// -1
        /// 其他值...
        /// </summary>
        public List<string> ListEnumName { get { return EnumTool.GetNamesEx(EnumType); } }


        /// <summary>
        /// Return all define name
        /// 
        /// PS：
        /// -如果是Flag，那就返回除了0的所有值
        /// </summary>
        public List<Enum> ListEnumValue
        {
            get
            {
                List<Enum> result = new List<Enum>();
                if (EnumType != null)
                {
                    foreach (var val in Enum.GetValues(EnumType))
                    {
                        result.Add(val as Enum);
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// 通过传入的枚举类型，自动初始化
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public virtual IDataOption Init(Type enumType)
        {
            enumTypeFullName = enumType?.FullName;
            return this;
        }

        /// <summary>
        /// Convert from name/value to Enum type
        /// </summary>
        /// <param name="enumNameOrValue">enum name (including multi name splited by ','), or value in string format）</param>
        /// <returns></returns>
        public Enum Parse(string enumNameOrValue) { return EnumTool.Parse(EnumType, enumNameOrValue); }

        /// <summary>
        /// 
        /// PS:
        /// -不声明为静态，避免后续要针对特殊字段进行处理
        /// </summary>
        /// <param name="displayName"></param>
        /// <param name="dicEnumNameToDisplayName"></param>
        /// <returns></returns>
        public string DisplayNameToEnumName(string displayName, Dictionary<string, string> dicEnumNameToDisplayName = null) { return EnumTool.DisplayNameToEnumName(displayName, dicEnumNameToDisplayName); }

        public string GetEnumName(object value) { return EnumTool.GetNameEx(EnumType, value); }

        /// <summary>
        /// 每个Enum的UI显示名称（与EnumName一一对应，转换而成）
        /// Return all Name or value in string type, mainly for UI display
        /// （针对Unity官方的Flag枚举（如CameraType)，因为并无定义0/1对应的枚举，所以会保存为其值的字符串）
        /// <paramref name="dicEnumNameToDisplayName"/>针对特殊值的自定义显示<paramref name="dicEnumNameToDisplayName"/>>
        /// </summary>
        public List<string> GetListDisplayName(Dictionary<string, string> dicEnumNameToDisplayName = null) { return EnumTool.GetDisplayNames(EnumType, dicEnumNameToDisplayName); }

        #region IEquatable
        public override bool Equals(object obj) { return Equals(obj as DataOption_Enum); }
        public override int GetHashCode() { return base.GetHashCode(); }
        public bool Equals(DataOption_Enum other)
        {
            if (other == null)
                return false;
            return Equals(enumTypeFullName, other.enumTypeFullName);//避免某个string为null

        }
        #endregion
    }

    //——File——

    /// <summary>
    /// Decice how to read, decode file
    /// </summary>
    [Serializable]
    public class DataOption_File : DataOption
        , IEquatable<DataOption_File>
    {
        /// <summary>
        /// Valid file extension（eg: "jpg", "jpeg", or "*" for anytype ）
        /// </summary>
        public string[] FileFilterExtensions { get { return OverrideFileFilterExtensions != null && OverrideFileFilterExtensions.Length > 0 ? OverrideFileFilterExtensions : DefaultFileFilterExtensions; } }
        public virtual string[] DefaultFileFilterExtensions { get { return new string[] { "*" }; } }
        public string[] OverrideFileFilterExtensions { get { return overrideFileFilterExtensions; } set { overrideFileFilterExtensions = value; } }
        public virtual ReadFileOption ReadFileOption { get { return readFileOption; } set { readFileOption = value; } }
        public virtual IDecodeOption DecodeOption { get { return null; } set { } }

        [SerializeField] protected string[] overrideFileFilterExtensions;
        [SerializeField] protected ReadFileOption readFileOption;

        #region IEquatable
        public override bool Equals(object obj) { return Equals(obj as DataOption_File); }
        public override int GetHashCode() { return base.GetHashCode(); }
        public bool Equals(DataOption_File other)
        {
            if (other == null)
                return false;

            return overrideFileFilterExtensions.IsSequenceEqual(other.overrideFileFilterExtensions) && Equals(readFileOption, other.readFileOption);
        }
        #endregion
    }

    [Serializable]
    public class DataOption_File<TAsset> : DataOption_File { }

    [Serializable]
    public class DataOption_File<TAsset, TDecodeOption> : DataOption_File<TAsset>
        , IEquatable<DataOption_File<TAsset, TDecodeOption>>
    where TDecodeOption : class, IDecodeOption
    {
        public override IDecodeOption DecodeOption { get { return decodeOption; } set { decodeOption = value as TDecodeOption; } }
        [SerializeField] protected TDecodeOption decodeOption;

        #region IEquatable
        public override bool Equals(object obj) { return Equals(obj as DataOption_File<TAsset, TDecodeOption>); }
        public override int GetHashCode() { return base.GetHashCode(); }
        public bool Equals(DataOption_File<TAsset, TDecodeOption> other)
        {
            if (!base.Equals(other))
                return false;
            return decodeOption.Equals(other.decodeOption);
        }
        #endregion
    }

    //PS：放在这里便于所有人访问

    [Serializable]
    public class DataOption_TextFile : DataOption_File<TextAsset, TextAssetDecoder.DecodeOption>
    {
        //PS：默认支持所有格式，且Text格式众多（Xml、json...），所以不用限制
    }

    [Serializable]
    public class DataOption_BytesFile : DataOption_File<SOBytesAsset, BytesAssetDecoder.DecodeOption>
    {
        //默认不需要限定限定后缀，如有需要可以设置overrideFileFilterExtensions
    }
    [Serializable]
    public class DataOption_TextureFile : DataOption_File<Texture, TextureDecoder.DecodeOption>
    {
        public override string[] DefaultFileFilterExtensions { get { return new string[] { "jpg", "jpeg", "png" }; } }
    }

    [Serializable]
    public class DataOption_SO : DataOption
    {
    }
}