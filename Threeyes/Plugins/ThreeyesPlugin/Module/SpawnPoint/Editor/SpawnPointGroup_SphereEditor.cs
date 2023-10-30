#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Threeyes.SpawnPoint
{
    [CustomEditor(typeof(SpawnPointGroup_Sphere)), CanEditMultipleObjects]
    public class SpawnPointGroup_SphereEditor : UnityEditor.Editor
    {
        private SphereBoundsHandle m_BoundsHandle = new SphereBoundsHandle();

        // the OnSceneGUI callback uses the Scene view camera for drawing handles by default
        protected virtual void OnSceneGUI()
        {
            SpawnPointGroup_Sphere inst = (SpawnPointGroup_Sphere)target;

            Vector3 position = inst.transform.position;
            float radius = inst.sphereRadius;
            Handles.color = Color.cyan;
            DrawRotatedBoxBoundsHandle(m_BoundsHandle, ref position, inst.transform.rotation, ref radius);
            inst.transform.position = position;
            inst.sphereRadius = radius;
        }

        //ToDo：提炼成通用
        /// <summary>
        /// 绘制BoxBoundsHandle，能够适配物体位置、旋转
        /// 
        /// Ref：https://discussions.unity.com/t/boxboundshandle-rotation/185687
        /// </summary>
        /// <param name="m_BoundsHandle"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="radius"></param>
        /*    public static*/
        void DrawRotatedBoxBoundsHandle(SphereBoundsHandle m_BoundsHandle, ref Vector3 position, Quaternion rotation, ref float radius)
        {
            Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);
            using (new Handles.DrawingScope(matrix))
            {
                m_BoundsHandle.center = matrix.inverse.MultiplyPoint3x4(position);
                m_BoundsHandle.radius = radius;

                EditorGUI.BeginChangeCheck();
                m_BoundsHandle.DrawHandle();
                if (EditorGUI.EndChangeCheck())
                {
                    position = matrix.MultiplyPoint3x4(m_BoundsHandle.center);
                    radius = m_BoundsHandle.radius;
                }
            }
        }
    }
}
#endif