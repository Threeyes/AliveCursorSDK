using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if USE_VRTK
using VRTK;
#endif

public class VRTK_UICanvasEx :
#if USE_VRTK
VRTK_UICanvas
#else
MonoBehaviour
#endif
{
#if USE_VRTK
    protected override void OnDisable()
    {
        //PS:禁用时不销毁对应组件，避免频繁显隐导致指定组件初始化失败
        //base.OnDisable();
    }
#endif
}
