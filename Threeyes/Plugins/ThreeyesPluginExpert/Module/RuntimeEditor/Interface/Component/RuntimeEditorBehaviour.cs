using System.Collections;
using System.Collections.Generic;
using Threeyes.Core;
using UnityEngine;

namespace Threeyes.RuntimeEditor
{
    /// <summary>
    /// 接收RuntimeEditor模式物体被选中/取消选中等事件
    ///
    /// </summary>
    public class RuntimeEditorBehaviour : MonoBehaviour
        , IRuntimeEditor_ModeActiveHandler
        , IRuntimeEditorSelectEnterHandler
        , IRuntimeEditorSelectExitHandler
    {
        #region Property & Field
        public bool IsModeActive { get; set; } = false;

        public bool IsSelected { get; set; } = false;
        public BoolEvent onModeActiveDeactive = new BoolEvent();
        public BoolEvent onSelectEnterExit = new BoolEvent();

        #endregion

        void Start()
        {
            ////Init on start（方便隐藏用户的某些设置）（非必须，先注释）
            //OnRuntimeEditorSelectExitedFunc();
            if (RuntimeEditorManagerHolder.RuntimeEditorManager != null)//避免在模型在其他场景出现报错
            {
                OnRuntimeEditorModeChanged(RuntimeEditorManagerHolder.RuntimeEditorManager.IsActive);//确保在编辑模式生成物体时，能够直接调用事件
            }
        }

        public void OnRuntimeEditorModeChanged(bool isActive)
        {
            IsModeActive = isActive;
            onModeActiveDeactive.Invoke(isActive);
        }

        public void OnRuntimeEditorSelectEntered(RESelectEnterEventArgs args)
        {
            IsSelected = true;
            onSelectEnterExit.Invoke(true);
        }

        public void OnRuntimeEditorSelectExited(RESelectExitEventArgs args)
        {
            IsSelected = false;
            onSelectEnterExit.Invoke(false);
        }
    }
}