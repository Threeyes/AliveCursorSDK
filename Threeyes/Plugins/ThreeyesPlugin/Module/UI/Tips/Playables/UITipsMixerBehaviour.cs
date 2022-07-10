#if UNITY_2020_1_OR_NEWER
#define NewVersion
#else//旧版本直接激活
#define Active
#endif
#if NewVersion && USE_Timeline
#define Active
#endif
#if Active
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class UITipsMixerBehaviour : MixerBehaviourBase<UITipsBehaviour>
{
    // NOTE: This function is called at runtime and edit time.  Keep that in mind when setting the values of properties.
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        int inputCount = playable.GetInputCount();

        for (int i = 0; i < inputCount; i++)
        {
            float inputWeight = playable.GetInputWeight(i);
            ScriptPlayable<UITipsBehaviour> inputPlayable = (ScriptPlayable<UITipsBehaviour>)playable.GetInput(i);
            UITipsBehaviour input = inputPlayable.GetBehaviour();

            // Use the above variables to process each frame of this playable.     
        }
    }
}
#endif