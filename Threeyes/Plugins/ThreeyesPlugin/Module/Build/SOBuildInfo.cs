using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif
/// <summary>
/// 打包信息
/// 
/// 命名规范：
/// BuildInfo名字：
/// 电脑：     英文程序名_Common
///  一体机：英文程序名_Common_AIO    
/// 
/// Folder名字：
/// 
/// </summary>
[CreateAssetMenu(menuName = EditorDefinition.AssetMenuPrefix_SO_Build + "BuildInfo")]
public class SOBuildInfo : ScriptableObject
{
    public List<SOSceneInfo> AllSceneInfo
    {
        get
        {
            var rawGroup = soSceneInfoGroup ? soSceneInfoGroup.ListData : listSceneInfo;
            return rawGroup.FindAll((ssi) => ssi.sceneInfoType == SOSceneInfo.SceneInfoType.Normal);//仅使用有效的数据
        }
    }

    public SOBuildInfo()
    {
        //Init
        streamingAssetsFolderName = appName;
    }

    [Header("场景配置")]
    public SOCompanyConfig companyConfig;//用于保存各公司的资源和配置
    public SOConsoleSceneConfig sOConsoleSceneConfig;//场景控制台配置（可空）
    public SOSceneInfoGroup soSceneInfoGroup;//需要打包的场景组合
    public List<SOSceneInfo> listSceneInfo = new List<SOSceneInfo>();//需要打包的场景
    [Tooltip("对场景进行进一步修改，可空")]
    //在打包前对场景进行修改（如使用通用的播放器场景，则只需要添加继承于SOSceneModifierBase的方法即可修改其使用的资源）（适用于OEM）
    public List<SOSceneModifierBase> listSceneModifier = new List<SOSceneModifierBase>();
    [Tooltip("只会拷贝文件夹，不拷贝文件！")]
    public bool isCopyStreamingAsset = true;
    public bool isReplaceStreamingAsset = false;//替换原来的
    public bool isOpenFolderAfterBuild = false;//打包之后打开文件夹

    [Header("展示信息")]
    public string companyName = "";//公司名
    public string oemName = "";//文件包装名
    public string folderName = "";//电脑：   程序名 。一体机：程序名_一体机
    public string appName = "";
    public string streamingAssetsFolderName = "";//特殊名称，可空

    public string applicationIdentifier;
    [Tooltip("exe文件图标，可空")]
    public Texture2D logo;//exe文件图标，可空
    [Tooltip("项目管理软件上的显示图标。注意：如需设置Icon，需要在PlayerSettings-Icon栏目下，勾选 Override for PC，Mac & Linux Standalone")]
    public Texture2D icon;//项目管理软件上的显示图标(512×512,png格式)，可空


    [Header("打包注释")]
    public List<UpdateDescriptionInfo> listUpdateDescriptionInfo = new List<UpdateDescriptionInfo>();

    /// <summary>
    /// 应用打包注释
    /// </summary>
    [Serializable]
    public class UpdateDescriptionInfo
    {
        public string strDateTime = "";
        public string version = "1.0";
        [Multiline]
        public string description = "";

        public override string ToString()
        {
            string content = "————\r\n";
            content += "时间" + strDateTime + "\r\n";
            content += "\t版本" + version + "\r\n";
            content += "\t描述：" + description;

            return content;
        }
    }

    /// <summary>
    /// 增加更新描述
    /// </summary>
    public void AddUpdateDescription()
    {
        string curVersion = "1.0";
        UpdateDescriptionInfo lastUpdateDescriptionInfo = listUpdateDescriptionInfo.LastOrDefault();
        if (lastUpdateDescriptionInfo.NotNull())
        {
            float fLastVersion = lastUpdateDescriptionInfo.version.TryParse<float>();
            if (fLastVersion != default(float))
                curVersion = (fLastVersion + 0.1f).ToString();
        }

        UpdateDescriptionInfo updateDescriptionInfo = new UpdateDescriptionInfo();
        updateDescriptionInfo.strDateTime = DateTime.Now.Serialize();
        updateDescriptionInfo.version = curVersion;
        listUpdateDescriptionInfo.Add(updateDescriptionInfo);
    }

}


#region Rubbish

///// <summary>
///// 公司名
///// </summary>
//public enum CompanyNameType
//{
//    Colyu = 0,
//    MyCourse = 5,
//}

//[AttributeUsage(AttributeTargets.Field)]
//public class RemarkAttribute : Attribute
//{
//    public RemarkAttribute(SceneNameType sceneName, string cnName, string folderName)
//    {
//        this.sceneName = sceneName;
//        this.cnName = cnName;
//        this.folderName = folderName;
//    }
//    public SceneNameType sceneName
//    {
//        get;
//        private set;
//    }
//    public string cnName
//    {
//        get;
//        private set;
//    }
//    public string folderName
//    {
//        get;
//        private set;
//    }
//}

//public static class EditorExtensions
//{
//    /// <summary>
//    /// 获取枚举描述特性值
//    /// </summary>
//    /// <typeparam name="TEnum"></typeparam>
//    /// <param name="enumerationValue">枚举值</param>
//    /// <returns>枚举值的描述/returns>
//    public static RemarkAttribute GetDescription<TEnum>(this TEnum enumerationValue)
//        where TEnum : struct, IComparable, IFormattable, IConvertible
//    {
//        Type type = enumerationValue.GetType();
//        if (!type.IsEnum)
//        {
//            throw new ArgumentException("EnumerationValue必须是一个枚举值", "enumerationValue");
//        }

//        //使用反射获取该枚举的成员信息
//        MemberInfo[] memberInfo = type.GetMember(enumerationValue.ToString());
//        if (memberInfo != null && memberInfo.Length > 0)
//        {
//            object[] attrs = memberInfo[0].GetCustomAttributes(typeof(RemarkAttribute), false);

//            if (attrs != null && attrs.Length > 0)
//            {
//                //返回枚举值得描述信息
//                return ((RemarkAttribute)attrs[0]);
//            }
//        }
//        //如果没有描述特性的值，返回该枚举值得字符串形式
//        return null;
//    }
//}

#endregion