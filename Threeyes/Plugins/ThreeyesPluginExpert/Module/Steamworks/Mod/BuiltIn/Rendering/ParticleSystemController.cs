using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Threeyes.Core;
namespace Threeyes.Steamworks
{
    /// <summary>
    /// Manage ParticleSystem
    /// 
    /// PS:
    /// -目前仅作为RuntimeEditor标识某根物体的主要用途，后续为其添加特定功能
    /// </summary>
    public class ParticleSystemController : MonoBehaviour
    {
        [SerializeField] ParticleSystem mainParticleSystem;

        private void Start()
        {
            //RuntimeGizmo
            if (preGizmoIndicator)//创建对应实例
            {
                preGizmoIndicator.InstantiatePrefab(transform);//默认在原点
            }
        }


        [Header("RuntimeGizmo")]//用于运行时选中
        public GameObject preGizmoIndicator;//挂载RuntimeEditorBehaviour等组件，会自动据当前是否为编辑状态进行显隐
    }
}
