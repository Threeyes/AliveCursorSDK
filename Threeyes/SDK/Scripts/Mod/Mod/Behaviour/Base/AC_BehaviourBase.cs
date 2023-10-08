using System.Collections.Generic;
using Threeyes.EventPlayer;
/// <summary>
/// 
/// (PS:之所以继承EventPlayer是因为其有丰富的界面绘制方法，方便通过界面调试，不需要再弄一个单独的类来监听事件)
/// </summary>
public abstract class AC_BehaviourBase : EventPlayer_SOAction
{
    #region Editor Method
#if UNITY_EDITOR

    protected List<string> listHierarchyParamCache = new List<string>();

#endif
    #endregion
}
