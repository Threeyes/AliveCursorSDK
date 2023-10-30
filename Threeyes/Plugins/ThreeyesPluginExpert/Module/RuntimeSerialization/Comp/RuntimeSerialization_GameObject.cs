using System.Collections.Generic;
using UnityEngine;
using Threeyes.Data;
using System.Linq;
using Newtonsoft.Json;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Threeyes.RuntimeSerialization
{
    /// <summary>
    /// 序列化物体必须要有的组件，存储GameObject及组件的相关信息
    /// Ref:Unity.RuntimeSceneSerialization.Json.Adapters.JsonAdapter.GameObject
    /// 
    /// PS：
    /// -放在场景中的不算Prefab，所以不需要初始化其PrefabMetadata字段。PrefabMetadata的作用是重新生成Prefab
    /// 
    /// ToUpdate:
    /// -在GameObject没有唯一标识的情况下，建议序列的物体组要不全实例，要不全通过Prefab生成，否则容易在反序列化时出现混淆
    /// </summary>
    public class RuntimeSerialization_GameObject : RuntimeSerializationBehaviour<GameObject, GameObjectPropertyBag>
    {
        //#Cache以下二选一：
        [SerializeField] public PrefabMetadata cachePrefabMetadata = new PrefabMetadata();//【运行时设置】【序列化时优先使用】缓存运生成Prefab的信息（该字段用于后续序列化时提供给GameObjectPropertyBag）
        [SerializeField] public InstanceMetadata cacheInstanceMetadata = new InstanceMetadata();//【编辑器非运行时设置】缓存已存在实例的信息（Prefab或NotAPrefab）（即使与cachePrefabMetadata同时设置也没关系，因为会优先考虑cachePrefabMetadata）（PS：Prefab对应实例中该字段会提示需要Apply，可以忽略）

#if UNITY_EDITOR
        /// <summary>
        /// 如果cacheInstanceMetadata的数据没有保存到Scene，则调用该方法
        /// </summary>
        [ContextMenu(" EditorUtility.SetDirty")]
        void EditorSetDirty()
        {
            EditorUtility.SetDirty(this);
        }
        /// <summary>
        /// Editor-only function that Unity calls when the script is loaded or a value changes in the Inspector.
        /// 
        /// </summary>
        private void OnValidate()
        {
            if (Application.isPlaying)//运行时不需要生成，因为只需要处理预先摆放的物体
                return;

            if (BuildPipeline.isBuildingPlayer)//打包时不进入
                return;
            if (!transform.gameObject)
                return;
            if (!transform.gameObject.scene.IsValid())
                return;
            if (!transform.gameObject.scene.isLoaded)//有可能打包时会进入该方法，通过检查是否Load可避免报错
                return;


            //——以下只针对场景实例，确保每个物体在对应场景中的UID唯一——
            //#存储编辑器非运行模式下生成的实例
            bool isInstance = IsInstance(gameObject);//检查该物体是否为实例
            //Debug.LogError($"State: {gameObject.scene.name}_{gameObject.scene.rootCount}_{gameObject.name}_IsInstance:{isInstance}");

            if (isInstance)//当该物体是场景实例：为其设置新的GUID
            {
                bool shouldGenerateNewGUID = !cacheInstanceMetadata.IsValid || cacheInstanceMetadata.IsValid && !IsInstanceGUIDUniqueInScene();//需要重新创建GUID：当前GUID无效，或者与同场景物体的GUID冲突
                if (shouldGenerateNewGUID)
                {
                    int retryCount = 0;
                    int maxRetryCount = 10;
                    do
                    {
                        cacheInstanceMetadata.Guid = InstanceMetadata.NewGuid();
                        retryCount++;
                    }
                    while (!IsInstanceGUIDUniqueInScene() && retryCount < maxRetryCount);
                    if (retryCount == maxRetryCount)
                    {
                        Debug.LogError($"Try to generate new GUID for [{gameObject}] failed! Reach max retry count {maxRetryCount}!");
                    }
                    else
                    {
                        Debug.LogWarning($"【Temp】Update {gameObject.scene.name}_{gameObject.scene.rootCount}_{gameObject.name}'s cacheInstanceMetadata to {cacheInstanceMetadata.Guid}");
                    }
                    EditorUtility.SetDirty(this);// mark as dirty, so the change will be save into scene file
                }
            }
            else
            {
                ///（以下非必须，只要cachePrefabMetadata有效就不影响反序列化）
                //清空cacheInstanceMetadata【Bug：在进入程序时会清楚实例的GUID，原因位置】
                if (cacheInstanceMetadata.IsValid)
                {
                    //针对本地的Prefab（包括从场景拖拽到Asset生成的Prefab：清空Instance信息
                    if (EditorUtility.IsPersistent(gameObject))
                    {
                        cacheInstanceMetadata.Guid = "";
                        Debug.LogWarning($"【Temp】Clear {gameObject.scene.name}_{gameObject.scene.rootCount}_{gameObject.name}'s cacheInstanceMetadata");
                        //EditorUtility.SetDirty(this);
                        PrefabUtility.RecordPrefabInstancePropertyModifications(this);
                    }
                }
            }
        }
        /// <summary>
        /// 检查物体是否为场景中的实例（包括Prefab）
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        bool IsInstance(GameObject go)
        {
            ///检查当前物体是否在Prefab Mode，如果是就代表仍然是PrefabAsset
            ///-Ref:NetworkIdentity.SetupIDs
            if (PrefabStageUtility.GetCurrentPrefabStage() && PrefabStageUtility.GetPrefabStage(go))
                return false;


            return !EditorUtility.IsPersistent(go);//Determines if an object is stored on disk.
            ///Warning:
            ///-当打开Prefab进行编辑时，以下属性会返回1（代表该预制物为Root）
            //return go.scene.rootCount != 0;
        }

        ///// <summary>
        ///// 本GUID是否在此层级唯一
        ///// </summary>
        ///// <returns></returns>
        //bool IsInstanceGUIDUniqueInHierarchy()
        //{
        //    if (!cacheInstanceMetadata.IsValid)//本GUID无效
        //        return false;

        //    if (!transform.parent)//无父物体
        //        return true;

        //    foreach (var rtsGO in transform.parent.GetComponentsInChildren<RuntimeSerialization_GameObject>())
        //    {
        //        if (rtsGO == this)//忽略自身
        //            continue;
        //        if (rtsGO.cacheInstanceMetadata.IsValid && rtsGO.cacheInstanceMetadata.Guid == cacheInstanceMetadata.Guid)
        //            return false;
        //    }
        //    return true;
        //}

        /// <summary>
        /// 本GUID是否在此场景唯一
        /// </summary>
        /// <returns></returns>
        bool IsInstanceGUIDUniqueInScene()
        {
            if (!cacheInstanceMetadata.IsValid)//本GUID无效
                return false;

            try
            {
                foreach (var rtsGO in transform.gameObject.scene.GetComponents<RuntimeSerialization_GameObject>())
                {
                    if (rtsGO == this)//忽略自身
                        continue;
                    if (rtsGO.cacheInstanceMetadata.IsValid && rtsGO.cacheInstanceMetadata.Guid == cacheInstanceMetadata.Guid)
                        return false;
                }
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Try create InstanceGUID for gameobject [{transform.gameObject.name}] failed! Error info:\r\n{e}");
            }
            return false;
        }

#endif

        #region Public

        /// <summary>
        /// 【Runtime】临时存储Prefab的信息，便于序列化时保存
        /// 
        /// 调用时刻：
        /// -该物体基于Prefab生成
        /// -反序列化时还原该数值
        /// </summary>
        /// <param name="m_Guid"></param>
        public void InitPrefabMetadata(string m_Guid)
        {
            cachePrefabMetadata = new PrefabMetadata(m_Guid);
        }
        #endregion

        //ToAdd：标记该物体的唯一ID，便于绑定。可以由开发者设置，也可以通过获取GUID进行自动指定（ReadOnly）

        protected override GameObject GetContainerFunc() { return gameObject; }

        public override GameObjectPropertyBag GetSerializePropertyBag()
        {
            //保存相关信息
            GameObjectPropertyBag propertyBag = base.GetSerializePropertyBag();
            if (cachePrefabMetadata.IsValid)
                propertyBag.prefabMetadata = new PrefabMetadata(cachePrefabMetadata.Guid);
            if (cacheInstanceMetadata.IsValid)
                propertyBag.instanceMetadata = new InstanceMetadata(cacheInstanceMetadata.Guid);

            //#1 扫描并存储该物体所有RTSComponent的序列化数据
            List<IRuntimeSerializationComponent> listRSComponent = gameObject.GetComponents<IRuntimeSerializationComponent>().ToList();
            //propertyBag.serializedComponents.AddRange(listRSComponent.ConvertAll(rsc => rsc.BaseSerializePropertyBag));
            propertyBag.serializedComponents.AddRange(listRSComponent.ConvertAll(rsc => rsc.OnSerialize()));

            //#2 扫描子物体
            foreach (Transform tfChild in container.transform)
            {
                RuntimeSerialization_GameObject rtsgChild = tfChild.GetComponent<RuntimeSerialization_GameObject>();
                if (rtsgChild)
                {
                    GameObjectPropertyBag propertyBagChild = rtsgChild.GetSerializePropertyBag();
                    propertyBag.children.Add(propertyBagChild);
                }
            }

            return propertyBag;
        }

        public override void OnDeserializeRaw(GameObjectPropertyBag propertyBag)
        {
            base.OnDeserializeRaw(propertyBag);

            //#1 反序列化自身所有组件
            //-将serializedComponents转为PropertyBag
            List<PropertyBag> listPropertyBagCom = propertyBag.serializedComponents.ConvertAll((s) => SerializedComponentToPropertyBag(s));
            //List<PropertyBag> listPropertyBagCom = propertyBag.serializedComponents;

            //-查找与序列化后的类型相同的组件，并传入对应序列化字段进行OnDeserialize（不直接使用反射的好处是，方便用户）
            //【ToUpdate】：
            //-针对Modder的脚本，只需要匹配类名即可，因为Mod打包后所在程序集名称不一样
            //-针对多个相同组件，需要按顺序匹配（如标记某个组件已经反序列化）
            //-【进阶】对组件进行排序（参考Unity.RuntimeSceneSerialization.Internal.SerializationUtils.SortComponentList）
            List<IRuntimeSerializationComponent> listRSComponent = gameObject.GetComponents<IRuntimeSerializationComponent>().ToList();
            foreach (var rsComponent in listRSComponent)
            {
                int matchedIndex = listPropertyBagCom.FindIndex(pB => pB.containerTypeName == PropertyBag.GetTypeName(rsComponent.ContainerType));
                if (matchedIndex != -1)
                {
                    rsComponent.OnDeserialize(propertyBag.serializedComponents[matchedIndex]);
                    //rsComponent.OnDeserializeBase(propertyBag.serializedComponents[matchedIndex]);
                }
            }

            //#2链接或重新生成所有子物体
            foreach (var childPropertyBag in propertyBag.children)
            {
                if (childPropertyBag.prefabMetadata.IsValid)//如果是Prefab：生成实例并反序列化（Warning：如果prefabMetadata为空，则无法正常生成对应预制物并还原配置。可能原因是二次序列化时没有保存好数据，意外使用了RuntimeSerialization_GameObject的cachePrefabMetadata）
                {
                    Transform tfParent = transform;//以该物体为父物体
                    string guid = childPropertyBag.prefabMetadata.Guid;
                    GameObject goInst = SOAssetPackManager.TryInstantiatePrefab(guid, tfParent);
                    //ToAdd:如果没有预制物，则生成占位物体

                    //#如果无法找到预制物，则会生成一个空占位Prefab
                    if (goInst == null)
                    {
                        Debug.LogWarning($"Failed to instantiate prefab with guid {guid}");
                        goInst = new GameObject();
                        goInst.transform.SetParent(tfParent);
                        goInst.name = $"Prefab Placeholder {guid}";
                    }


                    RuntimeSerialization_GameObject rtsgInst = goInst.GetComponent<RuntimeSerialization_GameObject>();
                    if (rtsgInst)
                    {
                        rtsgInst.OnDeserializeRaw(childPropertyBag);
                        rtsgInst.InitPrefabMetadata(childPropertyBag.prefabMetadata.Guid);//不管该值是否有效，统一用于初始化，便于后续序列化保存
                    }
                }
                else if (childPropertyBag.instanceMetadata.IsValid)//#如果是实例：查找匹配的InstanceMetadata直接并反序列化
                {
                    RuntimeSerialization_GameObject rtsgInst = transform.GetComponentsInChildren<RuntimeSerialization_GameObject>().FirstOrDefault(rtsg => rtsg.cacheInstanceMetadata.Guid == childPropertyBag.instanceMetadata.Guid);
                    if (rtsgInst)
                    {
                        rtsgInst.OnDeserializeRaw(childPropertyBag);
                    }
                    else
                    {
                        Debug.LogError($"Can't find instance for [{childPropertyBag.name}(GUID:{childPropertyBag.instanceMetadata.Guid})]!");
                    }
                }
                else
                {
                    Debug.LogError($"[{childPropertyBag.name}] doesn't have any valid metadata!");
                }
            }

            /////#通过排序进行反序列化【ToDelete】：改为用cacheInstanceMetadata进行匹配，匹配失败才用此排序方法匹配——
            /////#3 等上一步骤的Prefab全部生成后，根据siblingIndex进行排序，便于后续一一对应（【小Bug】：如果将实例与Prefab混放，如果Modder新增实例，则可能会对不上，因此建议不要混放，或者想办法增加唯一ID）
            //List<GameObjectPropertyBag> listSortPropertyBag = new List<GameObjectPropertyBag>(propertyBag.children);
            //listSortPropertyBag.Sort((a, b) => a.siblingIndex - b.siblingIndex);

            ////#4 按顺序反序列化所有子物体（针对已存在物体）
            //int indexRSG = 0;//当前反序列化的子物体序号
            //foreach (Transform tfChild in container.transform)
            //{
            //    if (indexRSG >= propertyBag.children.Count)
            //        break;
            //    RuntimeSerialization_GameObject rtsgChild = tfChild.GetComponent<RuntimeSerialization_GameObject>();
            //    if (rtsgChild)
            //    {
            //        GameObjectPropertyBag propertyBagChild = listSortPropertyBag[indexRSG];
            //        rtsgChild.OnDeserialize(propertyBagChild);
            //        rtsgChild.InitPrefabMetadata(propertyBagChild.prefabMetadata.Guid);//不管该值是否有效，统一用于初始化，便于后续序列化保存
            //        indexRSG++;
            //    }
            //}
        }


        ///ToAdd:
        ///-将Component序列化字段转为PropertyBag，以便获取其类型等信息
        public static PropertyBag SerializedComponentToPropertyBag(string serializedComponent)
        {
            return JsonConvert.DeserializeObject<PropertyBag>(serializedComponent);
        }
    }
}