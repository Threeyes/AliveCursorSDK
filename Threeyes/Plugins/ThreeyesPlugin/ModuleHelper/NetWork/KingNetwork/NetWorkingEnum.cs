
/// <summary>
/// 收/发信息的类型
/// 
/// （This enum is responsible for represents the packets of messages to client and server handlers.）
/// </summary>
public enum ColyuPacketType : byte
{
    None = 0,
    DB_UserLoginInfo,//用户登录信息
    DB_UserPlayHistory,//用户使用信息

    //自定义信息:
    String = 101,//普通文本信息

}