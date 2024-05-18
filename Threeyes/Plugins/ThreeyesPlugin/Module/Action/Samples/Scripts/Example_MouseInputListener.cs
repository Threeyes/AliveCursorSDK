using UnityEngine;
using UnityEngine.EventSystems;

namespace Threeyes.Action.Example
{
    public class Example_MouseInputListener : MonoBehaviour,
     IScrollHandler
    {
        public EventPlayer_SOAction eventPlayer_SOAction;
        public void OnScroll(PointerEventData eventData)
        {
            //Note: In this case, due to the high input frequency, the SOAction's duration was set to 0 to ensure immediate visuals response.
            eventPlayer_SOAction.PlayWithParam(-eventData.scrollDelta.y);//Send Scroll value, note that the input value is object type
        }
    }
}