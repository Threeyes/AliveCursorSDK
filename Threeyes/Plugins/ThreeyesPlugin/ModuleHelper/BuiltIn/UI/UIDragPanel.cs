using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// ���ܣ���ק����
/// ʹ�÷��������ڴ��ڵ�����������������У�Ȼ��panelRectTransform���ô������
/// Ref:Unity Technologies/UI Samples.DragPanel.cs from AssetStore
/// </summary>
public class UIDragPanel : MonoBehaviour, IPointerDownHandler, IDragHandler
{

    public RectTransform panelRectTransform;//��Ҫ��ק��Panel��Ҫ���и������ұ߽緶Χ������ȷ
    private RectTransform parentRectTransform;

    private Vector2 originalLocalPointerPosition;
    private Vector3 originalPanelLocalPosition;


    // Clamp panel to area of parent
    public void ClampToWindow()
    {
        Init();
        
        Vector3 pos = panelRectTransform.localPosition;

        Vector3 minPosition = parentRectTransform.rect.min - panelRectTransform.rect.min;
        Vector3 maxPosition = parentRectTransform.rect.max - panelRectTransform.rect.max;

        pos.x = Mathf.Clamp(panelRectTransform.localPosition.x, minPosition.x, maxPosition.x);
        pos.y = Mathf.Clamp(panelRectTransform.localPosition.y, minPosition.y, maxPosition.y);

        panelRectTransform.localPosition = pos;
    }

    void Awake()
    {
        Init();
    }

    bool isInit = false;
    void Init()
    {
        if (isInit)
            return;

        if (!panelRectTransform)
            panelRectTransform = transform.parent as RectTransform;

        if (panelRectTransform.parent)
            parentRectTransform = panelRectTransform.parent.GetComponent<RectTransform>();
        else
            Debug.LogError(panelRectTransform.ToString() + " ȱ�ٸ����壡");
    }

    public void OnPointerDown(PointerEventData data)
    {
        originalPanelLocalPosition = panelRectTransform.localPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, data.position, data.pressEventCamera, out originalLocalPointerPosition);
    }

    public void OnDrag(PointerEventData data)
    {
        if (panelRectTransform == null || parentRectTransform == null)
            return;

        Vector2 localPointerPosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, data.position, data.pressEventCamera, out localPointerPosition))
        {
            Vector3 offsetToOriginal = localPointerPosition - originalLocalPointerPosition;
            panelRectTransform.localPosition = originalPanelLocalPosition + offsetToOriginal;
        }

        ClampToWindow();
    }

}
