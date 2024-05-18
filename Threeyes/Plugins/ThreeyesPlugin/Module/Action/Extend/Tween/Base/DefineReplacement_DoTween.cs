
namespace Threeyes.Action
{
    /// <summary>
    /// 该组件的作用是：在指定宏定义不激活时，保证相关数据类可用
    /// </summary>

#if !USE_DOTween
    //
    // 摘要:
    //     Rotation mode used with DORotate methods
    public enum RotateMode
    {
        //
        // 摘要:
        //     Fastest way that never rotates beyond 360°
        Fast,
        //
        // 摘要:
        //     Fastest way that rotates beyond 360°
        FastBeyond360,
        //
        // 摘要:
        //     Adds the given rotation to the transform using world axis and an advanced precision
        //     mode (like when using transform.Rotate(Space.World)).
        //     In this mode the end value is is always considered relative
        WorldAxisAdd,
        //
        // 摘要:
        //     Adds the given rotation to the transform's local axis (like when rotating an
        //     object with the "local" switch enabled in Unity's editor or using transform.Rotate(Space.Self)).
        //     In this mode the end value is is always considered relative
        LocalAxisAdd
    }

    public enum Ease
    {
        Unset,
        Linear,
        InSine,
        OutSine,
        InOutSine,
        InQuad,
        OutQuad,
        InOutQuad,
        InCubic,
        OutCubic,
        InOutCubic,
        InQuart,
        OutQuart,
        InOutQuart,
        InQuint,
        OutQuint,
        InOutQuint,
        InExpo,
        OutExpo,
        InOutExpo,
        InCirc,
        OutCirc,
        InOutCirc,
        InElastic,
        OutElastic,
        InOutElastic,
        InBack,
        OutBack,
        InOutBack,
        InBounce,
        OutBounce,
        InOutBounce,
        Flash,
        InFlash,
        OutFlash,
        InOutFlash,
        //
        // 摘要:
        //     Don't assign this! It's assigned automatically when creating 0 duration tweens
        INTERNAL_Zero,
        //
        // 摘要:
        //     Don't assign this! It's assigned automatically when setting the ease to an AnimationCurve
        //     or to a custom ease function
        INTERNAL_Custom
    }

    //
    // 摘要:
    //     Types of loop
    public enum LoopType
    {
        //
        // 摘要:
        //     Each loop cycle restarts from the beginning
        Restart,
        //
        // 摘要:
        //     The tween moves forward and backwards at alternate cycles
        Yoyo,
        //
        // 摘要:
        //     Continuously increments the tween at the end of each loop cycle (A to B, B to
        //     B+(A-B), and so on), thus always moving "onward".
        //     In case of String tweens works only if the tween is set as relative
        Incremental
    }


    //
    // 摘要:
    //     Type of scramble to apply to string tweens
    public enum ScrambleMode
    {
        //
        // 摘要:
        //     No scrambling of characters
        None,
        //
        // 摘要:
        //     A-Z + a-z + 0-9 characters
        All,
        //
        // 摘要:
        //     A-Z characters
        Uppercase,
        //
        // 摘要:
        //     a-z characters
        Lowercase,
        //
        // 摘要:
        //     0-9 characters
        Numerals,
        //
        // 摘要:
        //     Custom characters
        Custom
    }
#endif

}