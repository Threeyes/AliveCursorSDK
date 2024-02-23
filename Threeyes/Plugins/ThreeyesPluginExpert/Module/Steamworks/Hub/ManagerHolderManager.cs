using System;
using System.Collections;
using System.Collections.Generic;
using Threeyes.Core;
using UnityEngine;
using UnityEngine.Events;

namespace Threeyes.Steamworks
{
    /// <summary>
    /// 管理与各ManagerHolder特殊字段的初始化配置
    /// 
    /// PS：
    /// -存在Hub及Simulator中
    /// -为了避免访问顺序的问题，可用于存储静态属性、事件
    /// </summary>
    public abstract class ManagerHolderManager : HubManagerBase<ManagerHolderManager>
    {
        public event UnityAction<Type, bool> GlobalControllerConfigStateChanged;//全局Controller配置文件的可用状态

        /// <summary>
        /// 启用/禁用 全局Controller的配置文件
        /// </summary>
        /// <typeparam name="TSOInterface">接口</typeparam>
        /// <param name="isActive"></param>
        public void FireGlobalControllerConfigStateEvent<TSOInterface>(bool isActive)
        {
            GlobalControllerConfigStateChanged.Execute(typeof(TSOInterface), isActive);
        }

        public override void SetInstance()
        {
            base.SetInstance();
            InitSpecialFields();
        }

        protected virtual void InitSpecialFields()
        {
            InitWorkshopItemInfoFactory();

            ManagerHolder.GetListManagerModPreInitOrder = GetListManagerModPreInitOrder;
            ManagerHolder.GetListManagerModInitOrder = GetListManagerModInitOrder;//因为使用了ManagerHolder的字段，所以需要等使用后才能获取，不能在这个阶段设置) 
        }
        /// <summary>
        /// 初始化ManagerHolder.WorkshopItemInfoFactory
        /// </summary>
        protected abstract void InitWorkshopItemInfoFactory();

        protected abstract List<IHubManagerModPreInitHandler> GetListManagerModPreInitOrder();

        /// <summary>
        /// 初始化ManagerHolder.GetListManagerModInitOrder
        /// </summary>
        /// <returns></returns>
        protected abstract List<IHubManagerModInitHandler> GetListManagerModInitOrder();
    }
}