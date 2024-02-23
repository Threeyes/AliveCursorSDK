using UnityEngine;

using System;
using System.Threading.Tasks;

using Steamworks;
using Steamworks.Data;
using UgcEditor = Steamworks.Ugc.Editor;
using Threeyes.Core;

namespace Threeyes.Steamworks
{
    public sealed class WorkshopItemUploader
    {
        //(Bug:弄成Interface无法正常传送数据，因此暂不需要为AC_SOWorkshopItemInfo提炼出接口）
        public static async Task<string> RemoteUploadItem(SOWorkshopItemInfo soInfo, Action<float> onUploadProcessUpdate, string changeLog = null)
        {
            //PS:错误还是在Console打印比较好，方便复制和查看
            string errorLog = null;
            //——检查Steam是否激活——
            if (!SteamClient.IsValid)
            {
                try
                {
                    var steamAppID = SORuntimeSettingManager.Instance.steamAppID;
                    SteamClient.Init(steamAppID, true);
                }
                catch (Exception e)
                {
                    errorLog = "SteamClient Init failed with error:\r\n" + e;
                }
            }

            /////（Bug，暂不使用）：ModDirectory类在UMod命名空间，而SDK中不包含此Dll。改为SDK打包时检查，或直接检查根文件夹是否含有任意cs文件(命名空间)；也可以改为运行时检查
            ////ToUpdate:查看ItemManager是怎样获取信息的，应该有其他方式。如通过ExportProfileSettings查看
            ////ToUpdate2:可以改为下载后进行验证，如果tag跟ModInfo不一致则报错
            ////——根据Mod的信息，修改Tag——
            ////PS:为了避免Modder通过手动更改该字段作弊，因此在上传前才设置Safety枚举，而且封装为Dll
            //try
            //{
            //             //#检查Mod有没有脚本，如果有则更新Json对应的Flag
            //             FileInfo fileInfoMod = new FileInfo(soInfo.ExportItemModFilePath);
            //             if (fileInfoMod.Exists && UMod.ModDirectory.IsModFile(fileInfoMod))//PS：已经提前检查过有无该Json文件，因此不需要报错
            //             {
            //                 ModContent modContent = UMod.ModDirectory.GetMod(fileInfoMod).GetModContentMask();
            //                 soInfo.itemSafety = modContent.Has(ModContent.Scripts) ? AC_WCItemSafety.IncludeScripts : AC_WCItemSafety.None;
            //                 EditorUtility.SetDirty(soInfo);

            //                 //#强制重新生成Json文件（不需要检查是否匹配，避免同时读写同一个文件导致占用错误）
            //                 var json = JsonConvert.SerializeObject(soInfo.ItemInfo, Formatting.Indented);
            //                 File.WriteAllText(soInfo.ExportItemInfoFilePath, json);
            //             }
            //         }
            //         catch (Exception e)//PS：不影响后续操作
            //         {
            //             errorLog = "Update Json File failed!\r\n" + e;
            //         }

            if (errorLog.NotNullOrEmpty())
            {
                return errorLog;//Steam初始化失败，直接跳出
            }

            //——开始上传——
            //#1：确定是创建还是更新
            bool isItemUploaded = soInfo.IsItemUploaded;
            UgcEditor uploadRequest = isItemUploaded ? new UgcEditor(soInfo.ItemID) : UgcEditor.NewCommunityFile;//创建Item。这一步会设置creatingNew=true

            //#2：设置参数（PS除了Content外，其他都是使用Unity内部的信息，避免频繁读取Json文件）（缺点是需要打包后才能上传）
            //Set Basic info
            uploadRequest = uploadRequest
                .WithTitle(soInfo.Title)
                .WithDescription(soInfo.Description)
                .WithContent(soInfo.ContentDirPath)
                .WithPreviewFile(soInfo.PreviewFilePath);

            //Set Visibility
            switch (soInfo.ItemVisibility)
            {
                case WSItemVisibility.Public:
                    uploadRequest = uploadRequest.WithPublicVisibility(); break;
                case WSItemVisibility.FriendsOnly:
                    uploadRequest = uploadRequest.WithFriendsOnlyVisibility(); break;
                case WSItemVisibility.Private:
                    uploadRequest = uploadRequest.WithPrivateVisibility(); break;
            }

            //Set Tag(PS: will replace all tags）
            foreach (var tag in soInfo.Tags)
            {
                uploadRequest = uploadRequest.WithTag(tag);
            }
            ///SetChangeLog。
            if (changeLog.NotNullOrEmpty())
                uploadRequest.WithChangeLog(changeLog);
            ///ToAdd: 
            ///2.metaData：这些元数据可从查询返回，而无需下载安装实际内容。

            //#3：上传
            try
            {
                var TotalUploadresult = await uploadRequest.SubmitAsync(new ProgressClass(onUploadProcessUpdate),
                    (createResult) =>
                    {
                        //PS：用户首次创建时，需要检查他是否签约了上传协议，如果没有则Item会默认隐藏。这时需要提示他打开Item网址，（https://steamcommunity.com/workshop/workshoplegalagreement），具体呈现方式可看网站(https://partner.steamgames.com/doc/features/workshop/implementation#Legal）：

                        //<Workshop items will be hidden by default> until the contributor agrees to the Steam Workshop Legal Agreement. In order to make it easy for the contributor to make the item publicly visible, please do the following.
                        //Include text next to the button that submits an item to the workshop, something to the effect of: "By submitting this item, you agree to the workshop terms of service"(including the link)
                        //After a user submits an item, open a browser window to the Steam Workshop page for that item by calling ISteamFriends::ActivateGameOverlayToWebPage with pchURL set to steam://url/CommunityFilePage/<PublishedFileId_t> replacing <PublishedFileId_t> with the workshop item id.
                        //This has the benefit of directing the author to the workshop page so that they can see the item and configure it further if necessary and will make it easy for the user to read and accept the Steam Workshop Legal Agreement.
                        if (createResult.NeedsWorkshopAgreement)
                        {
                            Debug.Log("Item create completed but will be hidden by default until you agrees to the Steam Workshop Legal Agreement (https://steamcommunity.com/workshop/workshoplegalagreement), you can do this later on item's Steam Workshop page.");//提醒用户自行打开Item页面，以便接收许可
                            try
                            {
                                //自动帮用户打开确认Agreement的链接，避免用户忘记确认
                                string itemUrl = WorkshopItemTool.GetUrl(createResult.FileId.Value);
                                if (itemUrl != null)
                                    Application.OpenURL(itemUrl);
                            }
                            catch (Exception e)
                            {
                                Debug.LogError("Try to open agreement page failed: " + e);
                            }
                        }
                        //Debug.Log("Item Created");
                    });

                if (TotalUploadresult.Result != Result.OK)
                {
                    errorLog = $"Upload {soInfo.Title} failed with error code: #{(int)TotalUploadresult.Result}({TotalUploadresult.Result})";

                    //PS:场景上传常见错误情况（https://partner.steamgames.com/doc/api/ISteamUGC#SubmitItemUpdateResult_t）：
                    //Todo:提取出一个通用的错误查询方法
                    switch (TotalUploadresult.Result)
                    {
                        case Result.InvalidParam://8 
                            errorLog += "\r\n" + "Either the provided app ID is invalid or doesn't match the consumer app ID of the item or, you have not enabled ISteamUGC for the provided app ID on the Steam Workshop Configuration App Admin page."; break;
                        case Result.AccessDenied:
                            errorLog += "\r\n" + "The user doesn't own a license for the provided app ID."; break;
                        case Result.FileNotFound:
                            errorLog += "\r\n" + "Failed to get the workshop info for the item or failed to read the preview file, check if the remote item is deleted."; break;
                        case Result.LockingFailed:
                            errorLog += "\r\n" + "Failed to aquire UGC Lock."; break;
                        case Result.LimitExceeded://LimitExceeded：预览图太大
                            errorLog += "\r\n" + "The preview image is too large, it must be less than 1 Megabyte; or there is not enough space available on the users Steam Cloud."; break;
                    }
                    errorLog += "\r\n Check  <color=cyan>https://partner.steamgames.com/doc/api/ISteamUGC#SubmitItemUpdateResult_t</color>  for more detail.";
                }
                else
                {
                    if (soInfo.ItemID == 0)
                    {
                        PublishedFileId fileId = TotalUploadresult.FileId;
                        soInfo.ItemID = fileId.Value;//保存ID，用于标记是否已经上传
                        Debug.Log($"Create { soInfo.Title} Complete! ID is {soInfo.ItemID}");
                    }
                    else
                    {
                        Debug.Log($"Upload {soInfo.Title} Complete!");
                    }
                    soInfo.SetAsDirty();//需要调用该方法保存更改
                }
            }
            catch (Exception e)
            {
                errorLog = "Upload failed with error:\r\n" + e;
            }
            return errorLog;
        }

        class ProgressClass : IProgress<float>
        {
            float lastvalue = 0;
            Action<float> onProcessUpdate;
            public ProgressClass(Action<float> tempOnProcessUpdate)
            {
                onProcessUpdate = tempOnProcessUpdate;
            }
            public void Report(float value)
            {
                if (lastvalue >= value) return;

                lastvalue = value;
                onProcessUpdate.Execute(value);
            }
        }
    }
}