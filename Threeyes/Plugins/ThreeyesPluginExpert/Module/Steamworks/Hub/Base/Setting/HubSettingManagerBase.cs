using System;
using UnityEngine;
using Threeyes.Data;
using NaughtyAttributes;
using System.Reflection;
using System.Collections.Generic;
using Threeyes.Config;
using Threeyes.Core;

namespace Threeyes.Steamworks
{
    public abstract class HubSettingManagerBase<T, TSOConfig, TConfig> : HubManagerBase<T>, IHubSettingManager
        where T : HubSettingManagerBase<T, TSOConfig, TConfig>
        where TSOConfig : SOConfigBase<TConfig>
        where TConfig : HubSettingConfigInfoBase
    {
        /// <summary>
        /// 当前App的版本
        /// </summary>
        public string strAppVersion { get { return Application.version; } }//App版本，格式必须是A.B.C
        public static TConfig Config
        {
            get
            {
                return Instance.soOverrideConfig != null ? Instance.soOverrideConfig.config : Instance.defaultConfig;
            }
        }
        //TConfig config;//PS:不能是state，便于程序重置时自动清空(Bug:不能使用，因为程序调用问题可能会导致提前销毁而无法保存）

        [SerializeField] protected TConfig defaultConfig;//Default config
        [Expandable] [SerializeField] protected TSOConfig soOverrideConfig;//Override config

        public bool HasInit { get { return hasInit; } set { hasInit = value; } }
        protected bool hasInit;
        public virtual void Init(bool isFirstInit)
        {
            UpdateConfig(isFirstInit);//更新已读取Config的特殊字段（如Version）

            InitEvent();//监听Config事件
            InitUI();//基于读取的Config初始化对应UI

            NotifyAllDataEvent(BasicDataState.Init);//【不能删掉】#外部已经监听完毕，且值已经设置完毕，通知所有监听BaseData的方法，从而进行初始化（如generalSetting_IsSupportMultiDisplay）
            hasInit = true;
        }
        public virtual void DeInit()
        {
            //PS：【Editor】模式下，BasicData中的Action会在Replay后自动清空。如果需要实现Reload HubScene的功能，需要调用Config.ClearAllDataEvent
            ClearAllDataEvent();
        }

        public virtual void InitEvent()
        {
        }
        public virtual void ResetConfigToDefault()
        {
            ///Bug:
            ///-偶尔会使用各Field的默认值，可能原因是使用了Json中的Default值，而其保存的是默认值
            ///ToUpdate：改为使用defaultConfig的对应值

            //重置配置为默认值
            ResetAllDataToDefault();
        }

        /// <summary>
        /// 更新Config中的特殊字段（如Version）,也可以在这里针对旧版本的配置属性进行修改
        /// </summary>
        /// <param name="isFirstInit"></param>
        protected virtual void UpdateConfig(bool isFirstInit)
        {
            try
            {
                //更新本地配置的版本值
                Version curVersion = new Version(strAppVersion);
                if (curVersion != Config.version)
                    Config.version = curVersion;
            }
            catch (Exception e)
            {
                Debug.LogError("UpdateConfig failed: " + e);
            }
        }
        protected abstract void InitUI();

        #region Data
        public virtual void ResetAllDataToDefault()
        {
            try
            {
                ///使用默认值进行重置，同时会调用actionValueReset从而静默更新UI
                ExecuteBetweenConfig(ref defaultConfig, ref soOverrideConfig.config,
                    (bdSource, bdOther) =>
                    {
                        //#1 复制Value和DefaultValue到目标Config
                        (bdSource as BasicData).CloneTo(ref bdOther);
                        //#2 调用目标Config的ResetToDefaultValue，使用DefaultValue来重置值和通知事件来更新UI（Warning：DefaultValue来源于defaultConfig！）
                        (bdOther as BasicData).ResetToDefaultValue();
                    },
                    GetListIgnoreBaseDataFieldName_Reset());
            }
            catch (Exception e)
            {
                Debug.LogError("ResetAllData with error:\r\n" + e);
            }
        }
        public void ClearAllDataEvent()
        {
            try
            {
                GetListBaseData().ForEach((bd) => bd.ClearEvent());
            }
            catch (Exception e)
            {
                Debug.LogError("ClearAllDataEvent with error:\r\n" + e);
            }
        }
        public void NotifyAllDataEvent(BasicDataState state = BasicDataState.Update)
        {
            {
                GetListBaseData().ForEach((bd) =>
                {
                    try
                    {
                        bd.NotifyValueChanged(state);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"NotifyAllDataEvent {bd} on state {state} with error:\r\n" + e);
                    }
                });
            }
        }

        ///// <summary>
        ///// 获取需要重置的字段
        ///// </summary>
        ///// <returns></returns>
        //protected virtual List<BasicData> GetListBaseData_Reset()
        //{
        //    return GetListBaseData();
        //}
        /// <summary>
        /// 获取需要忽略的BaseData字段
        /// </summary>
        /// <returns></returns>
        protected virtual List<string> GetListIgnoreBaseDataFieldName_Reset()
        {
            return new List<string>();
        }
        /// <summary>
        /// 获取所有的BasicData实例
        /// ToUpdate:改为放到管理类中，去掉泛型
        /// </summary>
        /// <returns></returns>
        protected List<BasicData> GetListBaseData(List<string> listIgnoreFieldName = null)
        {
            List<BasicData> listBD = new List<BasicData>();
            //复制对应的值
            foreach (FieldInfo fieldInfo in typeof(TConfig).GetFields())
            {
                var fieldValue = fieldInfo.GetValue(Config);//真实值
                if (fieldValue.GetType().IsSubclassOf(typeof(BasicData)))
                {
                    if (listIgnoreFieldName != null && listIgnoreFieldName.Contains(fieldInfo.Name))//忽略指定字段
                        continue;

                    BasicData inst = fieldValue as BasicData;
                    listBD.Add(inst);
                }
            }
            return listBD;
        }

        /// <summary>
        /// 
        /// Old:将字段复制给其他实例，会调用其BasicData事件更新
        /// </summary>
        /// <param name="other"></param>
        public virtual void ExecuteBetweenConfig(ref TConfig source, ref TConfig other, Action<object, object> actBetweenTwoBasicData, List<string> listIgnoreFieldName = null)
        {
            try
            {
                foreach (FieldInfo fieldInfo in typeof(TConfig).GetFields())
                {
                    if (listIgnoreFieldName != null && listIgnoreFieldName.Contains(fieldInfo.Name))//忽略指定字段
                        continue;


                    if (fieldInfo.FieldType.IsSubclassOf(typeof(BasicData)))
                    {
                        actBetweenTwoBasicData.Execute(fieldInfo.GetValue(source), fieldInfo.GetValue(other));
                        //bdThis.CloneTo(ref fieldValueOthers);//复制对应的值
                    }
                    //else//ToAdd其他(如enum:直接set)(需要确认是否需要通知。目前CommonSetting全部使用BasicData因此不需要考虑enumerate)
                    //{

                    //}

                }
            }
            catch (Exception e)
            {
                Debug.LogError("Clone error:\r\n" + e);
            }
        }
        #endregion
    }

    #region Define
    public abstract class HubSettingConfigInfoBase : SerializableDataBase
    {
        public Version version = new Version("3.0.0");//Warning：格式必须是A.B.c，否则报错！（The major and minor components are required; the build and revision components are optional）
    }
    #endregion

}
