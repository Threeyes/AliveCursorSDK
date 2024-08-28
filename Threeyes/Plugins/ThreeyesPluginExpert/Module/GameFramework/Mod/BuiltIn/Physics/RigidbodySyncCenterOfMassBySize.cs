using System.Collections;
using System.Collections.Generic;
using Threeyes.Core;
using UnityEngine;
/// <summary>
/// 检测模型缩放事件，并更新Rigidbody的自定义CenterOfMass
/// 
/// PS:
/// -仅适用于自定义CenterOfMass
/// -Automatic Center Of Mass: 激活该选项，能够自动根据物体缩放计算重心质量，否则需要自行计算（Enable to use the physics system’s predicted center of mass for the Rigidbody, based on its 【shape and scale】. Disable to set your own X, Y and Z coordinates for the center of mass.）
/// -自定义CenterOfMass不受物体缩放影响， 所以需要挂载该组件，用于确保物体缩放后的重心一致（Note: centerOfMass is relative to the transform's position and rotation, but will not reflect the transform's scale!）（https://docs.unity3d.com/ScriptReference/Rigidbody-centerOfMass.html）
/// </summary>
namespace Threeyes.GameFramework
{
    public class RigidbodySyncCenterOfMassBySize : ComponentHelperBase<Rigidbody>
    {
        //# Config
        public Vector3 centerOfMassPerUnit = new Vector3(0, 0, 0);//每单元对应的UV Scale值

        //# Runtime
        Vector3 curSize;
        Transform cacheTf;//缓存Rigidbody的Transform

        private void Awake()
        {
            if (!Comp)
            {
                Debug.LogError($"{nameof(Comp)} not set!");
                return;
            }
            cacheTf = Comp.transform;
            curSize = cacheTf.lossyScale;
        }

        private void LateUpdate()
        {
            if (!(Comp && cacheTf))
                return;

            if (cacheTf.lossyScale != curSize)
            {
                if (Comp.automaticCenterOfMass)
                    return;

                //Debug.LogError("Test Update!" + curSize + "  " + transform.lossyScale);
                curSize = transform.lossyScale;
                SetComponentValue();
            }
        }

        private void SetComponentValue()
        {
            Vector3 value = centerOfMassPerUnit;
            value.Scale(curSize);
            Comp.centerOfMass = value;
        }

        #region Editor
#if UNITY_EDITOR
        [ContextMenu("UpdateConfigUsingComponentData")]
        void EditorUpdateConfigUsingComponentData()
        {
            centerOfMassPerUnit = Comp.centerOfMass;//假设物体的当前全局缩放为1
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
        #endregion
    }
}