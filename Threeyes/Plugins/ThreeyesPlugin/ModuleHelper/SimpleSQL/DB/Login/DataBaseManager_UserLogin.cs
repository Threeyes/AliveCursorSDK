using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if USE_SimpleSQL
using SimpleSQL;
#endif
public class DataBaseManager_UserLogin : DataBaseManagerBase<DataBaseManager_UserLogin, UserLoginInfo>
{
#if USE_SimpleSQL

    protected override void Start()
    {
        base.Start();

        CurData = QueryLatest();//自动获取上次登录的用户信息
    }

    //——Insert——

    //ToAdd:InsertWeChatLoginInfo//针对微信登录，也可以是直接调用InsertOrUpdate

    /// <summary>
    /// 本地登录
    /// </summary>
    /// <param name="unionID">用户唯一ID</param>
    public void InsertOrUpdateLocalLoginInfo(string unionID)
    {
        UserLoginInfo userLoginInfoToInsert = new UserLoginInfo();
        userLoginInfoToInsert.UnionID = unionID;
        userLoginInfoToInsert.NickName = unionID;
        userLoginInfoToInsert.LoginTime = DateTime.Now;
        userLoginInfoToInsert.LoginType = UserLoginInfo.UserLoginType.Local;

        InsertOrUpdate(userLoginInfoToInsert);
    }

    public override void InsertOrUpdate(UserLoginInfo data)
    {
        base.InsertOrUpdate(data);

        CurData = data;//Todo:移动到统一调用的地方
        //PS：此时应该监听onCurDataUpdate事件，刷新UI列表
    }

    //——Query——

    public List<UserLoginInfo> listDebugData;
    [ContextMenu("TestQueryAllWithOrder")]
    public void TestQueryAllWithOrder()
    {
        listDebugData = QueryAllWithOrder();
    }

    /// <summary>
    /// 获取最新登录用户的信息
    /// </summary>
    /// <returns></returns>
    public UserLoginInfo QueryLatest()
    {
        var result = QueryAllWithOrder().FirstOrDefault();
        if (result == null)
        {
            Debug.Log("先前无人登录");
        }
        return result;
    }

    /// <summary>
    /// 查找UID对应的用户，可能返回null
    /// </summary>
    /// <param name="unionID"></param>
    /// <returns></returns>
    public UserLoginInfo QueryByUID(string unionID)
    {
        UserLoginInfo target = QueryAll().Find(i => i.UnionID == unionID);
        if (target == null)
        {
            Debug.Log("找不到UID为 " + unionID + " 的用户！");
        }
        return target;
    }


    /// <summary>
    /// 获取所有登录的信息并降序排序，常用于列表显示
    /// </summary>
    /// <returns></returns>
    public override List<UserLoginInfo> QueryAllWithOrder()
    {
        return QueryAll().OrderByDescending(d => d.LoginTime).ToList();
    }

#endif
}

/// <summary>
/// 用户登录信息(ID作为主键)
/// 
/// Ref：存储信息 http://www.dengtar.com/20714.html
/// Ref：微信登录及数据解密的完整流程 https://blog.csdn.net/qq_41970025/article/details/90700677
/// Ref：官方连接：https://developers.weixin.qq.com/doc/oplatform/Website_App/WeChat_Login/Authorized_Interface_Calling_UnionID.html
/// 
/// ToDo:尝试改名为DB_UserLoginInfo
/// </summary>
[Serializable]
public class UserLoginInfo : SerializationDeSerializationData<UserLoginInfo>
{
    [PrimaryKey] public string UnionID { get { return unionid; } set { unionid = value; } }
    public string OpenID { get { return openid; } set { openid = value; } }
    public string NickName { get { return nickname; } set { nickname = value; } }
    public string Sex { get { return sex; } set { sex = value; } }
    public string Province { get { return province; } set { province = value; } }
    public string City { get { return city; } set { city = value; } }
    public string Country { get { return country; } set { country = value; } }
    public string HeadImgUrl { get { return headimgurl; } set { headimgurl = value; } }
    public string Session_key { get { return session_key; } set { session_key = value; } }

    public DateTime LoginTime { get { return loginTime; } set { loginTime = value; strLoginTime = value.ToString(); } }
    public UserLoginType LoginType { get { return loginType; } set { loginType = value; } }
    public string Site { get { return site; } set { site = value; } }
    public string Remark { get { return remark; } set { remark = value; } }


    //——WeChat——

    //注意两个id的区别（https://developers.weixin.qq.com/community/develop/doc/00006699a048a0299f693c61751800）：
    //1:openid同一用户同一应用唯一，unionid同一用户不同应用唯一。
    //获取用户的openid是无需用户同意的，获取用户的基本信息（包括unionid）则需要用户同意
    [SerializeField] private string unionid;//用户统一标识（类似身份证号）。针对一个微信开放平台帐号下的应用，同一用户的unionid是唯一的。（开发者最好保存用户unionID信息，以便以后在不同应用中进行用户信息互通。）
    [SerializeField] private string openid;//普通用户的标识，对当前开发者帐号唯一（类似学号、工号）【也可以是本地登录ID】
    [SerializeField] private string nickname;//用户显示名称(本地登录为空，那就使用userID）
    [SerializeField] private string sex;//性别(1为男性，2为女性)
    [SerializeField] private string province;//省份
    [SerializeField] private string city;//城市
    [SerializeField] private string country;//国家，如中国为CN
    [SerializeField] private string headimgurl;//头像图像地址(用户头像，最后一个数值代表正方形头像大小（有0、46、64、96、132数值可选，0代表640*640正方形头像），用户没有头像时该项为空)
    [SerializeField] private string session_key;//用于检测用户登录是否失效，每次login后都会返回新的key

    //——Common——

    [SerializeField] private DateTime loginTime;//登录时间
    [SerializeField] private UserLoginType loginType = UserLoginType.WeChat;//登录类型
    [SerializeField] private string site;//社区位置（如沙河社区）
    [SerializeField] private string remark;//标识

    //——Debug——

    [SerializeField] private string strLoginTime;//用于显示时间

    public enum UserLoginType
    {
        WeChat,
        Local,//本地登录
    }

    //
    /// <summary>
    /// 默认类实例，当数据库无信息时可用该实例填充
    /// 可用于调用该实例的override方法
    /// </summary>
    public static UserLoginInfo defaultInst = new UserLoginInfo()
    {
        unionid = "匿名",
        nickname = "匿名",
        loginTime = DateTime.Now,
    };
    public static UserLoginInfo Deserialize(string content)
    {
        return defaultInst.DeserializeFunc(content);
    }


    //返回信息示例：
    //{
    //"openid":"OPENID",
    //"nickname":"NICKNAME",
    //"sex":1,
    //"province":"PROVINCE",
    //"city":"CITY",
    //"country":"COUNTRY",
    //"headimgurl": "https://thirdwx.qlogo.cn/mmopen/g3MonUZtNHkdmzicIlibx6iaFqAc56vxLSUfpb6n5WKSYVY0ChQKkiaJSgQ1dZuTOgvLLrhJbERQQ4eMsv84eavHiaiceqxibJxCfHe/0",
    //"privilege":[
    //"PRIVILEGE1",
    //"PRIVILEGE2"
    //],
    //"unionid": " o6_bmasdasdsad6_2sgVt7hMZOPfL"

    //}

}

