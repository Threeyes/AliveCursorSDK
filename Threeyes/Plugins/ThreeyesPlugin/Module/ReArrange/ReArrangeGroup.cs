using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 物体重排位置
/// 
/// 注意：
/// ——重排默认不修改层级
/// 可选：
/// ——自身重排
/// ——在可用路径下重排
/// 
/// </summary>
public class ReArrangeGroup : ComponentGroupBase<ReArrangeTarget>
{
    public List<ReArrangePoint> listRearrangePoint = new List<ReArrangePoint>();//目标点（可选）

    [ContextMenu("ReArrange")]
    /// <summary>
    /// 重新排列
    /// </summary>
    public void ReArrange()
    {
        var listRearrangePointLeft = listRearrangePoint.SimpleClone();
        foreach (var com in ListComp)
        {
            var targetPoint = listRearrangePointLeft.GetRandom();
            if (targetPoint)
            {
                //SetPos
                com.SetPos(targetPoint);
            }
            else
            {
                Debug.LogError("RearrangePoint数量不足！");
            }
            listRearrangePointLeft.Remove(targetPoint);
        }

    }

}
