#if USE_DOTweenPro
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoTweenAnimationHelperGroup : ComponentGroupBase<DoTweenAnimationHelper>
{

    public void DoPlayPause(bool isPlay)
    {
        ForEachChildComponent((c) => c.DoPlayPause(isPlay));
    }
}
#endif