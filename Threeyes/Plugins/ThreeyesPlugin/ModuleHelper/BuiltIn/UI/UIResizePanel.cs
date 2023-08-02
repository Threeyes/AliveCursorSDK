using UnityEngine;
using UnityEngine.EventSystems;
/// <summary>
/// 功能：更改窗口尺寸
/// 使用方法：挂在窗口的对应拖拽UI元素中，然后panelRectTransform引用窗口组件
/// 注意：
/// 1.缩放基于Panel的Pivot（eg：如需要基于左上角缩放，则把Pivot设置为（0，1））
/// </summary>
public class UIResizePanel : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    public RectTransform panelRectTransform;
    private RectTransform parentRectTransform;

    public Vector2 minSize = new Vector2(100, 100);
    public Vector2 maxSize = new Vector2(400, 400);//（如果某个值设置为0，那就默认填充满整个父物体）

    [Header("Runtime")]
    public Vector2 runtimeMaxSize;
    public Vector2 originalLocalPointerPosition;
    public Vector2 originalPanelSize;

	/// <summary>
	/// 便于屏幕尺寸更改后重新调用
	///
	/// PS:需要在UICanvas显示完毕后调用，不能直接在Awake/OnEnable调用，因为获取的size可能有误
	/// </summary>
	public void Init()
    {
        if (!panelRectTransform)
            panelRectTransform = transform.parent.GetComponent<RectTransform>();

        if (panelRectTransform?.parent)
        {
            parentRectTransform = panelRectTransform.parent.GetComponent<RectTransform>();
            //更新上限
            runtimeMaxSize.x = maxSize.x <= 0 ? parentRectTransform.GetSize().x : maxSize.x;
            runtimeMaxSize.y = maxSize.y <= 0 ? parentRectTransform.GetSize().y : maxSize.y;
        }
        else
            Debug.LogError(panelRectTransform.ToString() + " 缺少父物体！");
    }

    public void OnPointerDown(PointerEventData data)
    {
        originalPanelSize = panelRectTransform.GetSize();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRectTransform, data.position, data.pressEventCamera, out originalLocalPointerPosition);
    }

    public void OnDrag(PointerEventData data)
    {
        if (panelRectTransform == null)
            return;

        Vector2 localPointerPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRectTransform, data.position, data.pressEventCamera, out localPointerPosition);

        Vector3 offsetToOriginal = localPointerPosition - originalLocalPointerPosition;
        Vector2 sizeDelta = originalPanelSize + new Vector2(offsetToOriginal.x, -offsetToOriginal.y);
        sizeDelta = new Vector2(Mathf.Clamp(sizeDelta.x, minSize.x, runtimeMaxSize.x), Mathf.Clamp(sizeDelta.y, minSize.y, runtimeMaxSize.y));


        panelRectTransform.SetSize(sizeDelta);
    }
}