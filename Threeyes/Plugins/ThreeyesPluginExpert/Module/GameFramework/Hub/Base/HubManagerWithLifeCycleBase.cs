using System.Collections;
using System.Collections.Generic;
using Threeyes.Core;
using UnityEngine;

namespace Threeyes.GameFramework
{
    /// <summary>
    /// 管理各模块的Manager,当需要在退出前执行事件才需要继承
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HubManagerWithLifeCycleBase<T> : HubManagerBase<T>, IProgramLifeCycle
    where T : InstanceBase<T>
    {
        #region IProgramLifeCycle

        public string CycleName { get { return name; } }
        public bool CanQuit { get { return canQuit; } set { canQuit = value; } }
        protected bool canQuit = false;

        public virtual int QuitExecuteOrder { get { return 0; } }

        protected bool isQuitEnter = false;//是否准备退出，可用于停止某些操作（如更改屏幕）
        /// <summary>
        /// 退出前需要立即执行的方法（常用于需要立即执行的重要方法（如涉及系统光标、注册表的操作），避免电脑关机后跳过）
        /// </summary>
        public virtual void OnQuitEnter()
        {
            isQuitEnter = true;
        }

        /// <summary>
        /// 退出前需要多帧执行的方法
        /// 
        /// Warning:
        /// -在编辑器中不会被调用
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerator IETryQuit()
        {
            yield return null;
            //注意：完成后要把canQuit设置为true，为了避免继承者忘了设置，默认返回true
            canQuit = true;
        }
        #endregion
    }
}