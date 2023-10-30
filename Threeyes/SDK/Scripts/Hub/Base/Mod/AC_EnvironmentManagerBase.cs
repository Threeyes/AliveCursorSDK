using System.Collections;
using System.Collections.Generic;
using Threeyes.Steamworks;
using UnityEngine;

public class AC_EnvironmentManagerBase<T> : EnvironmentManagerBase<T, IAC_EnvironmentController, AC_DefaultEnvironmentController, IAC_SOEnvironmentControllerConfig>
    , IAC_EnvironmentManager
where T : AC_EnvironmentManagerBase<T>
{
}
