using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// RemoteDestinationPoint管理器
/// </summary>
public class RemoteDestinationPointManager : InstanceBase<RemoteDestinationPointManager>
{
    #region Networking

    public bool IsCommandMode { get { return isCommandMode; } set { isCommandMode = value; } }//Set to true to invoke Action instead
    private bool isCommandMode = false;
    public Action<RemoteDestinationPoint, Vector3?, bool> actCommandSetPos;

    public Action<RemoteDestinationPoint, Vector3?, bool> actRealSetPos;


    public Transform tfRoot;//存放所有DP的根物体（ToDelete,改为从listRDP中查找）

    #endregion


    public float GlobalHeight { get { return globalHeight; } set { globalHeight = value; } }
    public float globalHeight = 1.7f;


    /// <summary>
    /// 重新传送到当前DP（建议在设置GlobalHeight后调用该方法，重设玩家的高度）
    /// </summary>
    public void TeleportToCurDP()
    {
        var dp = RemoteDestinationPoint.CurrentDestinationPoint;
        if (dp != null)
            dp.SetPos();
    }
}
