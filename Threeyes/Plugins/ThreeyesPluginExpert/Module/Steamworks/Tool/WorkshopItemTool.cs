using System.IO;
using UnityEngine;

namespace Threeyes.Steamworks
{
	/// <summary>
	/// 针对WorkshopItem数据类的工具类
	/// </summary>
	public static class WorkshopItemTool
	{
		//——Path——
		public static bool IsValidItemDir(string itemDirPath)
		{
			return File.Exists(GetItemJsonFileDir(itemDirPath));
		}
		public static string GetItemJsonFileDir(string itemDirPath)
		{
			if (itemDirPath.IsNullOrEmpty())
				return "";
			return Path.Combine(itemDirPath, WorkshopItemInfo.ItemInfoFileName);
		}

		//——Id——

		/// <summary>
		/// Get Item ID via directory
		/// </summary>
		/// <param name="itemDirPath"></param>
		/// <returns></returns>
		public static ulong GetId(string itemDirPath)
		{
			ulong itemId = 0;
			if (Directory.Exists(itemDirPath))
			{
				if (PathTool.IsProjectDir(itemDirPath))//可能原因：SteamingAssets/Export文件，因为未上传，因此id无效
					return itemId;

				DirectoryInfo directoryInfo = new DirectoryInfo(itemDirPath);
				ulong valueID;
				if (ulong.TryParse(directoryInfo.Name, out valueID))//通过Mod文件夹名获取对应的ItemId
					itemId = valueID;
				else
					Debug.LogError($"Failed to convert ID {directoryInfo.Name} from item dir: { itemDirPath}");//可能原因：未上传文件
			}
			return itemId;
		}


		//——Url——

		//PS:以下Url的格式通过浏览器打开，与Steamworks.Ugc.Item统一
		// 主Url
		public static string GetUrl(ulong itemId, bool isOpenViaWeb = true)
		{
			//"steam://url/CommunityFilePage/" + itemId//PS:需要调用Steam
			return GetUrlFunc("http://steamcommunity.com/sharedfiles/filedetails/?id=", itemId, isOpenViaWeb);
		}
		public static string GetChangeLogUrl(ulong itemId, bool isOpenViaWeb = true)
		{
			return GetUrlFunc("http://steamcommunity.com/sharedfiles/filedetails/changelog/", itemId, isOpenViaWeb);
		}
		public static string GetCommentsUrl(ulong itemId, bool isOpenViaWeb = true)
		{
			return GetUrlFunc("http://steamcommunity.com/sharedfiles/filedetails/comments/", itemId, isOpenViaWeb);
		}
		public static string GetDiscussUrl(ulong itemId, bool isOpenViaWeb = true)
		{
			return GetUrlFunc("http://steamcommunity.com/sharedfiles/filedetails/discussions/", itemId, isOpenViaWeb);
		}
		public static string GetStatsUrl(ulong itemId, bool isOpenViaWeb = true)
		{
			return GetUrlFunc("http://steamcommunity.com/sharedfiles/filedetails/stats/", itemId, isOpenViaWeb);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="strMainUrl"></param>
		/// <param name="itemId"></param>
		/// <param name="isOpenViaWeb">true: open via web; false: open via steam app</param>
		/// <returns></returns>
		static string GetUrlFunc(string strMainUrl, ulong itemId, bool isOpenViaWeb = true)
		{
			if (itemId != 0)
			{
				//ToUpdate: 除GetUrl外的链接需要更新
				if (isOpenViaWeb)
					return strMainUrl + itemId.ToString();
				else
					return $"steam://url/CommunityFilePage/" + itemId.ToString();//直接通过Steam窗口打开
			}
			return null;
		}


		/// <summary>
		/// 检查RankedBy能否设置时间段
		/// </summary>
		/// <param name="wSQueryRankedBy"></param>
		/// <returns></returns>
		public static bool CanUseTrendDays(WSQueryRankedBy wSQueryRankedBy)
		{
			return wSQueryRankedBy == WSQueryRankedBy.Vote || wSQueryRankedBy == WSQueryRankedBy.VotesUp;
		}


	}

	public static class WorkshopItemInfoExtension
	{
		/// <summary>
		/// 主Url
		/// </summary>
		/// <param name="workshopItemInfo"></param>
		/// <returns></returns>
		public static string Url(this WorkshopItemInfo workshopItemInfo)
		{
			return WorkshopItemTool.GetUrl(workshopItemInfo.id);
		}

		/// <summary>
		/// Item 本地预览图Url
		/// </summary>
		/// <param name="workshopItemInfo"></param>
		/// <returns></returns>
		public static string PreviewImageUrl(this WorkshopItemInfo workshopItemInfo)
		{
			return "file://" + Path.Combine(workshopItemInfo.dirPath, workshopItemInfo.previewFileRelatePath);
		}

		/// <summary>
		/// Item 本地Mod的位置
		/// </summary>
		/// <param name="workshopItemInfo"></param>
		/// <returns></returns>
		public static string ModFilePath(this WorkshopItemInfo workshopItemInfo)
		{
			return Path.Combine(workshopItemInfo.dirPath, workshopItemInfo.modFileRelatePath);
		}
		/// <summary>
		/// Item用户配置的存储路径
		/// </summary>
		/// <param name="workshopItemInfo"></param>
		/// <returns></returns>
		public static string PersistentDataDirPath(this WorkshopItemInfo workshopItemInfo)
		{
			//PS:相对其文件夹进行存储，避免使用id生成，因为调试时未上传的Mod的id无效
			return Steamworks_PathDefinition.Data_Save_ItemDirPath + "/" + workshopItemInfo.DirName() + "/" + Steamworks_PathDefinition.persistentFolderName;
		}

		/// <summary>
		/// 本地Item用户配置的存储路径
		/// </summary>
		/// <param name="workshopItemInfo"></param>
		/// <returns></returns>
		public static string PersistentDataLocalDirPath(this WorkshopItemInfo workshopItemInfo)
		{
			//PS:相对其文件夹进行存储，避免使用id生成，因为调试时未上传的Mod的id无效
			return Steamworks_PathDefinition.Data_Save_ItemLocalDirPath + "/" + workshopItemInfo.DirName() + "/" + Steamworks_PathDefinition.persistentFolderName;
		}
		public static string LogDirPath(this WorkshopItemInfo workshopItemInfo)
		{
			return Steamworks_PathDefinition.Data_Save_LogDirPath + "/" + workshopItemInfo.DirName();

		}
		/// <summary>
		/// Item 本地文件夹名称
		/// </summary>
		/// <param name="workshopItemInfo"></param>
		/// <returns></returns>
		public static string DirName(this WorkshopItemInfo workshopItemInfo)
		{
			return PathTool.GetDirectoryName(workshopItemInfo.dirPath);
		}
	}
}