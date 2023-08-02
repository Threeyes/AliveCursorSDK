using System.Collections;
using System.Collections.Generic;
using Threeyes.Log;
using UnityEngine;

namespace Threeyes.Steamworks
{
    public class SystemLogManagerSimulator : LogManager_Editor
    {
        protected override void SetInstanceFunc()
        {
            base.SetInstanceFunc();
            SteamworksTool.RegistManagerHolder(this);
        }
    }
}