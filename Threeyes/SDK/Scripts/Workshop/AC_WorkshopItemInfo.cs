using Newtonsoft.Json;

///Todo：
/// 1.本地查询只实现基本的Tag、文件名搜索（需要自己实现）。
/// 2.联网时会显示高级的Tag搜索栏、我的主题以及搜索选项，这样的好处是可以统一调用Steam的查询方法，不需要自己重新实现。
/// 3.（仅开发者调试）扫描打包后的文件夹并列在SteamWorkshopBrowser中，便于选择（此时可以不更新UIItemInfoPanel，减少不必要的工作量）【本地的Mod是不需要在SteamWorkshopBrowser中显示的，因为不是WallPaperEngine那种可以直接编辑的方式。】
/// 4.用户联网时主动点击一个下载后的Item并查询到Steamworks.Ugc.Item后，将其整个反序列化并存到Item文件夹中，便于不登录Steam时查询，这样的好处是能够存储足够多的数据，且不需要将ItemId、OwnerId等存储到WorkshopItemInfo中以增加复杂性。（用户离线时点击就暂不获取信息）

///以下信息不需要存储，通过运行时获得：
/// 1.作者ID【通过查询ItemId可得知】（如果在Build时保存，那么用户只能登录后才Build）
/// 2.ItemID（因为是上传完成后才会获得该ID，所以初次上传前并不会得到该ID。已经上传的Item可以直接通过其文件夹名获得，所以没有必要存储）【通过查询ItemId可得知】
/// 3.Version(与Setting同步)（非必须，可直接通过UMod的SDK版本获取，但是必须保证SDK版本正确）


/// <summary>
/// 管理打包后的Item信息
/// 通过Mod文件夹中的ItemInfo.json存储，便于Browser读取下载后的Mod然后反序列化该文件并实现离线读取信息
/// (通用的类，便于后期制作其他Steam项目）
/// </summary>
[System.Serializable]
[JsonObject(MemberSerialization.OptIn)]
public sealed class AC_WorkshopItemInfo
{
    //#Basic Info
    [JsonProperty] public string title;
    [JsonProperty] public string description;
    [JsonProperty] public string modFileRelatePath;//局部的Mod文件路径
    [JsonProperty] public string previewFileRelatePath;//局部预览图路径                                   
    [JsonProperty] public AC_WSItemVisibility itemVisibility = AC_WSItemVisibility.Public;
    [JsonProperty] public string[] tags;//PS:因为最终呈现的形式都是tags，所以不需要存储枚举，以免后续有更改

    //#Runtime Info
    public AC_WSItemLocation itemLocation = AC_WSItemLocation.Downloaded;//标识Mod位置，便于区分
    public ulong id;//Item ID
    public string dirPath;//Item所在文件夹路径
    public long fileSize;//文件总大小

    public static readonly string ItemInfoFileName = "ItemInfo.json";//序列化的WorkshopItemInfo
    public static readonly string ItemModName = "Scene";//打包后的Mod名称
    public static readonly string ItemModFileName = ItemModName + ".umod";//Mod文件
   
    public AC_WorkshopItemInfo() { }//PS：便于Json反序列化调用的默认构造函数
}