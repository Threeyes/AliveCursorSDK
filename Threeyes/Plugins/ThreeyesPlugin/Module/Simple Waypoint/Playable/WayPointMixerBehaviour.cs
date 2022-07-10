#if UNITY_2020_1_OR_NEWER
#define NewVersion
#else//旧版本直接激活
#define Active
#endif
#if NewVersion && USE_Timeline
#define Active
#endif
#if Active
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class WayPointMixerBehaviour : MixerBehaviourBase<WayPointBehaviour>
{

    /// <summary>
    /// 初始化位置
    /// </summary>
    /// <param name="playable"></param>
    public override void OnGraphStart(Playable playable)
    {
        //Debug.Log(playable + " OnGraphStart");

        //在游戏运行时，将目标重置到第一个waypointgroup的默认位置
        int inputCount = playable.GetInputCount();
        if (inputCount > 0)
        {
            ScriptPlayable<WayPointBehaviour> inputPlayable = (ScriptPlayable<WayPointBehaviour>)playable.GetInput(0);
            WayPointBehaviour input = inputPlayable.GetBehaviour();
            input.Calculate(0);
        }

        base.OnGraphStart(playable);
    }

    // NOTE: This function is called at runtime and edit time.  Keep that in mind when setting the values of properties.
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        Transform trackBinding = playerData as Transform;

        if (!trackBinding)
            return;

        int inputCount = playable.GetInputCount();

        for (int i = 0; i < inputCount; i++)
        {
            float inputWeight = playable.GetInputWeight(i);
            ScriptPlayable<WayPointBehaviour> inputPlayable = (ScriptPlayable<WayPointBehaviour>)playable.GetInput(i);
            WayPointBehaviour input = inputPlayable.GetBehaviour();

            // Use the above variables to process each frame of this playable.

        }
    }
}
#endif