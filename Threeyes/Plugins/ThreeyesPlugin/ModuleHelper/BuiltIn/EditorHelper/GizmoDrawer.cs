using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 绘制模型
/// </summary>
public class GizmoDrawer : GameObjectBase
{
    public DrawMeshType drawMeshType = DrawMeshType.WireMesh;
    public Mesh meshToDraw;
    public Color color = new Color(0, 1, 1, 0.03125f);
    public Vector3 localEulerAngles;
    public Vector3 scale = Vector3.one;
    public float scaleRatio = 1f;//快速缩放

    void OnDrawGizmos()
    {
        if (!meshToDraw)
            return;
        Gizmos.color = color;
        Vector3 pos = tfThis.position;
        Quaternion rot = tfThis.rotation * Quaternion.Euler(localEulerAngles);
        Vector3 sca = scale * scaleRatio;
        switch (drawMeshType)
        {
            case DrawMeshType.WireMesh:
                Gizmos.DrawWireMesh(meshToDraw, pos, rot, sca); break;
            case DrawMeshType.Mesh:
                Gizmos.DrawMesh(meshToDraw, pos, rot, sca); break;
        }
    }

    [System.Serializable]
    public enum DrawMeshType
    {
        Mesh,
        WireMesh,
    }
}