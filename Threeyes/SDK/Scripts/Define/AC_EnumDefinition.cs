#region Cursor & Input

/// <summary>
/// Cursor state
/// </summary>
[System.Flags]
public enum AC_CursorState
{
	None = 0,
	Enter = 1 << 0,//登场
	Exit = 1 << 1,//退场
	Show = 1 << 2,//临时显示
	Hide = 1 << 3,//临时隐藏

	Working = 1 << 4,//跟随光标
	StandBy = 1 << 5,//短时间无输入
	Bored = 1 << 6,//长时间无输入

	All = ~0
}

[System.Flags]
public enum AC_CursorInputType
{
	None = 0,

	LeftButtonDownUp = 1 << 0,//1
	RightButtonDownUp = 1 << 1,
	MiddleButtonDownUp = 1 << 2,
	XButton1DownUp = 1 << 3,
	XButton2DownUp = 1 << 4,

	WheelScroll = 1 << 10,//1024

	Move = 1 << 20,//1024*1024
	Drag = 1 << 21,
	All = ~0
}

/// <summary>
/// Key State
/// </summary>
public enum AC_KeyState
{
	None = 0,
	Down = 1,
	Pressing = 2,
	Up = 3
}

/// <summary>
///Specifies the set of modifier keys.
///
/// Ref: https://docs.microsoft.com/en-us/dotnet/api/system.windows.input.modifierkeys?view=windowsdesktop-6.0
/// </summary>
[System.Flags]
public enum AC_ModifierKeys
{
	None = 0x0,//No modifiers are pressed.
	Alt = 0x1,//The ALT key.
	Control = 0x2,//The CTRL key.
	Shift = 0x4,//The SHIFT key.
				//Windows = 0x8//The Windows logo key.

	All = ~0
}

/// <summary>
/// Which side of the modifier key located
/// </summary>
public enum AC_ModifierKeySide
{
	Left,
	Right,
	Both,
}

/// <summary>
/// Defines a set of default cursor appearance
/// About: https://docs.microsoft.com/en-us/windows/win32/menurc/about-cursors
/// Enum: https://docs.microsoft.com/en-us/dotnet/api/system.windows.input.cursortype?view=windowsdesktop-6.0
/// </summary>
[System.Flags]
public enum AC_SystemCursorAppearanceType
{
	None = 0,//Unknown or not set

	//——Default——
	No = 1 << 0,//禁止标志
	Arrow = 1 << 1,//箭头【自定义箭头】
	AppStarting = 1 << 2,//箭头+圆形转圈Loading  【自定义箭头+特效】
	Crosshair = 1 << 3,//Cross(十字光标)
	Help = 1 << 4,//箭头+问号 【自定义箭头+问号】
	IBeam = 1 << 5,//输入 （应该有特殊处理，比如变成竖线或者直接禁用）
	SizeAll = 1 << 6,//四向
	SizeNESW = 1 << 7,//右上左下调整尺寸
	SizeNS = 1 << 8,//上下调整
	SizeNWSE = 1 << 9,//左上右下调整
	SizeWE = 1 << 10,//横向调整
	UpArrow = 1 << 11,//向上
	Wait = 1 << 12,//等待
	Hand = 1 << 13,//手形

	//——Extra——
	Pen = 1 << 14,//Pen （注册表中有此配置，但WinForm中无此选项）
	ScrollNS = 1 << 15,
	ScrollWE = 1 << 16,
	ScrollAll = 1 << 17,
	ScrollN = 1 << 18,
	ScrollS = 1 << 19,
	ScrollW = 1 << 20,
	ScrollE = 1 << 21,
	ScrollNW = 1 << 22,
	ScrollNE = 1 << 23,
	ScrollSW = 1 << 24,
	ScrollSE = 1 << 25,
	ArrowCD = 1 << 26,//Cursor with a CD（注册表中有此配置，但WinForm中无此选项）

	All = ~0
}
#endregion

#region Workshop Item

//——Workshop提供的选项——

/// <summary>
/// Ref: 对应Steamworks.RemoteStoragePublishedFileVisibility
/// </summary>
public enum AC_WSItemVisibility
{
	Public = 0,
	FriendsOnly = 1,
	Private = 2,
	Unlisted = 3,
}

