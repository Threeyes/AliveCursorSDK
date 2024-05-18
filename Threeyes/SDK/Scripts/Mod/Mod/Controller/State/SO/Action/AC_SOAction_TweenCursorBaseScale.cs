using DG.Tweening;
using Threeyes.Action;
using UnityEngine;
/// <summary>
/// Change TransformManager.CursorBaseScale to achieve Show/Hide behaviour, mainly for StateController
///
/// Valid for CursorState：Show/Hide/StandBy
/// </summary>
[CreateAssetMenu(menuName = AC_EditorDefinition.AssetMenuPrefix_Root_Mod_Controller_State_Action + "TweenCursorBaseScale", fileName = "TweenCursorBaseScale")]
#if UNITY_EDITOR
[UnityEditor.CanEditMultipleObjects]
#endif
public class AC_SOAction_TweenCursorBaseScale : SOAction_TweenBase<ActionConfig_TweenVector3Ex, Vector3, Transform>
{
	protected override Tween CreateTween(ActionTweenRuntimeData<ActionConfig_TweenVector3Ex, Vector3, Transform> runtimeData)
	{
        //PS：不需要Punch/Shake等变化
        var config = runtimeData.Config;
        Tween tween = DOTween.To(
			() => AC_ManagerHolder.TransformManager.CursorBaseScale,
			(f) => AC_ManagerHolder.TransformManager.CursorBaseScale = f,
            config.EndValue,
            config.Duration);

		return tween;
	}
}