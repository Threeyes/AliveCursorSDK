using System.Collections.Generic;
using UnityEngine;
using Threeyes.Data;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using Threeyes.Core;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using Threeyes.Core.Editor;
#endif

namespace Threeyes.RuntimeSerialization
{
    /// <summary>
    /// 物体想序列化必须要有的组件，存储GameObject及组件的相关信息
    /// Ref:Unity.RuntimeSceneSerialization.Json.Adapters.JsonAdapter.GameObject
    /// 
    /// PS：
    /// -放在场景中的不算Prefab，所以不需要初始化其prefabID字段。prefabID的作用是重新生成Prefab
    /// -因为该类不需要子类继承，所以暂时不需要继承类似IRuntimeSerializableComponent的接口
    /// 
    /// -在GameObject没有唯一标识的情况下，建议序列的物体组要不全实例，要不全通过Prefab生成，否则容易在反序列化时出现混淆
    /// </summary>
    [DisallowMultipleComponent]
    public sealed partial class RuntimeSerializable_GameObject : RuntimeSerializableUnityObject<GameObject, GameObjectPropertyBag>
    {
        #region ID  
        public override Identity ID { get { return cacheInstanceID; } set { } }//暂不允许设置（继承IIdentityHolder接口仅用于使用RuntimeSerializationTool相关方法，平时不应使用！）

        ///——缓存该物体的唯一ID，便于绑定。可以由开发者设置，也可以通过获取GUID进行自动指定（ReadOnly）——

        //#以下字段二选一来使用：
        public Identity CacheInstanceID { get { return cacheInstanceID; } }
        public Identity CachePrefabID { get { return cachePrefabID; } }
        public string CacheScope { get { return cacheScope; } }

        [SerializeField] Identity cacheInstanceID = new Identity();//【编辑器非运行时设置】缓存场景已存在实例的信息，或者预制物中非顶层物体的信息，主要用于识别每个子层组件（Prefab或NotAPrefab）（即使与cachePrefabID同时设置也没关系，因为会优先考虑 cachePrefabID）（PS：Prefab对应实例中该字段会提示需要Apply，可以忽略）
        [SerializeField] Identity cachePrefabID = new Identity();//【运行时设置】【序列化时优先使用】缓存运行时生成该实例的SOAssetPack中Prefab的GUID信息（由Unity生成）（该字段用于后续序列化时提供给GameObjectPropertyBag）
        [SerializeField] string cacheScope = "";//运行时设置】【可空】其所在范围的唯一标识（与cachePrefabID一同设置）

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
        void OnValidate()
        {
            if (Application.isPlaying)//运行时不需要生成，因为只需要处理预先摆放的物体
                return;

            if (BuildPipeline.isBuildingPlayer)//打包时不进入
                return;
            if (!transform.gameObject)
                return;

            //——以下只针对场景实例，确保每个物体在对应层级中的UID唯一——
            //#存储编辑器非运行模式下生成的实例
            if (EditorTool.IsInstanceGameObject(gameObject))//当该物体是场景实例：为其设置新的GUID
            {
                //——以下针对实例物体——
                if (!transform.gameObject.scene.IsValid())
                    return;
                if (!transform.gameObject.scene.isLoaded)//有可能打包时会进入该方法，通过检查scene是否Load可避免报错
                    return;

                bool shouldGenerateNewID = !IsIDUniqueInHierarchy(true);//需要重新创建GUID：当前GUID无效，或者与同场景物体的GUID冲突
                if (shouldGenerateNewID)
                {
                    int retryCount = 0;
                    int maxRetryCount = 10;
                    do
                    {
                        cacheInstanceID.Guid = Identity.NewGuid();
                        retryCount++;
                    }
                    while (!IsIDUniqueInHierarchy(true) && retryCount < maxRetryCount);

                    if (retryCount == maxRetryCount)
                        Debug.LogError($"【Instance】Try to generate new GUID for [{gameObject}] failed! Reach max retry count {maxRetryCount}!");
                    //else
                    //{
                        //Debug.LogWarning($"【Temp】Update instance {gameObject.scene.name}_{gameObject.scene.rootCount}_{gameObject.name}'s {nameof(cacheInstanceID)} to {cacheInstanceID.Guid}");
                    //}
                    EditorUtility.SetDirty(this);// mark as dirty, so the change will be save into scene file
                }
            }
            else if (EditorTool.IsPersistentGameObject(gameObject))//该物体是本地Prefab（包括从场景拖拽到Asset生成的Prefab)
            {
                ///ToAdd：
                ///-针对Prefab，如果其子物体也有RS_GO，则应该也需要为其设置唯一InstanceIDID，否则会出现无法还原的Bug。下面的已有方法仅针对Prefab的根物体含有RS_GO的情况（测试物体：AD的TableWithCabinet.DrawerPivot）

                //ToUpdate:可以通过PrefabUtility.GetOutermostPrefabInstanceRoot获取并判断返回物体是否与自身一致，从而确定是否为根物体（参考EditorUtility.FindPrefabRoot的Obsolete注释）
                bool isTopGO = gameObject.transform.parent == null;//是否为顶层物体

                if (isTopGO)//如果为顶层Prefab：【注释】备注：暂不清除，因为其不影响绑定，避免因为频繁修改Prefab导致字段频繁更新而出错。其值会在实例化时自动匹配并进行更新（包括作为其他Prefab的子Prefab）
                {
                    //Debug.LogError("[Temp]Fxcking top! " + gameObject.name);

                    //清空cacheInstanceID，因为放到需要在实例化时设置该值
                    //if (cacheInstanceID.IsValid())//cacheInstanceID有效：清空cacheInstanceID
                    //{
                    //    cacheInstanceID.Guid = "";//清空GUID
                    //    Debug.LogWarning($"【Temp】Clear prefab {gameObject.name}'s cacheInstanceMetadata");
                    //    PrefabUtility.RecordPrefabInstancePropertyModifications(this);//通知预制物进行保存
                    //}
                }
                else//如果为Prefab下的子物体：为其设置本层级唯一的GUID，确保每个Prefab实例中都有有效的ID
                {
                    //Debug.LogError("[Temp]Fxcking child! " + gameObject.name);

                    if (!IsIDUniqueInHierarchy(false))
                    {
                        int retryCount = 0;
                        int maxRetryCount = 10;
                        do
                        {
                            cacheInstanceID.Guid = Identity.NewGuid();
                            retryCount++;
                        }
                        while (!IsIDUniqueInHierarchy(false) && retryCount < maxRetryCount);

                        if (retryCount == maxRetryCount)
                            Debug.LogError($"【Prefab】Try to generate new GUID for [{gameObject}] failed! Reach max retry count {maxRetryCount}!");
                        //else
                        //{
                        //    Debug.LogWarning($"【Temp】Update prefab {gameObject.name}'s {nameof(cacheInstanceID)} to {cacheInstanceID.Guid}");
                        //}
                        PrefabUtility.RecordPrefabInstancePropertyModifications(this);//通知预制物进行保存
                    }
                }
            }
        }

