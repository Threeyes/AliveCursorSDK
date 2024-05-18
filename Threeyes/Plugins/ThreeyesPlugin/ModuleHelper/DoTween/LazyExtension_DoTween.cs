#if USE_DOTween
using DG.Tweening;

public static class LazyExtension_DoTween
{
    #region Tween
    /// <summary>
    /// 
    /// </summary>
    /// <param name="tween"></param>
    /// <returns>确保该值最后为null</returns>
    public static Tween TryKill(this Tween tween)
    {
        if (tween != null && tween.IsActive() && !tween.IsComplete())
        {
            tween.Kill();
            tween = null;
        }
        return null;
    }

    public static Tweener TryKill(this Tweener tween)
    {
        if (tween != null && tween.IsActive() && !tween.IsComplete())
        {
            tween.Kill();
            tween = null;
        }
        return null;
    }

    public static bool Equal(this Tween tween1, Tween tween2)
    {
        if (tween1 == null || tween2 == null)
            return false;

        if (tween1.Duration() != tween2.Duration())
            return false;
        if (tween1.isBackwards != tween2.isBackwards)
            return false;

        return true;
    }

    #endregion
}
#endif
