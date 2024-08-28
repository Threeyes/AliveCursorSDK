using System.Collections;
using System.Reflection;
using System.Linq;
using UnityEngine;
using System;
using Threeyes.Core;

namespace Threeyes.GameFramework
{
    public class HubManagerBase<T> : InstanceBase<T>
        , IHubProgramActiveHandler
    where T : InstanceBase<T>
    {
        protected override void SetInstanceFunc()
        {
            base.SetInstanceFunc();
            GameFrameworkTool.RegisterManagerHolder(this);
        }

        #region NaughtyAttributes
        protected const string foldoutName_Debug = "[Debug]";
        #endregion

        #region Callback
        protected bool isProgramActived = false;//缓存项目的激活状态，便于Manager自行决定是否执行后续任务(不直接读取CommonSettingManager的相关字段，是为了避免Update中频繁访问，降低性能）
        public void OnProgramActiveChanged(bool isActive)
        {
            isProgramActived = isActive;
            OnProgramActiveChangedFunc(isActive);
        }
        protected virtual void OnProgramActiveChangedFunc(bool isActive)
        {
            enabled = isActive;//通过disable，禁止Update等常用方法运行
        }
        #endregion
    }
}