//——Custom Tags——

/// <summary>
/// 年龄等级【必选】（Ref： https://rating-system.fandom.com/wiki/Australian_Classification_Board）
/// </summary>
public enum AC_WSItemAgeRating
{
	General,//Suitable for everyone
	ParentalGuidance,// Suggested for younger viewers
	Restricted//Children Under XX Require Accompanying Parent or Adult Guardian.
}

/// <summary>
/// Item样式(外形）
/// </summary>
[System.Flags]
public enum AC_WSItemStyle
{
	None = 0,

	Animal = 1 << 0,//动物（如猫、爬虫）
	Human = 1 << 1,//人物（如武士）
	Botany = 1 << 2,//植物（如花草）
	Food = 1 << 3,//食物（如香蕉）
	Gear = 1 << 4,//机械（如齿轮）
	Vehicle = 1 << 5,//交通工具（如车、船）
	Weapon = 1 << 6,//武器（如枪械、弹弓）
	Sport = 1 << 7,//体育（如篮球/足球）

	All = ~0//-1
}

/// <summary>
/// Item类型
/// Ref：https://www.imdb.com/feature/genre
/// </summary>
[System.Flags]
public enum AC_WSItemGenre
{
	None = 0,
	Comedy = 1 << 0,//喜剧
	Scifi = 1 << 1,//科幻（Science fiction often takes place in a dystopian society sometime in the future and contains elements of advanced technology.）【反乌托邦（如银翼杀手）】
	Horror = 1 << 2,//恐怖（如爬虫、猎头蟹 ）
	Romance = 1 << 3,//浪漫
	Action = 1 << 4,//动作
	Mystery = 1 << 5,//神秘
	Crime = 1 << 6,//犯罪
	Animation = 1 << 7,//动画片
	Adventure = 1 << 8,//冒险
	Fantasy = 1 << 9,//幻想。异兽及超自然能力（如指环王、哈利波特）（usually set in the fantasy realm and includes mythical creatures and supernatural powers.）
	SuperHero = 1 << 10,//超级英雄

	All = ~0
}

/// <summary>
/// Item引用（致敬）
/// </summary>
[System.Flags]
public enum AC_WSItemReference
{
	None = 0,

	Game = 1 << 0,//游戏（如Halo）
	Movie = 1 << 1,//电影（如Wall-E）
	Cartoon = 1 << 2,//卡通（如Futurama）
	Book = 1 << 3,//书籍（如西游记）
	Celebrity = 1 << 4,//名人（如鲁迅）
	Software = 1 << 5,//软件（如Mac）
	All = ~0
}

/// <summary>
/// 物体功能特性（多选）
/// </summary>
[System.Flags]
public enum AC_WSItemFeature
{
	None = 0,
	Interactable = 1 << 0,//可交互（碰撞）（如气球、弹球、扫雷、液体）
	AudioVisualizer = 1 << 1,//音频可视化
	Clock = 1 << 2,//时钟
	Exhibition = 1 << 3,//展示用途（如条幅、官方或第三方制作的用于展示及循环播放的广告）

	All = ~0
}

//ToUse:
/// <summary>
/// Item的高级（功能），涉及安全
/// 
/// PS:仅开发者可设置的内部Tag枚举。
/// 如：Item使用了代码，那就设置该枚举， 并且每次在Item上有UI提示
/// </summary>
[System.Flags]
public enum AC_WSItemAdvance
{
	None = 0,

	IncludeScripts = 1 << 0,//包含第三方脚本（Include custom script）
	KeyListening = 1 << 1,//监听按键（Listen to KeyBoard event）
	Networking = 1 << 2,//联网（eg:Multi Player）

	All = ~0
}


//——Internal——
/// <summary>
/// 本地Item所在位置
/// </summary>
public enum AC_WSItemLocation
{
	Downloaded,//Steam下载后的文件。包括SteamingAssets中内置的Item

	//Editor（调试用）
	UnityExported,//Unity导出的文件（已打包）
	UnityProject,//UnityProject内部的原始文件(未打包）
}

#endregion