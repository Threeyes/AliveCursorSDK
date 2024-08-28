using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threeyes.GameFramework
{
    /// <summary>
    /// Create a bounds area that the front bounds matchs MainCamera's ViewArea
    ///
    /// PS:
    /// 1.该根物体的Pivot位置对应Front的底面
    /// 2.该物体应保证x、y值为0，z为Front的起点，且需要在相机正前方
    /// 3.通过更改该物体的y轴位置来容纳有厚度的Cursor
    /// 4.取名参考ContentSizeFitter
    /// 
    /// ToUpdate:
    /// 1.增加针对相机区域的锥形结构（通过其他名字实现）
    /// 2.提供override相机，便于多相机实现
    /// 3.考虑相机的旋转（基于AD进行研究）
    /// 4.将配置改为SO
    /// 
    /// Warning:
    /// -在编辑器模式下调节Game的尺寸，可能Screen.width返回的数据是错的，不影响（可能原因是dpi设置：https://forum.unity.com/threads/screen-width-is-wrong-in-editor-mode.94572/）
    /// </summary>
    public class ViewBoundsFitter : MonoBehaviour
        , IHubSystemWindow_ChangeCompletedHandler
    {
        Camera TargetCamera { get { return overrideCamera ? overrideCamera : ManagerHolder.EnvironmentManager.MainCamera; } }

        //#Wall Colliders base on MainCamera axis (You can deactive any of them)
        public BoxCollider boxColliderFront;
        public BoxCollider boxColliderBack;
        public BoxCollider boxColliderLeft;
        public BoxCollider boxColliderRight;
        public BoxCollider boxColliderTop;
        public BoxCollider boxColliderBottom;

        //# bounds's property
        public float distanceToCamera = 10;//Front面与相机的距离
        public float edgeThickness = 0.1f;//Collider thickness
        public Camera overrideCamera;

        protected virtual void OnEnable()
        {
            SetEdges();
        }

        #region Callback
        public void OnWindowChangeCompleted()
        {
            SetEdges();
        }
        #endregion
        protected virtual void SetEdges()
        {
            //#1 将根物体移动到相对相机的指定位置
            transform.rotation = TargetCamera.transform.rotation;
            transform.localPosition = TargetCamera.transform.TransformPoint(new Vector3(0, 0, distanceToCamera));

            //#2 计算并设置每个边界物体
            float screenAspect = (float)Screen.width / Screen.height;
            float viewBoundsHeight = distanceToCamera * Mathf.Tan(Mathf.Deg2Rad * TargetCamera.fieldOfView / 2) * 2;//通过三角边算法计算 Height=(直边×tanθ)*2
            float viewBoundsWidth = viewBoundsHeight * screenAspect;
            float viewBoundsDepth = distanceToCamera * 2;

            ///通过改变Transform，方便可视化
            SetEdgeCollider(boxColliderFront, new Vector3(0, 0, edgeThickness / 2), new Vector3(viewBoundsWidth, viewBoundsHeight, edgeThickness));
            SetEdgeCollider(boxColliderBack, new Vector3(0, 0, -viewBoundsDepth - edgeThickness / 2), new Vector3(viewBoundsWidth, viewBoundsHeight, edgeThickness));

            SetEdgeCollider(boxColliderLeft, new Vector3(-viewBoundsWidth / 2 - edgeThickness / 2, 0, -viewBoundsDepth / 2), new Vector3(edgeThickness, viewBoundsHeight, viewBoundsDepth + edgeThickness * 2));
            SetEdgeCollider(boxColliderRight, new Vector3(viewBoundsWidth / 2 + edgeThickness / 2, 0, -viewBoundsDepth / 2), new Vector3(edgeThickness, viewBoundsHeight, viewBoundsDepth + edgeThickness * 2));

            SetEdgeCollider(boxColliderTop, new Vector3(0, viewBoundsHeight / 2 + edgeThickness / 2, -viewBoundsDepth / 2), new Vector3(viewBoundsWidth + edgeThickness * 2, edgeThickness, viewBoundsDepth + edgeThickness * 2));
            SetEdgeCollider(boxColliderBottom, new Vector3(0, -viewBoundsHeight / 2 - edgeThickness / 2, -viewBoundsDepth / 2), new Vector3(viewBoundsWidth + edgeThickness * 2, edgeThickness, viewBoundsDepth + edgeThickness * 2));
        }

        static void SetEdgeCollider(BoxCollider boxCollider, Vector3 localPosition, Vector3 localScale)
        {
            if (!boxCollider)
                return;
            boxCollider.transform.localPosition = localPosition;
            boxCollider.transform.localScale = localScale;
        }
    }
}