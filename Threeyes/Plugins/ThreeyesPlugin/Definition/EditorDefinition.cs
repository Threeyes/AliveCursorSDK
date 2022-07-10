using UnityEngine;
/// <summary>
/// 常见的Editor定义
/// 
/// PS:为了便于使用，都在后面加上 '/'
/// </summary>
public static class EditorDefinition
{
    //——[MenuItem]——

    /// <summary>
    /// 显示在顶部菜单栏
    /// </summary>
    public const string TopMenuItemPrefix = "Tools/Colyu/";

    /// <summary>
    /// 显示在Hierarchy窗口
    /// </summary>
    public const string HierarchyMenuItemPrefix = "GameObject/Colyu/";

    /// <summary>
    /// 显示在Project窗口
    /// </summary>
    public const string AssetMenuItemPrefix = "Assets/Colyu/";

    public const string AssetMenuItemPrefix_Create = "Assets/Create/Colyu/";


    //——[CreateAssetMenu]—— (PS:不需要加上"Assets/"
    public const string AssetMenuPrefix_SO = "SO/";
    public const string AssetMenuPrefix_SO_Build = AssetMenuPrefix_SO + "Build/";
}