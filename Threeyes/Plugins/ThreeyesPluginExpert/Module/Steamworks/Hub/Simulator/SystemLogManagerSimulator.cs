using System.Collections;
using System.Collections.Generic;
using Threeyes.Core;
using Threeyes.Log;
using UnityEngine;

namespace Threeyes.Steamworks
{
    public class SystemLogManagerSimulator : LogManager_Editor
    {
        protected override void SetInstanceFunc()
        {
            base.SetInstanceFunc();
            ManagerHolderTool.Register(this, typeof(LogManagerHolder));//注册指定接口
        }
    }
}