using Threeyes.Steamworks;

/// <summary>
/// [Required]
/// Function:
/// 1.Fix init error when umod loaded
///
/// 
/// PS:
/// 1.该类应该是通用的针对Config的通用代码，本类中任意参数设置后都需要重新调用Init方法
/// 2.为了避免Config字段过多导致设置麻烦，且为了减少耦合，每个组件都要有对应的Controller及配置，其中过长的名字可以缩短（参考ZibraLiquid中的字段命名）
///
/// Warning：
/// 1.
/// </summary>
[System.Obsolete("Use the class without AC_ prefix instead!", true)]
public class AC_ZibraLiquidController : ZibraLiquidController { }