#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AC_GizmoDrawer : InstanceBase<AC_GizmoDrawer>
{
	public DrawMeshType drawMeshType = DrawMeshType.WireMesh;
	public Mesh meshToDraw;
	public Color color = new Color(0, 1, 1, 0.03125f);
	public Vector3 localEulerAngles;
	public Vector3 scale = Vector3.one;
	public float scaleRatio = 1f;//全局缩放

	void OnDrawGizmos()
	{
		if (Application.isPlaying)
			return;

		if (!meshToDraw)
			return;
		Gizmos.color = color;
		Vector3 pos = transform.position;
		Quaternion rot = transform.rotation * Quaternion.Euler(localEulerAngles);
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
#endif