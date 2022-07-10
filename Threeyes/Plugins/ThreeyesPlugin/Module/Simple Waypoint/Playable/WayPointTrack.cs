#if UNITY_2020_1_OR_NEWER
#define NewVersion
#else//�ɰ汾ֱ�Ӽ���
#define Active
#endif
#if NewVersion && USE_Timeline
#define Active
#endif
#if Active
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(1f, 0.3284664f, 0f)]
[TrackClipType(typeof(WayPointClip))]
[TrackBindingType(typeof(Transform))]
public class WayPointTrack : TrackBase<WayPointTrack, WayPointBehaviour, WayPointMixerBehaviour, WayPointClip, Transform>
{
}
#endif