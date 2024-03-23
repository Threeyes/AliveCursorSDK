using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threeyes.Steamworks
{
    /// <summary>
    /// PS:
    /// -该物体需要包含Trigger，且水平面对应物体中心和Trigger顶部
    /// 
    /// ToAdd：
    /// -可以选择是否再OnTrigger时更新，适用于可以移动的Container
    /// -可选是否自动为掉进的物体增加BuoyantObject组件
    /// 
    /// Bug:
    /// -会导致附着在Socket上的物体异常抖动
    /// </summary>
    public class BuoyantContainer : MonoBehaviour
    {
        public Renderer m_Renderer;
        public bool isContinuousUpdate = false;//持续更新

        //Runtime
        bool hasInited = false;
        Material material;
        float steepness;
        float wavelength;
        float speed;
        Vector4 directions;
        void Start()
        {
            if (m_Renderer)
            {
                material = m_Renderer.material;
                steepness = material.GetFloat("_Wave_Steepness");
                wavelength = material.GetFloat("_Wave_Length");
                speed = material.GetFloat("_Wave_Speed");
                directions = material.GetVector("_Wave_Directions");
                hasInited = true;
            }
            else
            {
                Debug.LogError($"{nameof(m_Renderer)} is null!");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            SearchAndUpdate(other);
        }
        private void OnTriggerStay(Collider other)
        {
            if (!isContinuousUpdate)
                return;
            SearchAndUpdate(other);
        }
        private void SearchAndUpdate(Collider other)
        {
            if (!hasInited)
                return;
            if (other == null)
                return;
            Rigidbody rigidbody = other.attachedRigidbody;
            if (rigidbody == null)
                return;

            BuoyantObjectController buoyantObject = rigidbody.GetComponent<BuoyantObjectController>();
            if (!buoyantObject)
                return;

            buoyantObject.waterHeight = transform.position.y;

            buoyantObject.steepness = steepness;
            buoyantObject.wavelength = wavelength;
            buoyantObject.speed = speed;
            buoyantObject.directions = directions;
        }
    }
}