using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorTool
{
    /// <summary>
    /// 计算两个向量超过180的夹角
    /// Ref:https://forum.unity.com/threads/how-to-get-a-360-degree-vector3-angle.42145/#post-2777468
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static float Angle360(Vector3 from, Vector3 to, Vector3 right)
    {
        float angle = Vector3.Angle(from, to);
        float angle2 = (Vector3.Angle(right, to) > 90f) ? 360f - angle : angle;
        //Debug.LogError($"{from}->{to}:" + angle + "_" + angle2);
        return angle2;
    }

}
