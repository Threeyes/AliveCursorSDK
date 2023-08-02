using System.Collections;
using System.Collections.Generic;
using Threeyes.Config;
using UnityEngine;

namespace Threeyes.Steamworks
{
    public class LookAtCamera : ConfigurableUpdateComponentBase<Transform, SOLookAtCameraConfig, LookAtCamera.ConfigInfo>
    {
        protected override void UpdateFunc()
        {
            base.UpdateFunc();
            Comp.LookAt(ManagerHolder.EnvironmentManager.MainCamera.transform.position, Config.worldUp);
        }

        #region Define
        [System.Serializable]
        public class ConfigInfo : SerializableDataBase
        {
            public Vector3 worldUp = Vector3.up;
        }
        #endregion
    }
}