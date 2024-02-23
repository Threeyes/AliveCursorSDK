using System.Collections.Generic;
using UnityEngine;
using Threeyes.Data;
using System.Linq;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using Threeyes.Core.Editor;
using Threeyes.Core;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Threeyes.RuntimeSerialization
{
    /// <summary>
    /// 物体想序列化必须要有的组件，存储GameObject及组件的相关信息
    /// Ref:Unity.RuntimeSceneSerialization.Json.Adapters.JsonAdapter.GameObject
    /// 
    /// PS：
    /// -放在场景中的不算Prefab，所以不需要初始化其PrefabMetadata字段。PrefabMetadata的作用是重新生成Prefab
    /// -因为该类不需要子类继承，所以暂时不需要继承类似IRuntimeSerializableComponent的接口
    /// 
    /// ToUpdate:
    /// -在GameObject没有唯一标识的情况下，建议序列的物体组要不全实例，要不全通过Prefab生成，否则容易在反序列化时出现混淆
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RuntimeSerializable_GameObject : RuntimeSerializableUnityObject<GameObject, GameObjectPropertyBag>, IIdentityHolder
    {
        #region ID  
        public override Identity ID { get { return cacheInstanceID; } set { } }//暂不允许设置（继承IIdentityHolder接口仅用于使用RuntimeSerializationTool相关方法，平时不应使用！）

        ///——缓存该物体的唯一ID，便于绑定。可以由开发者设置，也可以通过获取GUID进行自动指定（ReadOnly）——

        //#以下字段二选一来使用：
        [SerializeField] public Identity cachePrefabID = new Identity();//【运行时设置】【序列化时优先使用】缓存运行时生成该实例的SOAssetPack中Prefab的信息（该字段用于后续序列化时提供给GameObjectPropertyBag）
        [SerializeField] public Identity cacheInstanceID = new Identity();//【编辑器非运行时设置】缓存已存在实例的信息，或者预制物中非顶层物体的信息（Prefab或NotAPrefab）（即使与cachePrefabMetadata同时设置也没关系，因为会优先考虑cachePrefabMetadata）（PS：Prefab对应实例中该字段会提示需要Apply，可以忽略）


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
                    else
                        Debug.LogWarning($"【Temp】Update instance {gameObject.scene.name}_{gameObject.scene.rootCount}_{gameObject.name}'s {nameof(cacheInstanceID)} to {cacheInstanceID.Guid}");

                    EditorUtility.SetDirty(this);// mark as dirty, so the change will be save into scene file
                }
            }
            else if (EditorTool.IsPersistentGameObject(gameObject))//该物体是本地Prefab（包括从场景拖拽到Asset生成的Prefab)
            {
                ///ToAdd：
                ///-针对Prefab，如果其子物体也有RS_GO，则应该也需要为其设置唯一InstanceIDID，否则会出现无法还原的Bug。下面的已有方法仅针对Prefab的根物体含有RS_GO的情况（测试物体：AD的TableWithCabinet.DrawerPivot）

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
                        else
                            Debug.LogWarning($"【Temp】Update prefab {gameObject.name}'s {nameof(cacheInstanceID)} to {cacheInstanceID.Guid}");

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
            if (!cacheInstanceID.IsValid())//本GUID无效
                return false;

            //#检查是否在场景中唯一(ToUpdate:改为用静态List缓存)
            try
            {
                foreach (var rtsGO in transform.gameObject.scene.GetComponents<RuntimeSerializable_GameObject>())
                {
                    if (rtsGO == this)//忽略自身
                        continue;
                    if (rtsGO.cacheInstanceID.IsValid() && rtsGO.cacheInstanceID == cacheInstanceID)
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
            if (!cacheInstanceID.IsValid())//本GUID无效
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
                        if (rsGO.cacheInstanceID.IsValid() && rsGO.cacheInstanceID == cacheInstanceID)
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
                                if (rsGO.cacheInstanceID.IsValid() && rsGO.cacheInstanceID == cacheInstanceID)
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
        public void InitPrefabMetadata(string m_Guid)
        {
            cachePrefabID = new Identity(m_Guid);
        }
        #endregion

        //Cache
        bool isSourcePrefabMissing = false;
        GameObjectPropertyBag deserializedGameObjectPropertyBag;//当前反序列化的源信息，方便在Prefab丢失时保存其源信息

        //protected override Formatting Formatting { get { return Formatting.Indented; } }///GameObject：为了方便阅读，需要缩进【如果是使用PersistentData_String存储就不需要设置缩进，否则更难阅读】

        protected override GameObject GetContainerFunc() { return gameObject; }

        public override GameObjectPropertyBag GetPropertyBag()
        {
            if (isSourcePrefabMissing && deserializedGameObjectPropertyBag != null)//Prefab丢失：返回上次反序列化的源信息
            {
                return deserializedGameObjectPropertyBag;
            }


            //保存相关信息
            GameObjectPropertyBag propertyBag = base.GetPropertyBag();
            if (cachePrefabID.IsValid())
                propertyBag.prefabID = new Identity(cachePrefabID);
            if (cacheInstanceID.IsValid())
                propertyBag.instanceID = new Identity(cacheInstanceID);

            //#1 扫描并存储该物体所有RTSComponent的序列化数据
            //propertyBag.serializedComponents.AddRange(listRSComponent.ConvertAll(rsc => rsc.OnSerialize()));//【ToDelete】
            List<IRuntimeSerializableComponent> listRSComponent = gameObject.GetComponents<IRuntimeSerializableComponent>().ToList();
            List<IComponentPropertyBag> listCPB = new List<IComponentPropertyBag>();
            foreach (var rsComponent in listRSComponent)
            {
                IComponentPropertyBag cPB = rsComponent.ComponentPropertyBag;
                cPB.ID = rsComponent.ID;//初始化ID
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

        //——【ToUpdate】：通过分布类的方式，分离出具体实现——

        protected override void DeserializeFunc(GameObjectPropertyBag propertyBag, IDeserializationOption baseOption = null)
        {
            base.DeserializeFunc(propertyBag, baseOption);

            //DeserializationOption
            DeserializationOption_GameObject deserializationOption = new DeserializationOption_GameObject();
            if (baseOption is DeserializationOption_GameObject dOGM)//自定义的Option（ToTest：离开代码块后dOGM会不会变空）
            {
                deserializationOption = dOGM;
            }

            //#1 反序列化自身所有组件
            ////——实现1：基于序列化成字符串的string【ToDelete】——       
            ////-查找与序列化后的类型相同的组件，并传入对应序列化字段进行OnDeserialize（不直接使用反射的好处是，方便用户）
            ////【ToUpdate】：
            ////-针对每一个组件都根据ID进行唯一匹配
            ////-针对Modder的脚本，只需要匹配类名即可，因为Mod打包后所在程序集名称不一样
            ////-针对多个相同组件，需要按顺序匹配（如标记某个组件已经反序列化）
            ////-【进阶】对组件进行排序（参考Unity.RuntimeSceneSerialization.Internal.SerializationUtils.SortComponentList）
            //List<PropertyBag> listPropertyBagCom = propertyBag.serializedComponents.ConvertAll((s) => DeSerializeToCommonPropertyBag(s));//-将serializedComponents转为PropertyBag
            //List<IRuntimeSerializableComponent> listRSComponent = gameObject.GetComponents<IRuntimeSerializableComponent>().ToList();//查找所有可序列化的组件（PS：不能使用IRuntimeSerializable，否则会获取到自身的RTS_Go）
            //foreach (var rsComponent in listRSComponent)
            //{
            //    int matchedIndex = listPropertyBagCom.FindIndex(pB => pB.containerTypeName == PropertyBag.GetTypeName(rsComponent.ContainerType));
            //    if (matchedIndex != -1)
            //    {
            //        rsComponent.OnDeserialize(propertyBag.serializedComponents[matchedIndex]);
            //        //rsComponent.OnDeserializeBase(propertyBag.serializedComponents[matchedIndex]);
            //    }
            //}

            ////——实现2：基于原类型序列化——

            //#1 查找所有符合条件的组件进行反序列化
            List<IComponentPropertyBag> listComponentPropertyBag = propertyBag.compoentPropertyBags;
            List<IRuntimeSerializableComponent> listRSComponent = gameObject.GetComponents<IRuntimeSerializableComponent>().ToList();//PS：不能使用IRuntimeSerializable，否则会获取到自身的RTS_Go
            foreach (var componentPropertyBag in listComponentPropertyBag)
            {
                ///反序列化流程：
                ///     -优先搜索相同ID的组件并反序列化
                ///     -如果找不到，则警告。
                /// -【Todo】后续可尝试通过containerTypeName搜索首个且唯一相同类型的组件（注意：仅适用于必要组件（如ShellItem），否则有可能因为Modder主动删除或替换组件导致功能不一）
                IRuntimeSerializableComponent matchedRSComponent = listRSComponent.FirstOrDefault(rsC => rsC.ID == componentPropertyBag.ID);
                if (matchedRSComponent != null)
                {
                    matchedRSComponent.DeserializeBase(componentPropertyBag);
                }
                else
                {
                    Debug.LogWarning($"Can't find Component [{(componentPropertyBag.ContainerTypeName)}] with [guid {componentPropertyBag.ID}]! Will not be Deserialize!");
                }
            }

            //#2 链接或重新生成所有子物体
            List<RuntimeSerializable_GameObject> listInstanceRSGO = transform.FindComponentsInChild<RuntimeSerializable_GameObject>(includeSelf: false, isRecursive: false).ToList();//缓存所有挂载RS_GO的子物体实例(需要忽略自身；仅扫描一层)
            foreach (var childGOPropertyBag in propertyBag.children)
            {
                RuntimeSerializable_GameObject rtsgInst = null;
                bool isChildRuntimeDeserializedPrefab = deserializationOption.isParentRuntimeDeserializedPrefab;//标记该物体或父物体是否为运行时通过Prefab生成

                if (childGOPropertyBag.prefabID.IsValid())//基于Prefab：生成实例并反序列化（Warning：如果prefabMetadata为空，则无法正常生成对应预制物并还原配置。可能原因是二次序列化时没有保存好数据，意外使用了RuntimeSerialization_GameObject的cachePrefabMetadata）
                {
                    isChildRuntimeDeserializedPrefab = true;//标记为运行时生成

                    //#1 尝试查找对应预制物并生成实例
                    string guid = childGOPropertyBag.prefabID.Guid;
                    GameObject goInst = SOAssetPackManager.TryInstantiatePrefab(guid, transform);//以该物体为父物体
                    if (goInst)
                    {
                        rtsgInst = goInst.GetComponent<RuntimeSerializable_GameObject>();
                        if (rtsgInst)
                        {
                            rtsgInst.InitPrefabMetadata(childGOPropertyBag.prefabID.Guid);//不管prefabID的值是否有效都统一初始化，便于后续序列化时保存
                        }
                    }
                    else//#2 如果无法找到预制物，则会生成一个空占位Prefab，方便调试与后续序列化以避免丢失
                    {
                        ///ToUpdate:
                        ///-如果找不到，则创建一个带RS_GO的空物体（也可以使用方块+紫色材质代表丢失），并且把源序列化字符串存储到RS_GO中（可以用一个字段代表Missing，另一个字段存储源序列化字符串）
                        string dummyName = $"{childGOPropertyBag.name} (Missing Prefab with guid: {guid})";//参考Unity针对丢失Prefab的名字处理
                        Debug.LogWarning($"Failed to instantiate prefab with guid ({guid})! Use empty dummy ({dummyName}) instead!");
                        goInst = new GameObject();
                        goInst.transform.SetParent(transform);
                        goInst.name = dummyName;

                        RuntimeSerializable_GameObject dummyRS_GO = goInst.AddComponentOnce<RuntimeSerializable_GameObject>();//PS:因为不是源物体的RS_GO，因此不能赋值给rtsgInst，因为其后续还需要进行穷举反序列化
                        dummyRS_GO.isSourcePrefabMissing = true;
                        dummyRS_GO.deserializedGameObjectPropertyBag = childGOPropertyBag;//备份，以便在序列化时原样保存
                    }
                }
                else if (childGOPropertyBag.instanceID.IsValid())//基于现存的物体实例：查找匹配InstanceMetadata的物体直接并反序列化
                {
                    rtsgInst = listInstanceRSGO.FirstOrDefault(rtsg => rtsg.cacheInstanceID == childGOPropertyBag.instanceID);
                    if (rtsgInst)
                    {
                        listInstanceRSGO.Remove(rtsgInst);//如果已经匹配，则移除
                    }
                    else
                    {
                        ///常见报错原因：
                        ///-意外修改了Content物体的id导致无法找到
                        Debug.LogError($"Can't find {gameObject.name}'s child instance [{childGOPropertyBag.name}] for GUID: [{childGOPropertyBag.instanceID}]!");
                    }
                }
                else
                {
                    Debug.LogError($"[{childGOPropertyBag.name}] doesn't have any valid metadata! Will not be deserialize!");//可能原因：id已经改变
                }

                //#3 对该物体进行反序列化
                if (rtsgInst)
                {
                    //PS：因为deserializationOption是class，在后续还需要使用，所以只能缓存其isParentRuntimeDeserializedPrefab值，并在使用后还原
                    bool cacheSelf_IsParentRuntimeDeserializedPrefab = deserializationOption.isParentRuntimeDeserializedPrefab;
                    deserializationOption.isParentRuntimeDeserializedPrefab = isChildRuntimeDeserializedPrefab;
                    rtsgInst.DeserializeFunc(childGOPropertyBag, deserializationOption);

                    deserializationOption.isParentRuntimeDeserializedPrefab = cacheSelf_IsParentRuntimeDeserializedPrefab;
                }
            }

            //#3 根据Option进行后处理
            if (deserializationOption.DeleteNotExistInstance && listInstanceRSGO.Count > 0)//是否需要删除不存在的实例
            {
                if (!(deserializationOption.isParentRuntimeDeserializedPrefab && !deserializationOption.DeletePrefabNotExistInstance))//只要不是通过预制物生成的实例，且deletePrefabNotExistInstance为false：删除
                    listInstanceRSGO.ForEach(rsgo => rsgo.gameObject.DestroyAtOnce());
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

        ///// <summary>
        ///// [ToDelete]将Component序列化字段转为通用的PropertyBag，以便获取其类型等信息
        ///// </summary>
        ///// <param name="serializedComponent"></param>
        ///// <returns></returns>
        //static PropertyBag DeSerializeToCommonPropertyBag(string serializedComponent)
        //{
        //    return JsonConvert.DeserializeObject<PropertyBag>(serializedComponent);
        //}
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
        public Identity instanceID = new Identity();//如果为实例，则不为空
        public Identity prefabID = new Identity();//如果为Prefab，则不为空

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
            ////Todo:如果container是Prefab，则搜索其信息
            ///Unity.RuntimeSceneSerialization的实现是加上PrefabMetadata
            //if (container)
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