using System;
using System.Reflection;
using UnityEngine;
using Threeyes.Core;

#if USE_NaughtyAttributes
using NaughtyAttributes;
#endif
namespace Threeyes.Data
{
    /// <summary>
    /// Create relate DataOption
    /// </summary>
    public static class DataOptionFactory
    {
        public static DataOption_File CreateFile(MemberInfo memberInfo, Type assetType)
        {
            DataOption_File option = null;

            //ToUpdate:后期通过反射直接利用Type.IsSubclassOf搜索继承DataOption_File<TAsset>的类，便于用户自定义DataOption
            if (memberInfo != null && assetType != null)
            {
                switch (assetType.Name)
                {
                    case nameof(TextAsset): option = new DataOption_TextFile(); break;
                    case nameof(SOBytesAsset): option = new DataOption_BytesFile(); break;
                    case nameof(Texture): option = new DataOption_TextureFile(); break;
                }
            }

            if (option == null)
                option = new DataOption_File();
            option.Init(memberInfo);
            return option;
        }


        /// <summary>
        /// 针对有额外设置的类型，创建特殊的Option（如bool、Enum、file等）；其他普通类型则使用默认Option（如
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        public static IDataOption Create(MemberInfo memberInfo = null, object obj = null)
        {
            IDataOption option = null;
            if (memberInfo != null)
            {
                Type variableType = memberInfo.GetVariableType();//FieldInfo/PropertyInfo的类型
                Type baseType = DataTool.GetBaseType(variableType);
                switch (baseType.Name)
                {
                    case nameof(Int32): option = new DataOption_Int(); break;
                    case nameof(Single): option = new DataOption_Float(); break;
                    case nameof(Enum): option = new DataOption_Enum(); break;
                    case nameof(Color): option = new DataOption_Color(); break;
                    case nameof(Gradient): option = new DataOption_Gradient(); break;

                    default: option = new DataOption(); break;
                }
            }
            if (option == null)//使用默认值，避免返回null
                option = new DataOption();

            option.Init(memberInfo, obj);//根据Member的Attribute进行初始化（如Range、Enum类型等）
            return option;
        }

        #region NaughtyAttributes
        static Vector2? NaughtyAttributes_GetMinMaxRange(MemberInfo memberInfo)
        {
            Vector2? result = null;
#if USE_NaughtyAttributes
            MinValueAttribute minValueAttribute = memberInfo.GetCustomAttribute<MinValueAttribute>();
            MaxValueAttribute maxValueAttribute = memberInfo.GetCustomAttribute<MaxValueAttribute>();
            if (minValueAttribute != null && maxValueAttribute != null)
            {
                result = new Vector2(minValueAttribute.MinValue, maxValueAttribute.MaxValue);
            }
#endif
            return result;
        }
        #endregion

    }
}
