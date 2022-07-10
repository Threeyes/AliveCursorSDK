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

[Serializable]
public class WayPointClip : ClipBase<WayPointTrack, WayPointBehaviour, Transform>
{
    public ExposedReference<SimpleWaypointGroup> wayPointGroup;

#region Outdated
    [Header("Outdated")]
    public bool isUseLocalCurve = true;
    public AnimationCurve posTweenCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    //Todo
    public enum TurnType
    {
        Null,
        LookAt,
        TweenBetween
    }
    public bool isLookAt = true;
    public float rotateSpeed = 10;
    public float startRotPercent = 0.2f;
    public AnimationCurve rotTweenCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

#endregion

    public override ClipCaps clipCaps
    {
        get { return ClipCaps.Blending; }
    }


    public override void InitClone(WayPointBehaviour clone, PlayableGraph graph, GameObject owner)
    {
        clone.wayPointGroup = wayPointGroup.Resolve(graph.GetResolver());
        clone.isUseLocalCurve = isUseLocalCurve;
        clone.posTweenCurve = posTweenCurve;
        clone.isLookAt = isLookAt;
        clone.rotateSpeed = rotateSpeed;
        clone.startRotPercent = startRotPercent;
        clone.rotTweenCurve = rotTweenCurve;
    }
}
#endif