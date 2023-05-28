using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Threeyes.Action
{
    public abstract class SOAction_AnimatorBase<TActionConfig, TParam> : SOActionWithConfigBase<ActionRuntimeData<TActionConfig, TParam, Animator>, TActionConfig, TParam, Animator>
        where TActionConfig : ActionConfigBase<TParam>
    {
    }
}