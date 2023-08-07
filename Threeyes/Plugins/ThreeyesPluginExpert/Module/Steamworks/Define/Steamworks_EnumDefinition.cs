using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threeyes.Steamworks
{

    #region Common

    #endregion

    #region Workshop Item
    /// <summary>
    /// Ref: 对应Steamworks.RemoteStoragePublishedFileVisibility
    /// </summary>
    public enum WSItemVisibility
	{
		Public = 0,
		FriendsOnly = 1,
		Private = 2,
		Unlisted = 3,
	}

	//——Internal——
	/// <summary>
	/// 本地Item所在位置
	/// </summary>
	public enum WSItemLocation
	{
		Downloaded,//Steam下载后的文件。包括SteamingAssets中内置的Item

		//Editor（调试用）
		UnityExported,//【已打包】Unity导出的文件
		UnityProject,//【未打包】UnityProject内部的原始文件
	}
    #endregion

    #region Workshop Query
    /// <summary>
    /// 限制查询Item的范围
    /// 
    /// PS：
    /// -Where不为All时才需要调用Query方法
    /// -如果为All，则使用SortBy；否则只用RankedBy
    /// 
    /// Ref：https://partner.steamgames.com/doc/api/ISteamUGC#CreateQueryAllUGCRequest 中的 EUserUGCList
    /// 参考：
    /// -Steam网页：个人网页 » Workshop Items » Alive Cursor » Workshop Items页面中，右侧的“显示”选项
    /// -Wallpaper：Workshop界面中的排序栏，蓝线下的5行选项（My Favorites、Vote Up等）
    /// </summary>
    public enum WSQueryWhere
	{
		All = 0,//PS：不做限制
		UserPublished,//List of files the user has published. (equivalent to http://steamcommunity.com/my/myworkshopfiles/?browsesort=myfiles)
		VotedOn,//List of files the user has voted on. Includes both VotedUp and VotedDown.
		VotedUp,//List of files the user has voted up. (Restricted to the current user only).
		VotedDown,//List of files the user has voted down. (Restricted to the current user only).
				  //WillVoteLater,//Deprecated. Do not use! (Restricted to the current user only).
		Favorited,//List of files the user has favorited. (equivalent to http://steamcommunity.com/my/myworkshopfiles/?browsesort=myfavorites)
		Subscribed,//List of files the user has subscribed to. (Restricted to the current user only). (equivalent to http://steamcommunity.com/my/myworkshopfiles/?browsesort=mysubscriptions)
		UsedOrPlayed,//List of files the user has spent time in game with. (equivalent to http://steamcommunity.com/my/myworkshopfiles/?browsesort=myplayedfiles)
		Followed//List of files the user is following updates for.
	}


	/// <summary>
	/// 针对所有Item的排序
	/// PS:参考WallpaperEngine本地与Workshop采用不用的筛选策略
	/// Ref: https://partner.steamgames.com/doc/api/ISteamUGC#CreateQueryUserUGCRequest EUGCQuery
	/// </summary>
	public enum WSQueryRankedBy
	{
		Vote = 0,//【Most Popular】Sort by vote popularity(投票支持率) all-time
		PublicationDate,//【Most Recent】（最新最前）Sort by publication date descending
		VotesUp,//【Most Up Votes】Sort by number of votes up descending. Will use the "trend" period if specified (set in SetRankedByTrendDays)
		TotalUniqueSubscriptions,//【Most Subscribed】	Sort by lifetime total unique # of subscribers descending
								 //LastUpdatedDate,//【Last Updated】Sort by last updated time.//Todo:facepunch 未提供接口，需要重新打包Dll然后启用

		//ToAdd【非必要】:Friends相关、PlayTime相关
	}

	/// <summary>
	/// 对应SteamAPI的SetRankedByTrendDays（https://partner.steamgames.com/doc/api/ISteamUGC#SetRankedByTrendDays）
	/// </summary>
	public enum WSQueryRankedByTrendDays
	{
		Today = 0,
		Week,
		Month,
		Year,
		AllTime//对应k_EUGCQuery_RankedByVote
	}
	public enum WSQueryRankedBy_Local
	{
		//ToAdd
	}

	/// <summary>
	/// 【ToUse】针对用户Item的排序
	///【Warning】：SortBy需要同时Where不为All，否则会无效。具体可看源码UgcQuery.GetPageAsync，以及我的回复（https://github.com/Facepunch/Facepunch.Steamworks/issues/627）
	/// Ref：https://partner.steamgames.com/doc/api/ISteamUGC#GetUserItemVote 中的 EUserUGCListSortOrder
	/// 参考：
	/// -Steam网页：个人网页 » Workshop Items » Alive Cursor » Workshop Items页面中，右侧的选项（Favorited、Subscribed Items）对应[Where]方法，右侧的排序栏就对应下方方法
	/// </summary>
	public enum WSQuerySortBy
	{
		CreationDate = 0,//按创建日期返回物品。 降序 - 从最新的物品开始。（默认值)
		CreationDateAsc,//按创建日期返回物品。 升序 - 从最旧的物品开始。
		TitleAsc,//按名称返回物品。
		UpdateDate,//首先返回最近更新的物品。
		SubscriptionDate,//首先返回最近订阅的物品。
		VoteScore,//首先返回最近分数更新的物品。
		ForModeration,//返回待审核的物品。 
	}

	#endregion
}