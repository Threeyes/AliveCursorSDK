using Threeyes.Config;
using UnityEngine;

public class AC_LookAtCamera : AC_ConfigurableUpdateComponentBase<Transform, AC_SOLookAtCameraConfig, AC_LookAtCamera.ConfigInfo>
{
    protected override void UpdateFunc()
    {
        base.UpdateFunc();
        Comp.LookAt(AC_ManagerHolder.EnvironmentManager.MainCamera.transform.position, Config.worldUp);
    }

    #region Define
    [System.Serializable]
    public class ConfigInfo : SerializableDataBase
    {
        public Vector3 worldUp = Vector3.up;
    }
    #endregion
}
