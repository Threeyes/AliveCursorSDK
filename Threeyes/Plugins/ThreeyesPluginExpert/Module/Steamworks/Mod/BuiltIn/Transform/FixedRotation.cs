using System.Collections;
using System.Collections.Generic;
using Threeyes.Config;
using UnityEngine;

namespace Threeyes.Steamworks
{
    public class FixedRotation : ConfigurableUpdateComponentBase<Transform, SOFixedRotationConfig, FixedRotation.ConfigInfo>
    {
        protected override void UpdateFunc()
        {
            base.UpdateFunc();
            Comp.eulerAngles = Config.targetWorldAngle;
        }

        #region Define
        [System.Serializable]
        public class ConfigInfo : SerializableDataBase
        {
            public Vector3 targetWorldAngle = Vector3.zero;
        }
        #endregion
    }
}