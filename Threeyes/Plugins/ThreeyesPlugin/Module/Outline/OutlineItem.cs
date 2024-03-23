using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Threeyes.Core;
using UnityEngine;
namespace Threeyes.Outline
{
    /// <summary>
    /// 对单个模型起作用，不需要将模型设置为Readable
    /// 
    /// Ref: QuickOutline by Chris Nolet
    /// 
    /// PS：
    /// -仅在运行模式有效，主要时避免意外修改场景物体
    /// </summary>
    [DisallowMultipleComponent]
    public class OutlineItem : ElementBase<OutlineItem.ItemInfo>
    {
        #region Property
        readonly static string outlineMaskShaderName = "Threeyes/Outline/Mask";
        readonly static string outlineFillShaderName = "Threeyes/Outline/Fill";

        //public Mode OutlineMode
        //{
        //    get { return data.outlineMode; }
        //    set
        //    {
        //        data.outlineMode = value;
        //        needsUpdate = true;
        //    }
        //}
        //public Color OutlineColor
        //{
        //    get { return data.outlineColor; }
        //    set
        //    {
        //        data.outlineColor = value;
        //        needsUpdate = true;
        //    }
        //}
        //public float OutlineWidth
        //{
        //    get { return data.outlineWidth; }
        //    set
        //    {
        //        data.outlineWidth = value;
        //        needsUpdate = true;
        //    }
        //}
        //private bool needsUpdate;

        public bool autoInit = false;//使用已有数据初始化，适用于提前挂载

        //Runtime
        private Material outlineMaskMaterial;
        private Material outlineFillMaterial;
        private Renderer m_renderer;
        #endregion

        private void Start()
        {
            if (autoInit)
            {
                Init();
            }
        }
        public void Destroy()
        {
            Destroy(this);
        }

        protected override void OnDespawnFunc()
        {
            base.OnDespawnFunc();

            RemoveMaterial();//在Destroy时移除

            // Destroy material instances
            Destroy(outlineMaskMaterial);
            Destroy(outlineFillMaterial);
        }

        bool hasInit = false;
        readonly string outlineMaskMaterialName = "OutlineMask (Instance)";
        readonly string outlineFillMaterialName = "OutlineFill (Instance)";
        public override void InitFunc(ItemInfo data)
        {
            base.InitFunc(data);
            if (hasInit)
                return;
            // Instantiate outline materials
            outlineMaskMaterial = new Material(Shader.Find(outlineMaskShaderName));
            outlineFillMaterial = new Material(Shader.Find(outlineFillShaderName));

            outlineMaskMaterial.name = outlineMaskMaterialName;
            outlineFillMaterial.name = outlineFillMaterialName;

            m_renderer = GetComponent<Renderer>();
            AddMaterial();//在Init时加入
            UpdateMaterialProperties();

            hasInit = true;
        }

        public void AddMaterial()
        {
            if (!m_renderer)
                return;

            // Append outline shaders
            var materials = m_renderer.sharedMaterials.ToList();

            materials.Add(outlineMaskMaterial);
            materials.Add(outlineFillMaterial);

            m_renderer.materials = materials.ToArray();
        }
        public void RemoveMaterial()
        {
            if (!m_renderer)
                return;

            // Remove outline shaders（避免重复添加）
            var materials = m_renderer.sharedMaterials.ToList();
            materials.RemoveAll(m => m && m.shader && (m.shader.name == outlineMaskShaderName));
            materials.RemoveAll(m => m && m.shader && (m.shader.name == outlineFillShaderName));
            m_renderer.materials = materials.ToArray();
        }

        void OnValidate()
        {
            if (!Application.isPlaying)
                return;

            //// Update material properties when developer modify fields
            //needsUpdate = true;
            if (!hasInit)
                return;
            UpdateMaterialProperties();
        }

        //void Update()
        //{
        //    if (needsUpdate)
        //    {
        //        UpdateMaterialProperties();
        //        needsUpdate = false;
        //    }
        //}

        /// <summary>
        /// 每次设置Data后，都需要调用此方法
        /// </summary>
        public void UpdateMaterialProperties()
        {
            // Apply properties according to mode
            outlineFillMaterial.SetColor("_OutlineColor", data.outlineColor);

            switch (data.outlineMode)
            {
                case Mode.OutlineAll:
                    outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                    outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                    outlineFillMaterial.SetFloat("_OutlineWidth", data.outlineWidth);
                    break;

                case Mode.OutlineVisible:
                    outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                    outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                    outlineFillMaterial.SetFloat("_OutlineWidth", data.outlineWidth);
                    break;

                case Mode.OutlineHidden:
                    outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                    outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Greater);
                    outlineFillMaterial.SetFloat("_OutlineWidth", data.outlineWidth);
                    break;

                case Mode.OutlineAndSilhouette:
                    outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                    outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                    outlineFillMaterial.SetFloat("_OutlineWidth", data.outlineWidth);
                    break;

                case Mode.SilhouetteOnly:
                    outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                    outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Greater);
                    outlineFillMaterial.SetFloat("_OutlineWidth", 0f);
                    break;
            }
        }

        #region Define
        [System.Serializable]
        public class ItemInfo
        {
            public Mode outlineMode = Mode.OutlineAll;
            public Color outlineColor = Color.white;
            [Range(0f, 10f)] public float outlineWidth = 2f;

            public ItemInfo()
            {
                //如果不设置字段，则使用其默认值
            }

            public ItemInfo(Mode outlineMode, Color outlineColor, float outlineWidth)
            {
                this.outlineMode = outlineMode;
                this.outlineColor = outlineColor;
                this.outlineWidth = outlineWidth;
            }
        }
        public enum Mode
        {
            OutlineAll,
            OutlineVisible,
            OutlineHidden,
            OutlineAndSilhouette,
            SilhouetteOnly//【轮廓】
        }
        #endregion
    }
}