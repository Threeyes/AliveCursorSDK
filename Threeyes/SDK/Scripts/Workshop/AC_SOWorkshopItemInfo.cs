using System.Collections.Generic;
using UnityEngine;
using Threeyes.Steamworks;
/// <summary>
/// 管理编辑器中WorkshopItem的信息
/// 
/// PS：
/// 1.仅用于上传/编辑器调试Mod时存储信息
/// 
///Todo:
///1.删减对非必要库的依赖
///2.该物体在运行时不需要包括在程序中，所以可以放在编辑器文件中，不让用户访问源码(非必须）
/// </summary>
[CreateAssetMenu(menuName = AC_EditorDefinition.AssetMenuPrefix_Root_Workshop + "ItemInfo", fileName = "WorkshopItemInfo", order = -100)]
public class AC_SOWorkshopItemInfo : SOWorkshopItemInfo<AC_WorkshopItemInfo>
{
    public override AC_WorkshopItemInfo ItemInfo { get { return AC_WorkshopItemInfoFactory.Instance.Create(this); } }

    public AC_WSItemAgeRating ageRatingType = AC_WSItemAgeRating.General;
    public AC_WSItemStyle itemStyle = AC_WSItemStyle.None;
    public AC_WSItemGenre itemGenre = AC_WSItemGenre.None;
    public AC_WSItemReference itemReference = AC_WSItemReference.None;
    public AC_WSItemFeature itemFeature = AC_WSItemFeature.None;

    [HideInInspector] public AC_WSItemAdvance itemSafety = AC_WSItemAdvance.None;

    public override string[] Tags
    {
        get
        {
            List<string> listTag = new List<string>();
            listTag.Add(ageRatingType.ToString());//必选唯一

            listTag.AddRange(itemStyle.GetNamesEx());
            listTag.AddRange(itemGenre.GetNamesEx());
            listTag.AddRange(itemReference.GetNamesEx());
            listTag.AddRange(itemFeature.GetNamesEx());
            listTag.AddRange(itemSafety.GetNamesEx());

            return listTag.ToArray();
        }
    }

}