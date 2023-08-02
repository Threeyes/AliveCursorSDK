using Threeyes.Steamworks;

/// <summary>
/// Follow and look at target
/// 
/// 要求：
/// -跟随某一点移动
/// </summary>
public class AC_ObjectMovement_FollowTarget : ObjectMovement_FollowTarget
{
    protected override float RuntimeMoveSpeed
    {
        get
        {
            return base.RuntimeMoveSpeed * AC_ManagerHolder.CommonSettingManager.CursorSize;
        }
    }
}