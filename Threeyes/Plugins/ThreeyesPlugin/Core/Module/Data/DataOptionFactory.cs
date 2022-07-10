using System;
using System.Reflection;
using UnityEngine;

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

            //ToUpdate:����ͨ������ֱ������Type.IsSubclassOf�����̳�DataOption_File<TAsset>���࣬�����û��Զ���DataOption
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
            option?.Init(memberInfo);
            return option;
        }


        /// <summary>
        /// ����ж������õ����ͣ����������Option����bool��Enum��file�ȣ���������ͨ������ʹ��Ĭ��Option����
        /// </summary>
        /// <param name="memberInfo"></param>
        /// 
        /// <returns></returns>
        public static IDataOption Create(MemberInfo memberInfo = null)
        {
            IDataOption option = null;
            if (memberInfo != null)
            {
                Type variableType = memberInfo.GetVariableType();//FieldInfo/PropertyInfo������
                Type baseType = DataTool.GetBaseType(variableType);
                switch (baseType.Name)
                {
                    case nameof(Int32): option = new DataOption_Int(); break;
                    case nameof(Single): option = new DataOption_Float(); break;
                    case nameof(Enum): option = new DataOption_EnumInfo(); break;

                    default: option = new DataOption(); break;
                }
            }
            if (option == null)
                option = new DataOption();

            option?.Init(memberInfo);//����Member��Attribute���г�ʼ������Range��
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
