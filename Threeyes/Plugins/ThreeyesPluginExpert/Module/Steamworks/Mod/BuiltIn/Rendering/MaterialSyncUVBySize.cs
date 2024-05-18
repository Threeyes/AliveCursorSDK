using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Threeyes.Steamworks
{
    /// <summary>
    /// 根据模型的缩放，同步UV的Scale
    /// 
    /// 适用于：
    /// -洞洞板等需要表面重复且贴图不变的模型
    /// -物体的X、Y轴对应UV轴
    /// 
    /// ToAdd：
    /// -参考此实现，增加基于位置更改UVOffset的组件
    /// 
    /// PS:
    /// -因为Awake在反序列化还原之前执行，所以能正常保存Prefab实例化后的默认缩放值，从而正常处理被缩放过的实例物体
    /// </summary>
    public class MaterialSyncUVBySize : MonoBehaviour
    {
        public Material Material
        {
            get
            {
                if (targetRenderer)
                    return targetRenderer.material;
                return targetMaterial;
            }
        }
        [SerializeField] protected Renderer targetRenderer;//Where the material attached
        [SerializeField] protected Material targetMaterial;//Target material asset
        public string textureName = "_BaseMap";
        public Vector2 uvScalePerUnit = new Vector2(1, 1);//每单元对应的UV Scale值

        //Runtime
        Vector3 curSize;
        Transform cacheTf;//缓存Rigidbody的Transform
        private void Awake()
        {
            cacheTf = targetRenderer.transform;
            curSize = transform.lossyScale;
        }
        private void LateUpdate()
        {
            if (transform.lossyScale != curSize)//仅当尺寸变化时（包括开始时的变化）才会更改材质，避免创建多余的克隆体
            {
                if (!Material)
                    return;

                curSize = transform.lossyScale;
                Material.SetTextureScale(textureName, curSize * uvScalePerUnit);
            }
        }

        #region Editor
#if UNITY_EDITOR
        [ContextMenu("UpdateConfigUsingComponentData")]
        void EditorUpdateConfigUsingComponentData()
        {
            if (Application.isPlaying)
            {
                Debug.LogError("Can't set this on play mode!");
                return;
            }
            Material targetMat = targetRenderer ? targetRenderer.sharedMaterial : targetMaterial;//避免Unity克隆材质
            uvScalePerUnit = targetMat.GetTextureScale(textureName) / targetRenderer.transform.lossyScale;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
        #endregion
    }
}