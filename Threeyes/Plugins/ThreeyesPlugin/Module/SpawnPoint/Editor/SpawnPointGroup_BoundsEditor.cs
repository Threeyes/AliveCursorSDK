#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Threeyes.SpawnPoint
{
    [CustomEditor(typeof(SpawnPointGroup_Bounds)), CanEditMultipleObjects]
    public class SpawnPointGroup_BoundsEditor : UnityEditor.Editor
    {
        private BoxBoundsHandle m_BoundsHandle = new BoxBoundsHandle();

        // the OnSceneGUI callback uses the Scene view camera for drawing handles by default
        protected virtual void OnSceneGUI()
        {
            SpawnPointGroup_Bounds inst = (SpawnPointGroup_Bounds)target;

            Vector3 position = inst.transform.position;
            Vector3 size = inst.boundsSize;
            Handles.color = Color.cyan;
            DrawRotatedBoxBoundsHandle(m_BoundsHandle, ref position, inst.transform.rotation, ref size);
            inst.transform.position = position;
            inst.boundsSize = size;
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
        /// <param name="size"></param>
        public static void DrawRotatedBoxBoundsHandle(BoxBoundsHandle m_BoundsHandle, ref Vector3 position, Quaternion rotation, ref Vector3 size)
        {
            Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);
            using (new Handles.DrawingScope(matrix))
            {
                m_BoundsHandle.center = matrix.inverse.MultiplyPoint3x4(position);
                m_BoundsHandle.size = size;

                EditorGUI.BeginChangeCheck();
                m_BoundsHandle.DrawHandle();
                if (EditorGUI.EndChangeCheck())
                {
                    position = matrix.MultiplyPoint3x4(m_BoundsHandle.center);
                    size = m_BoundsHandle.size;
                }
            }
        }
    }
}
#endif