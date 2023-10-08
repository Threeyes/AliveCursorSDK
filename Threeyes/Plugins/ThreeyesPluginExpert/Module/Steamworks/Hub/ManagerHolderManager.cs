using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Threeyes.Steamworks
{
    /// <summary>
    /// 管理与各ManagerHolder特殊字段的初始化配置，可以直接放在Hub或Simulator中
    /// </summary>
    public abstract class ManagerHolderManager : HubManagerBase<ManagerHolderManager>
    {
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