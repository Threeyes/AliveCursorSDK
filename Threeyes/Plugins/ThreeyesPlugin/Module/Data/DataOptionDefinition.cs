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
    {
        public bool UseRange { get { return useRange; } set { useRange = value; } }
        public TValue MinValue { get { return minValue; } set { minValue = value; } }//如果不限制，则使用TValue类型的最小值表示
        public TValue MaxValue { get { return maxValue; } set { maxValue = value; } }//如果不限制，则使用TValue类型的最大值表示

        [SerializeField] protected bool useRange = false;
        #if USE_NaughtyAttributes
        [ShowIf(nameof(useRange))] [AllowNesting] 
#endif
        [SerializeField] protected TValue minValue;
#if USE_NaughtyAttributes
    [ShowIf(nameof(useRange))] [AllowNesting] 
#endif
        [SerializeField] protected TValue maxValue;

        public DataOption_RangeBase(bool useRange = false, TValue minValue = default(TValue), TValue maxValue = default(TValue))
        {
            this.useRange = useRange;
            this.minValue = minValue;
            this.maxValue = maxValue;
        }
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
    }

    [Serializable]
    public class DataOption_Color : DataOption
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
    }

    /// <summary>
    /// 可选项
    /// </summary>
    [Serializable]
    public class DataOption_OptionInfo : DataOption
    {
        public List<OptionData> listOptionData = new List<OptionData>();

#region Define

        /// <summary>
        /// (PS: 为了兼容UGUI、TextMeshpro或以后的UIToolkit，需要定义对应的数据类）
        /// （Ref：Dropdown.OptionData）
        /// </summary>
        [Serializable]
        public class OptionData
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
                this.text = text;
            }
            public OptionData(Sprite image)
            {
                this.image = image;
            }

            public OptionData(string text, Sprite image)
            {
                this.text = text;
                this.image = image;
            }
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
    }

    /// <summary>
    /// 枚举
    /// </summary>
    [Serializable]
    public class DataOption_Enum : DataOption
    {
        ///命名规范：
        /// 对应Type.FullName
        /// 需要提供所在命名空间的完整路径，eg：UnityEngine.TextAlignment
        /// 如果是在类中定义的类型，那就需要用+，eg: UnityEngine.UI.Slider+Direction. (You need the '+' sign to get Nested Classes to be mapped using Assembly.GeType. https://stackoverflow.com/questions/376105/using-assembly-gettypemycompany-class1-class2-returns-null）
        /// PS：可以调用Enum.GetUnderlyingType知道枚举值的类型（int，byte，etc）(https://www.delftstack.com/howto/csharp/how-to-get-int-value-from-enum-in-csharp/#:~:text=Get%20Int%20Value%20From%20Enum%20in%20C%23%201,method%20of%20GetTypeCode%20%28%29.%203%20More%20Examples.%20)

        [Tooltip("The full name of enum type (eg: UnityEngine.UI.Slider+Direction)")]
        public string enumTypeFullName;//枚举所在类型的FullName

        public override IDataOption Init(MemberInfo memberInfo, object obj = null)
        {
            Type variableType = memberInfo.GetVariableType();
            enumTypeFullName = variableType?.FullName;
            return this;
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

        //——Warning:调用以下的属性之前，尽量先判断EnumType是否为空——
        public bool UseFlag
        {
            get
            {
                if (EnumType != null)
                {
                    return EnumType.IsDefined(typeof(FlagsAttribute), false);
                }
                return false;
            }
        }

        /// <summary>
        /// 存储特殊映射
        /// PS：
        /// 1.因为Enum不一定是int，因此统一通过string存储Enum值，便于转换）
        /// 2.只有未定义时，其EnumName为其数值的对应字符串；如果已经定义，则无需转换
        ///
        /// Warning:
        /// 不能直接声明Dictionary为ReadOnly，否则UMod加载会报错
        /// </summary>
        [JsonIgnore]
        public static Dictionary<string, string> defaultDicSpeicalEnumNameToDisplayName
        {
            get
            {
                if (_defaultDicSpeicalEnumNameToDisplayName == null)
                    _defaultDicSpeicalEnumNameToDisplayName = new Dictionary<string, string>
                {
                    {defaultNothingEnumName,"Nothing" },
                    {defaultEverythingEnumName,"Everything" }
                };
                return _defaultDicSpeicalEnumNameToDisplayName;
            }
        }
        static Dictionary<string, string> _defaultDicSpeicalEnumNameToDisplayName;

        public const string defaultNothingEnumName = "0";
        public const string defaultEverythingEnumName = "-1";


        public string DisplayNameToEnumName(string displayName, Dictionary<string, string> dicEnumNameToDisplayName = null)
        {
            string enumName = displayName;

            if (dicEnumNameToDisplayName == null)
                dicEnumNameToDisplayName = defaultDicSpeicalEnumNameToDisplayName;
            if (dicEnumNameToDisplayName.ContainsValue(displayName))
                enumName = dicEnumNameToDisplayName.GetKey(displayName);
            return enumName;
        }

        //——ToUpdate:改为返回Dic

        /// <summary>
        /// 每个Enum的UI显示名称（与EnumName一一对应，转换而成）
        /// Return all Name or value in string type, mainly for UI display
        /// （针对Unity官方的Flag枚举（如CameraType)，因为并无定义0/1对应的枚举，所以会保存为其值的字符串）
        /// <paramref name="dicEnumNameToDisplayName"/>针对特殊值的自定义显示<paramref name="dicEnumNameToDisplayName"/>>
        /// </summary>
        public List<string> GetListDisplayName(Dictionary<string, string> dicEnumNameToDisplayName = null)
        {
            if (dicEnumNameToDisplayName == null)
                dicEnumNameToDisplayName = defaultDicSpeicalEnumNameToDisplayName;

            List<string> listResult = new List<string>(ListEnumName);

            //对EnumName进行名字替换
            for (int i = 0; i != listResult.Count; i++)
            {
                string curEnumName = listResult[i];
                if (dicEnumNameToDisplayName.ContainsKey(curEnumName))
                {
                    listResult[i] = dicEnumNameToDisplayName[curEnumName];
                }
            }
            return listResult;
        }

        public string GetEnumName(object value)
        {
            string enumName = "";

            Type enumType = EnumType;
            if (enumType != null)
            {
                if (UseFlag)
                {
                    //如果Enume没有定义0/-1，则添加
                    if (value.Equals(0) && !Enum.IsDefined(enumType, 0))
                        return defaultNothingEnumName;
                    if (value.Equals(-1) && !Enum.IsDefined(enumType, -1))
                        return defaultEverythingEnumName;
                }
                try
                {
                    enumName = Enum.GetName(enumType, value);
                }
                catch (Exception e)
                {
                    Debug.LogError("GetEnumName failed:" + e);
                }
            }
            return enumName;
        }

        /// <summary>
        /// 每个Enum对应的string名称
        ///
        /// 默认顺序：
        /// 0
        /// -1
        /// 其他值...
        /// </summary>
        public List<string> ListEnumName
        {
            get
            {
                List<string> listResult = new List<string>();
                if (EnumType != null)
                {
                    listResult.AddRange(Enum.GetNames(EnumType));
                    if (UseFlag)
                    {
                        //如果Enume没有定义0/-1，则添加（需要注意顺序不能弄错）
                        if (!Enum.IsDefined(EnumType, 0))
                            listResult.Insert(0, defaultNothingEnumName);
                        if (!Enum.IsDefined(EnumType, -1))
                            listResult.Add(defaultEverythingEnumName);
                    }
                }
                return listResult;
            }
        }

        /// <summary>
        /// Return all define name
        /// (如果是FLag，那就返回除了0的所有值)
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
        /// Convert from name/value to Enum type
        /// </summary>
        /// <param name="enumNameOrValue">enum name (including multi name splited by ','), or value in string format）</param>
        /// <returns></returns>
        public Enum Parse(string enumNameOrValue)
        {
            if (EnumType != null && enumNameOrValue.NotNullOrEmpty())
            {
                //判断当前所有名称是否都是该枚举的定义，避免更换枚举或改名的情况。注意如果是Flag就要将添加的"0"和"-1"考虑进去 (PS:不能用Enum.IsDefined，因为该方法不能判断多个值）
                string[] arrCurEnum = enumNameOrValue.Split(',');//分离出所有可能的值
                if (arrCurEnum.ToList().TrueForAll((str) => ListEnumName.Contains(str.Trim())))
                {
                    object result = null;
                    try
                    {
                        result = Enum.Parse(EnumType, enumNameOrValue);
                        if (result != null)
                            return result as Enum;
                    }
                    catch
                    {
                        //暂不处理
                    }
                }
            }
            return null;
        }
    }

    //——File——

    /// <summary>
    /// Decice how to read, decode file
    /// </summary>
    [Serializable]
    public class DataOption_File : DataOption
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
    }

    [Serializable]
    public class DataOption_File<TAsset> : DataOption_File { }

    [Serializable]
    public class DataOption_File<TAsset, TDecodeOption> : DataOption_File<TAsset>
    where TDecodeOption : class, IDecodeOption
    {
        public override IDecodeOption DecodeOption { get { return decodeOption; } set { decodeOption = value as TDecodeOption; } }
        [SerializeField] protected TDecodeOption decodeOption;
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
