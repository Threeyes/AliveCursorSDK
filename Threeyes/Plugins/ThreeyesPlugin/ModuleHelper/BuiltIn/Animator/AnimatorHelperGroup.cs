using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AnimatorHelperGroup : ComponentHelperGroupBase<AnimatorHelper, Animator>
{

    public void RandomSpeed()
    {
        ForEachChildComponent((a) => a.RandomSpeed());
    }

    public void CrossFade(string name)
    {
        ForEachChildComponent((a) => a.CrossFade(name));
    }
    public void SetTrigger(string name)
    {
        ForEachChildComponent((a) => a.SetTrigger(name));
    }
    public void SetBoolOn(string name)
    {
        ForEachChildComponent((a) => a.SetBoolOn(name));

    }
    public void SetBoolOff(string name)
    {
        ForEachChildComponent((a) => a.SetBoolOff(name));
    }

    public void EnableAnimator(bool isEnable)
    {
        ForEachChildComponent((a) => a.enabled = isEnable);
    }
}
