using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Threeyes.GameFramework;

public class AC_PostProcessingManagerBase<T> : PostProcessingManagerBase<T, IAC_PostProcessingController, AC_DefaultPostProcessingController, IAC_SOPostProcessingControllerConfig>
    , IAC_PostProcessingManager
where T : AC_PostProcessingManagerBase<T>
{
}
