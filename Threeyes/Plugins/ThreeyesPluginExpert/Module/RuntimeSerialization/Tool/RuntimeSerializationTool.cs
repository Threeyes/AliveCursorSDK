using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Threeyes.Data;
using Threeyes.Core.Editor;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Threeyes.RuntimeSerialization
{
    public static class RuntimeSerializationTool
    {
        #region Serialization
        public static readonly Formatting DefaultComponentFormatting = Formatting.None;//Component：不缩进，因为组件默认以string格式存储，因此不需要添加回车等多余的字符
        public static readonly DefaultValueHandling defaultValueHandling = DefaultValueHandling.Populate;//Members with a default value but no JSON will be set to their default value when deserializing.（反序列化时，当字段不存在时会使用默认值，可用[DefaultValue]指定特定默认值，避免新增字段不在旧Json文件中存在导致使用了类型默认值从而出错）
        public static readonly JsonSerializerSettings DefaultJsonSerializerSettings = new JsonSerializerSettings
        {
            //TypeNameAssemblyFormatHandling = Newtonsoft.Json.TypeNameAssemblyFormatHandling.Simple,
            //TypeNameHandling = TypeNameHandling.Objects,//增加$type字段，用于标记PropertyBag的类型
            TypeNameHandling = TypeNameHandling.All,//增加$type字段，用于标记PropertyBag的类型
            DefaultValueHandling = DefaultValueHandling.Populate//Members with a default value but no JSON will be set to their default value when deserializing.（反序列化时，当字段不存在时会使用默认值，可用[DefaultValue]指定特定默认值，避免新增字段不在旧Json文件中存在导致使用了类型默认值从而出错）
        };

        /// <summary>
        /// 使用推荐的参数来序列化物体
        /// </summary>
        /// <param name="value"></param>
        /// <param name="formatting"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static string SerializeObject(object value, Formatting? formatting = null, JsonSerializerSettings settings = null)
        {
            Formatting targetFormatting = formatting != null ? formatting.Value : DefaultComponentFormatting;
            JsonSerializerSettings targetSetting = settings != null ? settings : DefaultJsonSerializerSettings;
            return JsonConvert.SerializeObject(value, targetFormatting, targetSetting);
        }

        public static T DeserializeObject<T>(string value, JsonSerializerSettings settings = null)
        {
            JsonSerializerSettings targetSetting = settings != null ? settings : DefaultJsonSerializerSettings;
            return JsonConvert.DeserializeObject<T>(value, targetSetting);
        }
        #endregion

        #region ID
#if UNITY_EDITOR
        /// <summary>
        /// 针对组件生成ID，不管所挂载物体是否实例还是预制物
        /// </summary>
        /// <typeparam name="TComponent"></typeparam>
        /// <param name="rsComponent"></param>
        /// <param name="id"></param>
        public static void EditorUpdateComponetID<TComponent>(TComponent rsComponent, ref Identity id)
            where TComponent : Component, IRuntimeSerializableObject//Component, IRuntimeSerializableComponent
        {
            if (Application.isPlaying)//运行时不需要生成，因为只需要处理预先摆放的物体
                return;

            if (BuildPipeline.isBuildingPlayer)//打包时不进入
                return;

            string failedReason = "";
            if (!IsIDUniqueInGameObject(rsComponent, id, ref failedReason))
            {
                GameObject go = rsComponent.gameObject;
                //Debug.LogWarning($"【Temp】#PrefabMode:{EditorTool.IsPrefabMode(go)} PartOfAnyPrefab:{EditorTool.IsPartOfAnyPrefab(go)} PersistentGameObject:{EditorTool.IsPersistentGameObject(go)}");     
                //if (failedReason.NotNullOrEmpty())
                //    Debug.LogWarning($"{"FailedReason: " + failedReason}");

                //#跳过PrefabMode。因为在PrefabMode中，根Prefab会被全部实例化，此时它就不算是资源的一部分，这时候下方的EditorTool.IsPartOfAnyPrefab会返回false！
                if (EditorTool.IsPrefabMode(go))
                {
                    //Debug.LogWarning($"【Temp】Skip PrefabMode: [{go.name}_{rsComponent.ContainerType}]");
                    return;
                }

                if (EditorTool.IsPartOfAnyPrefab(rsComponent))//如果该组件是Prefab的一部分
                {
                    //Debug.LogWarning($"【Temp】Component is part of prefab: [{go.name}_{rsComponent.ContainerType}]");

                    //当前物体并非源头Prefab（也就是实例）：跳过，等后续源头Prefab调用此方法更新ID，这样可以避免冲突
                    if (!EditorTool.IsPersistentGameObject(go))
                    {
                        Debug.LogWarning($"【Temp】Wait for prefab asset to update GUID: [{go.name}_{rsComponent.ContainerType}]", go);
                        return;
                    }
                }

                //尝试更新ID
                int retryCount = 0;
                int maxRetryCount = 10;
                do
                {
                    id.Guid = Identity.NewGuid();
                    retryCount++;
                }
                while (!IsIDUniqueInGameObject(rsComponent, id, ref failedReason) && retryCount < maxRetryCount);

                if (retryCount == maxRetryCount)
                    Debug.LogError($"Try to generate new GUID for [{go.name} ({rsComponent.ContainerType})] failed! Reach max retry count {maxRetryCount}!", go);
                else
                    Debug.Log($"【Temp】Update [{go.name} ({rsComponent.ContainerType})]'s {nameof(id)} to {id.Guid}", go);

                if (EditorTool.IsInstanceGameObject(go))
                {
                    EditorUtility.SetDirty(rsComponent);// mark as dirty, so the change will be save into scene file
                }
                else if (EditorTool.IsPersistentGameObject(go))
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(rsComponent);//通知预制物进行保存
                }
            }
        }

        /// <summary>
        /// 指定组件的ID是否在此物体唯一
        /// 适用于：Component匹配
        /// </summary>
        /// <typeparam name="TComponent">实现了指定类型的组件类型</typeparam>
        /// <param name="rsComponent">指定的组件</param>
        /// <param name="id">指定的ID</param>
        /// <param name="failReason"></param>
        /// <returns></returns>
        public static bool IsIDUniqueInGameObject<TComponent>(TComponent rsComponent, Identity id, ref string failReason)
               where TComponent : Component, IIdentityHolder//Component, IRuntimeSerializableComponent
        {
            if (!id.IsValid)//本GUID无效
            {
                failReason = "ID not valid";
                return false;
            }
            TComponent[] arrExistRSC = rsComponent.GetComponents<TComponent>();
            foreach (TComponent rtsC in arrExistRSC)
            {
                if (rtsC == rsComponent)//只计算到自身序号的组件位置，忽略后续。因为新添加的组件默认都在最后，所以只判断此组件是否与之前任意组件冲突，如果是就更新
                    return true;
                if (rtsC.ID.IsValid && rtsC.ID == id)
                {
                    failReason = "ID exists in Hierarchy";
                    return false;
                }
            }
            return true;
        }
#endif
        #endregion
    }
}