#if UNITY_EDITOR
#if UNITY_2020_1_OR_NEWER
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
using UnityEngine.Events;
/// <summary>
/// ��дPackageManager����Ϣ
/// #ToUPdate��ֱ�Ӵ�manifest��ȡ
/// Ref��https://docs.unity3d.com/Manual/upm-api.html
/// </summary>
public static class PackageManagerHelper
{
    static ListRequest Request;


    /// <summary>
    /// (PS:Ϊ�˱�֤eventֻ����һ�Σ���Ҫ��-=��+=��https://stackoverflow.com/questions/367523/how-to-ensure-an-event-is-only-subscribed-to-once
    /// </summary>
    public static UnityAction<ListRequest, List<PackageInfo>> tempActionOnComplete;


    /// <summary>
    /// Warning:MenuItem��ע�ķ�����Ӧ���в���
    /// </summary>
    /// <param name="actionOnUpdate"></param>
    [UnityEditor.MenuItem(EditorDefinition.TopMenuItemPrefix + "PackageManagerHelper/" + "��ӡPackage")]
    public static void GetListAsync()
    {
        Request = Client.List();
        UnityEditor.EditorApplication.update += Progress;
    }

    static void Progress()
    {
        List<PackageInfo> listPackageInfo = new List<PackageInfo>();
        if (Request.IsCompleted)
        {
            if (Request.Status == StatusCode.Success)
            {
                foreach (PackageInfo package in Request.Result)
                    listPackageInfo.Add(package);
            }
            else if (Request.Status >= StatusCode.Failure)
            {
                //Debug.Log(Request.Error.message);
            }

            UnityEditor.EditorApplication.update -= Progress;
        }
        tempActionOnComplete.Execute(Request, listPackageInfo);
    }
}
#endif
#endif