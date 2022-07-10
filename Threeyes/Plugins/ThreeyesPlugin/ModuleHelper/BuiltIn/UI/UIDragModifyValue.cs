using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// 通过拖拽该Graphic，实现更改值的功能
/// 
/// 
/// ToUpdate:参考Button，改为继承Selectable，以便使用CanvasGroup的interactable功能
/// </summary>
public class UIDragModifyValue : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Range(1, 10)]
    public int unitThreshold = 5;//移动多少像素代表一个单位
    public IntEvent onAddDeltaUnit;//数值代表增减的单元

    [Header("Runtime")]
    public Vector2 lastPos;
    ScrollRect cacheScrollRect;
    bool shouldIgnore = false;
    bool hasChangeValue = false;

    ///ToDo:
    ///1.如果鼠标跳转屏幕后，需要立即更新startPosition（通过AC的子类实现，该类为通用代码）
    ///
    ///
    /// 实现：
    ///1.按下时记录初始位置，并且临时搜索并禁用父物体的ScrollView
    ///2.拖拽时，超过一个百分比后，发送一个AddDelta事件
    ///3.结束后，启用ScrollView
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!IsInteractable())
            return;

        lastPos = eventData.position;
        shouldIgnore = false;
        hasChangeValue = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!IsInteractable())
            return;

        if (shouldIgnore)
            return;

        //Todo：只有当超过一定值时才开始禁用Scroll

        Vector2 deltaPosition = eventData.position - lastPos;

        //用户DragY轴超过一定距离且hasChangeValue仍为false，此时就应该判断用户正在滑动ScrollView而不是更改该值，从而忽略后续的滑动
        if (Mathf.Abs(deltaPosition.y) > unitThreshold && !hasChangeValue)
        {
            shouldIgnore = true;
            return;
        }
        if (Mathf.Abs(deltaPosition.x) > unitThreshold)
        {
            //按照比例调用AddDelta，然后不足一个单元的多余值就保留
            int deltaUnit = (int)(deltaPosition.x / unitThreshold);
            onAddDeltaUnit.Invoke(deltaUnit);
            lastPos.x = lastPos.x + unitThreshold * deltaUnit;

            if (!hasChangeValue)
            {
                //临时禁用
                cacheScrollRect = GetComponentInParent<ScrollRect>();
                if (cacheScrollRect)
                    cacheScrollRect.enabled = false;

                hasChangeValue = true;
            }
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!IsInteractable())
            return;

        if (shouldIgnore)
            return;

        if (hasChangeValue && cacheScrollRect)
            cacheScrollRect.enabled = true;
    }

    /// <summary>
    /// Ref：UnityEngine.UI.Selectable
    /// </summary>
    /// <returns></returns>
    public virtual bool IsInteractable()
    {
        CanvasGroup canvasGroup = transform.GetComponentInParent<CanvasGroup>(true);
        if (!canvasGroup)
            return true;

        return canvasGroup.interactable;
    }
}
