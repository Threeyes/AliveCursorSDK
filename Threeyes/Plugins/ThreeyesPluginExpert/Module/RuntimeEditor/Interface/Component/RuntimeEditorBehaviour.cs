using System.Collections;
using System.Collections.Generic;
using Threeyes.Core;
using UnityEngine;

namespace Threeyes.RuntimeEditor
{
    /// <summary>
    /// 发送RuntimeEditor选中/取消选中等事件
    /// </summary>
    public class RuntimeEditorBehaviour : MonoBehaviour
        , IRuntimeEditorSelectEnterHandler
        , IRuntimeEditorSelectExitHandler
    {
        #region Property & Field
        public bool IsSecectEntering { get; set; } = false;
        public BoolEvent onSelectEnterExit = new BoolEvent();

        #endregion

        //void Start()
        //{
        //    //Init on start（方便隐藏用户的某些设置）（非必须，先注释）
        //    OnRuntimeEditorSelectExitedFunc();
        //}
        public void OnRuntimeEditorSelectEntered(RESelectEnterEventArgs args)
        {
            IsSecectEntering = true;
            onSelectEnterExit.Invoke(true);
        }

        public void OnRuntimeEditorSelectExited(RESelectExitEventArgs args)
        {
            OnRuntimeEditorSelectExitedFunc();
        }

        private void OnRuntimeEditorSelectExitedFunc()
        {
            IsSecectEntering = false;
            onSelectEnterExit.Invoke(false);
        }
    }
}