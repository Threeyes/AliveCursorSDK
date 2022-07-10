#if USE_VRTK
#if UNITY_2018
using VRTK;
/// <summary>
/// 自定义的位置传送组件
/// </summary>
[System.Obsolete("Use RemoteDestinationPoint instead")]
public class CustomDestinationPoint : VRTK_DestinationPoint
{
    public DestinationPointEventHandler onSet;
    protected override void DoDestinationMarkerSet(object sender, DestinationMarkerEventArgs e)
    {
        if (e.raycastHit.transform == transform)
        {
            //新增：增加了一个事件，以防snapToPoint为false时不会调用DestinationPointDisabled事件
            if (onSet != null)
                onSet(this);

            currentDestinationPoint = this;
            if (snapToPoint)
            {
                e.raycastHit.point = destinationLocation.position;
                setDestination = StartCoroutine(DoDestinationMarkerSetAtEndOfFrame(e));
            }
        }
        else if (currentDestinationPoint != this)
        {
            ResetPoint();
        }
        else if (currentDestinationPoint != null && e.raycastHit.transform != currentDestinationPoint.transform)
        {
            currentDestinationPoint = null;
            ResetPoint();
        }
    }
}
#endif
#endif