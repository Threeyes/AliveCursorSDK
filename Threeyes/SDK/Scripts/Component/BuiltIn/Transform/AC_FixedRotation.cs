using Newtonsoft.Json;
using UnityEngine;
/// <summary>
/// Set world rotation to fixed value
/// 
/// Use case: 2D UI
/// </summary>
public class AC_FixedRotation : AC_ConfigableUpdateComponentBase<Transform, AC_SOFixedRotationConfig, AC_FixedRotation.ConfigInfo>
{
    protected override void UpdateFunc()
    {
        base.UpdateFunc();
        Comp.eulerAngles = Config.targetWorldAngle;
    }

    #region Define
    [System.Serializable]
    public class ConfigInfo : AC_SerializableDataBase
    {
        public Vector3 targetWorldAngle = Vector3.zero;
    }
    #endregion
}