using System.Collections;
using System.Collections.Generic;
using Threeyes.Config;
using UnityEngine;

namespace Threeyes.Steamworks
{
    /// <summary>
    /// ToAdd:
    /// -可配置朝向的轴
    /// </summary>
    public class LookAtCamera : ConfigurableUpdateComponentBase<Transform, SOLookAtCameraConfig, LookAtCamera.ConfigInfo>
    {
        public bool IsActive
        {
            get { return isActive; }
            set { isActive = value; }
        }
        [SerializeField] protected bool isActive = true;

        protected override void UpdateFunc()
        {
            if (!isActive)
                return;

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