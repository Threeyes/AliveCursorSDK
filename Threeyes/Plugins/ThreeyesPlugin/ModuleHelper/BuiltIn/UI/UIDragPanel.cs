using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Threeyes.ModuleHelper
{

    /// <summary>
    /// 功能：拖拽窗口
    /// 使用方法：挂在窗口的任意子物体标题栏中，然后panelRectTransform引用窗口组件
    /// Ref:Unity Technologies/UI Samples.DragPanel.cs from AssetStore
    ///
    /// ToUpdate:
    /// 1.支持相对值的RectTransform
    /// </summary>
    public class UIDragPanel : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        public bool initOnAwake = true;//如果是挂载Prefab上，可以禁用并主动设置引用，否则会报警告
        public RectTransform panelRectTransform;//需要拖拽的Panel，要求有父物体且边界范围设置正确
        public RectTransform parentRectTransform;

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
            if (initOnAwake)
                Init();
        }

        bool isInit = false;
        [ContextMenu("Init")]
        public void Init()
        {
            if (isInit)
                return;

            if (!panelRectTransform)
                panelRectTransform = transform.parent as RectTransform;

            if (panelRectTransform && panelRectTransform.parent)
                parentRectTransform = panelRectTransform.parent.GetComponent<RectTransform>();
            else
                Debug.LogWarning($"缺少{nameof(panelRectTransform)}或{nameof(parentRectTransform)}！");
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
}