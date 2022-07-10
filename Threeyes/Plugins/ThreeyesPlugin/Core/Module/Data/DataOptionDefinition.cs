using System;
using System.Collections.Generic;
using System.Linq;
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
    public interface IDataOption
    {
        /// <summary>
        /// Init base on reflection info
        /// </summary>
        /// <param name="memberInfo">relate member (Field or Property)</param>
        /// <returns></returns>
        IDataOption Init(MemberInfo memberInfo);
    }

    public class DataOption : IDataOption
    {
        public virtual IDataOption Init(MemberInfo memberInfo) { return this; }
    }

    /// <summary>
    /// ����Χ
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
#if USE_JsonDotNet
    [JsonObject(MemberSerialization.Fields)]//PS: Ignore Property
#endif
    public abstract class DataOption_RangeBase<TValue> : DataOption
    {
        public bool UseRange { get { return useRange; } set { useRange = value; } }
        public TValue MinValue { get { return minValue; } set { minValue = value; } }
        public TValue MaxValue { get { return maxValue; } set { maxValue = value; } }

        [SerializeField] protected bool useRange = false;
        [SerializeField] protected TValue minValue;
        [SerializeField] protected TValue maxValue;

        public DataOption_RangeBase(bool useRange = false, TValue minValue = default(TValue), TValue maxValue = default(TValue))
        {
            this.useRange = useRange;
            this.minValue = minValue;
            this.maxValue = maxValue;
        }
    }
    [System.Serializable]
    public class DataOption_Int : DataOption_RangeBase<int>
    {
        public DataOption_Int(bool useRange = false, int minValue = 0, int maxValue = 0) : base(useRange, minValue, maxValue) { }

        public override IDataOption Init(MemberInfo memberInfo)
        {
            if (memberInfo != null)
            {
                Vector2? tempRange = null;
                RangeAttribute rangeAttribute = memberInfo.GetCustomAttribute<RangeAttribute>();
                if (rangeAttribute != null)
                    tempRange = new Vector2(rangeAttribute.min, rangeAttribute.max);
#if USE_NaughtyAttributes
                else
                {
                    MinValueAttribute minValueAttribute = memberInfo.GetCustomAttribute<MinValueAttribute>();
                    MaxValueAttribute maxValueAttribute = memberInfo.GetCustomAttribute<MaxValueAttribute>();
                    if (minValueAttribute != null && maxValueAttribute != null)
                    {
                        tempRange = new Vector2(minValueAttribute.MinValue, maxValueAttribute.MaxValue);
                    }
                }
#endif
                if (tempRange.HasValue)
                {
                    useRange = true;
                    minValue = (int)tempRange.Value.x;
                    maxValue = (int)tempRange.Value.y;
                }
            }
            return this;
        }
    }
    [System.Serializable]
    public class DataOption_Float : DataOption_RangeBase<float>
    {
        public DataOption_Float(bool useRange = false, float minValue = 0, float maxValue = 0) : base(useRange, minValue, maxValue) { }

        public override IDataOption Init(MemberInfo memberInfo)
        {
            if (memberInfo != null)
            {
                Vector2? tempRange = null;
                RangeAttribute rangeAttribute = memberInfo.GetCustomAttribute<RangeAttribute>();
                if (rangeAttribute != null)
                    tempRange = new Vector2(rangeAttribute.min, rangeAttribute.max);
#if USE_NaughtyAttributes
                else
                {
                    MinValueAttribute minValueAttribute = memberInfo.GetCustomAttribute<MinValueAttribute>();
                    MaxValueAttribute maxValueAttribute = memberInfo.GetCustomAttribute<MaxValueAttribute>();
                    if (minValueAttribute != null && maxValueAttribute != null)
                    {
                        tempRange = new Vector2(minValueAttribute.MinValue, maxValueAttribute.MaxValue);
                    }
                }
#endif
                if (tempRange.HasValue)
                {
                    useRange = true;
                    minValue = tempRange.Value.x;
                    maxValue = tempRange.Value.y;
                }
            }
            return this;
        }
    }

    /// <summary>
    /// ��ѡ��
    /// </summary>
    [System.Serializable]
    public class DataOption_OptionInfo : DataOption
    {
        public List<OptionData> listOptionData = new List<OptionData>();

        #region Define

        /// <summary>
        /// (PS: Ϊ�˼���UGUI��TextMeshpro���Ժ��UIToolkit����Ҫ�����Ӧ�������ࣩ
        /// ��Ref��Dropdown.OptionData��
        /// </summary>
        [System.Serializable]
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
    /// ��ѡ��
    /// </summary>
    [System.Serializable]
    public class DataOption_EnumInfo : DataOption
    {
        public override IDataOption Init(MemberInfo memberInfo)
        {
            Type variableType = memberInfo.GetVariableType();
            enumTypeFullName = variableType?.FullName;
            return this;
        }

        ///PS:
        /// Enum.GetUnderlyingType����֪��ö��ֵ�����ͣ�int��byte��etc��(https://www.delftstack.com/howto/csharp/how-to-get-int-value-from-enum-in-csharp/#:~:text=Get%20Int%20Value%20From%20Enum%20in%20C%23%201,method%20of%20GetTypeCode%20%28%29.%203%20More%20Examples.%20)

        ///�����淶��
        /// ��ӦType.FullName
        /// ��Ҫ�ṩ���������ռ������·����eg��UnityEngine.TextAlignment
        /// ����������ж�������ͣ��Ǿ���Ҫ��+��eg: UnityEngine.UI.Slider+Direction. (You need the '+' sign to get Nested Classes to be mapped using Assembly.GeType. https://stackoverflow.com/questions/376105/using-assembly-gettypemycompany-class1-class2-returns-null��
        [Tooltip("The full name of enum type (eg: UnityEngine.UI.Slider+Direction)")]
        public string enumTypeFullName;//ö���������͵�FullName

        //����Warning:�������µ�����֮ǰ����Ҫ���ж�EnumType�Ƿ�Ϊ�ա���
        public Type EnumType
        {
            get
            {
                Type typeEnum = null;
                if (enumTypeFullName.NotNullOrEmpty())
                {
                    //����ƥ��Ķ���
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        Type tempType = assembly.GetType(enumTypeFullName, false);
                        if (tempType != null && tempType.IsEnum)
                        {
                            typeEnum = tempType;
                            break;
                        }
                    }
                }
                return typeEnum;
            }
        }

        /// <summary>
        /// Convert from name/value to Enum type
        /// </summary>
        /// <param name="enumNameOrValue">enum name (including multi name splited by ','), or value in string format��</param>
        /// <returns></returns>
        public Enum Parse(string enumNameOrValue)
        {
            if (EnumType != null && enumNameOrValue.NotNullOrEmpty())
            {
                //�жϵ�ǰ���������Ƿ��Ǹ�ö�ٵĶ��壬�������ö�ٻ�����������ע�������Flag��Ҫ����ӵ�"0"��"-1"���ǽ�ȥ (PS:������Enum.IsDefined����Ϊ�÷��������ж϶��ֵ��
                string[] arrCurEnum = enumNameOrValue.Split(',');//��������п��ܵ�ֵ
                if (arrCurEnum.ToList().TrueForAll((str) => ArrStrNameOrValue.Contains(str.Trim())))
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
                        //�ݲ�����
                    }
                }
            }
            return null;
        }

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
        /// Return ll Name or value in string type, mainly for UI display
        /// �����Unity�ٷ���Flagö�٣���CameraType)����Ϊ���޶���0/1��Ӧ��ö�٣����Իᱣ��Ϊ��ֵ���ַ�����
        /// </summary>
        public string[] ArrStrNameOrValue
        {
            get
            {
                List<string> listResult = new List<string>();
                if (EnumType != null)
                {
                    if (UseFlag)
                    {
                        if (!Enum.IsDefined(EnumType, 0))
                            listResult.Add("0");
                        if (!Enum.IsDefined(EnumType, -1))
                            listResult.Add("-1");
                    }
                    listResult.AddRange(Enum.GetNames(EnumType));
                }
                return listResult.ToArray();
            }
        }

        /// <summary>
        /// Return all define name
        /// (�����FLag���Ǿͷ��س���0������ֵ)
        /// </summary>
        public Array ArrEnumValue
        {
            get
            {
                if (EnumType != null)
                {
                    return Enum.GetValues(EnumType);
                }
                return new int[] { };
            }
        }
    }

    //����File����

    /// <summary>
    /// Decice how to read, decode file
    /// </summary>
    [Serializable]
    public class DataOption_File : DataOption
    {
        /// <summary>
        /// Valid file extension��eg: "jpg", "jpeg", or "*" for anytype ��
        /// </summary>
        public string[] FileFilterExtensions { get { return OverrideFileFilterExtensions != null && OverrideFileFilterExtensions.Length > 0 ? OverrideFileFilterExtensions : DefaultFileFilterExtensions; } }
        public virtual string[] DefaultFileFilterExtensions { get { return new string[] { "*" }; } }
        public string[] OverrideFileFilterExtensions { get { return overrideFileFilterExtensions; } set { overrideFileFilterExtensions = value; } }
        public virtual ReadFileOption ReadFileOption { get { return readFileOption; } set { readFileOption = value; } }
        public virtual IDecodeOption DecodeOption { get { return null; } set { } }

        [SerializeField] protected string[] overrideFileFilterExtensions;
        [SerializeField] protected ReadFileOption readFileOption;
    }

    [System.Serializable]
    public class DataOption_File<TAsset> : DataOption_File { }

    [System.Serializable]
    public class DataOption_File<TAsset, TDecodeOption> : DataOption_File<TAsset>
    where TDecodeOption : class, IDecodeOption
    {
        public override IDecodeOption DecodeOption { get { return decodeOption; } set { decodeOption = value as TDecodeOption; } }
        [SerializeField] protected TDecodeOption decodeOption;
    }

    //PS������������������˷���

    [System.Serializable]
    public class DataOption_TextFile : DataOption_File<TextAsset, TextAssetDecoder.DecodeOption>
    {
        //PS��Ĭ��֧�����и�ʽ����Text��ʽ�ڶࣨXml��json...�������Բ�������
    }

    [System.Serializable]
    public class DataOption_BytesFile : DataOption_File<SOBytesAsset, BytesAssetDecoder.DecodeOption>
    {
        //Ĭ�ϲ���Ҫ�޶��޶���׺��������Ҫ��������overrideFileFilterExtensions
    }
    [System.Serializable]
    public class DataOption_TextureFile : DataOption_File<Texture, TextureDecoder.DecodeOption>
    {
        public override string[] DefaultFileFilterExtensions { get { return new string[] { "jpg", "jpeg", "png" }; } }
    }


    [System.Serializable]
    public class DataOption_SO : DataOption
    {
    }
}