        /// <summary>
        /// 本GUID是否在场景中唯一（非必须，确保层级唯一即可）
        /// </summary>
        /// <returns></returns>
        bool IsInstanceIDValidInScene()
        {
            if (!cacheInstanceID.IsValid)//本GUID无效
                return false;

            //#检查是否在场景中唯一(ToUpdate:改为用静态List缓存)
            try
            {
                foreach (var rtsGO in transform.gameObject.scene.GetComponents<RuntimeSerializable_GameObject>())
                {
                    if (rtsGO == this)//忽略自身
                        continue;
                    if (rtsGO.cacheInstanceID.IsValid && rtsGO.cacheInstanceID == cacheInstanceID)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isInstance">本物体是否为实例</param>
        /// <returns></returns>
        bool IsIDUniqueInHierarchy(bool isInstance)
        {
            if (!cacheInstanceID.IsValid)//本GUID无效
                return false;

            try
            {
                //#检查是否在层级中唯一
                Transform tfParent = transform.parent;
                if (tfParent)//有父物体：检查同级
                {
                    foreach (var rsGO in tfParent.GetComponentsInChildren<RuntimeSerializable_GameObject>())
                    {
                        if (rsGO == this)//忽略自身
                            continue;
                        if (rsGO.cacheInstanceID.IsValid && rsGO.cacheInstanceID == cacheInstanceID)
                            return false;
                    }
                }
                else//无父物体
                {
                    if (isInstance)//如果是实例：检查该场景的所有顶层物体
                    {
                        Scene scene = gameObject.scene;
                        if (scene.IsValid() && scene.isLoaded)
                        {
                            foreach (var goRoot in scene.GetRootGameObjects())
                            {
                                RuntimeSerializable_GameObject rsGO = goRoot.GetComponent<RuntimeSerializable_GameObject>();
                                if (!rsGO)
                                    continue;
                                if (rsGO == this)
                                    continue;
                                if (rsGO.cacheInstanceID.IsValid && rsGO.cacheInstanceID == cacheInstanceID)
                                    return false;
                            }
                        }
                    }
                    else//如果非实例：代表为预制物的顶层，返回true   
                    {
                        return true;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Try create InstanceGUID for gameobject [{transform.gameObject.name}] failed! Error info:\r\n{e}");
            }
            return true;
        }
#endif

        /// <summary>
        /// 【Runtime】临时存储Prefab的信息，便于序列化时保存
        /// 
        /// 调用时刻：
        /// -该物体基于Prefab生成
        /// -反序列化时还原该数值
        /// </summary>
        /// <param name="m_Guid"></param>
        public void InitPrefabMetadata(string m_Guid, string scope)
        {
            cachePrefabID = new Identity(m_Guid);
            cacheScope = scope;
        }
        #endregion

        //#Cache
        bool isSourcePrefabMissing = false;
        GameObjectPropertyBag deserializedGameObjectPropertyBag;//当前反序列化的源信息，方便在Prefab丢失时保存其源信息

        protected override GameObject GetContainerFunc() { return gameObject; }

        public override GameObjectPropertyBag GetPropertyBag()
        {
            if (isSourcePrefabMissing && deserializedGameObjectPropertyBag != null)//Prefab丢失：直接返回上次反序列化的源信息，便于原封不动地保存
            {
                return deserializedGameObjectPropertyBag;
            }

            //#0 保存相关信息到PropertyBag
            GameObjectPropertyBag propertyBag = base.GetPropertyBag();
            if (cacheInstanceID.IsValid)
                propertyBag.instanceID = new Identity(cacheInstanceID);
            if (cachePrefabID.IsValid)
                propertyBag.prefabID = new Identity(cachePrefabID);
            propertyBag.scope = cacheScope;//不管有无值都可以保存

            //#1 扫描并存储该物体所有RTSComponent的序列化数据
            //propertyBag.serializedComponents.AddRange(listRSComponent.ConvertAll(rsc => rsc.OnSerialize()));//【ToDelete】
            List<IRuntimeSerializableComponent> listRSComponent = gameObject.GetComponents<IRuntimeSerializableComponent>().ToList();
            List<IComponentPropertyBag> listCPB = new List<IComponentPropertyBag>();
            foreach (var rsComponent in listRSComponent)
            {
                IComponentPropertyBag cPB = rsComponent.ComponentPropertyBag;
                cPB.ID = rsComponent.ID;//存储ID到PropertyBag
                listCPB.Add(cPB);
            }
            propertyBag.compoentPropertyBags.AddRange(listCPB);

            //#2 扫描子物体
            foreach (Transform tfChild in container.transform)
            {
                RuntimeSerializable_GameObject rtsgChild = tfChild.GetComponent<RuntimeSerializable_GameObject>();
                if (rtsgChild)
                {
                    GameObjectPropertyBag propertyBagChild = rtsgChild.GetPropertyBag();
                    propertyBag.children.Add(propertyBagChild);
                }
            }

            return propertyBag;
        }
    }

    #region Define
    /// <summary>
    /// 原理:
    /// -如果是Prefab，则存储其prefabMetadata，还原时通过 Mod的AssetPack 可以找到直接对应Prefab并还原
    /// -Mod的AssetPack可在打包时扫描Mod文件夹并生成，并且以SO的形式打包（不能直接序列化，因为会丢失对Prefab物体的引用）
    /// </summary>
    [System.Serializable]
    [JsonObject(MemberSerialization.Fields)]
    public class GameObjectPropertyBag : UnityObjectPropertyBag<GameObject>
    {
        //缓存唯一ID（统一由RuntimeSerialization_GameObject管理）
        public Identity instanceID = new Identity();//（如果是实例，则不为空）
        public Identity prefabID = new Identity();//（如果是Prefab，则不为空）
        public string scope = "";//【可空】其所在范围的唯一标识（如果是Prefab，则可选设置）

        public string name = "GameObject";//PS：名称不能为空，否则会出现警告
        public int layer;
        public string tag = "Untagged";//默认值，避免出错
        public bool active;
        public int siblingIndex;//物体在同级中的顺序，主要用于重新生成时定位（不放在TransformPropertyBag是因为其组件非必须）

        //——以下通过RuntimeSerialization_GameObjectRuntimeSerialization_GameObject进行序列化——
        ///// <summary>
        ///// Warning:
        ///// -默认反序列化时不支持自动转为其子类，因此本实现是基于ID进行匹配
        ///// </summary>
        public List<IComponentPropertyBag> compoentPropertyBags = new List<IComponentPropertyBag>();//该物体的所有组件对应的PropertyBag

        //public List<string> serializedComponents = new List<string>();//保存ComponentPropertyBag序列化后的内容【ToDelete】

        [SerializeReference] public List<GameObjectPropertyBag> children = new List<GameObjectPropertyBag>();//Warning：因为Unity默认会序列化为值类型，所以会报警告：Serialization depth limit 10 exceeded。解决办法是将该字段标记为[SerializeReference]

        public GameObjectPropertyBag() { }
        public override void Init(GameObject container)
        {
            base.Init(container);
            name = container.name;
            layer = container.layer;
            tag = container.tag;
            active = container.activeSelf;
            siblingIndex = container.transform.GetSiblingIndex();
        }
        public override void Accept(ref GameObject container)
        {
            base.Accept(ref container);
            container.name = name;
            container.layer = layer;
            container.tag = tag;
            container.SetActive(active);

            //#由开发者自行处理子物体的siblingIndex，因为可能当前生成一序列Prefab，需要延后设置排序
            //if (container.transform.parent)
            //    container.transform.SetSiblingIndex(siblingIndex);
        }
    }

    [System.Serializable]
    public class DataOption_GameObjectPropertyBag : DataOption
    {
        public DataOption_GameObjectPropertyBag()
        {
        }
    }
    #endregion
}