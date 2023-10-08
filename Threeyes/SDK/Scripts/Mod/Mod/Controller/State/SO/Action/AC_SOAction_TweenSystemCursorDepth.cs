using UnityEngine;
using DG.Tweening;
using Threeyes.Action;

/// <summary>
/// Change System Cursor's Depth on Scene to achieve Move In/Out behaviour, mainly for StateController
///
/// Valid for CursorState：Enter/Exit/Working
/// </summary>
[CreateAssetMenu(menuName = AC_EditorDefinition.AssetMenuPrefix_Root_Mod_Controller_State_Action + "TweenSystemCursorDepth", fileName = "TweenSystemCursorDepth")]
#if UNITY_EDITOR
[UnityEditor.CanEditMultipleObjects]
#endif
public class AC_SOAction_TweenSystemCursorDepth : SOAction_TweenBase<ActionConfig_TweenFloat, float, Transform>
{
    protected override Tween CreateTween(ActionTweenRuntimeData<ActionConfig_TweenFloat, float, Transform> runtimeData)
    {
        //通过更改Depth，实现进出效果
        Tween tween = DOTween.To(
            () => AC_ManagerHolder.SystemCursorManager.CurDepth,
            (f) => AC_ManagerHolder.SystemCursorManager.CurDepth = f,
            runtimeData.EndValue,
            runtimeData.Duration);

        return tween;
    }
}