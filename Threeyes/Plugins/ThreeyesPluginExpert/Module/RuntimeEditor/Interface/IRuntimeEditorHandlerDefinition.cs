using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Threeyes.RuntimeEditor
{
    ///参考命名规范（增加RE可以避免与XR等重名，对进入、退出等事件进行拆分可以更加灵活）：
    ///-UnityEngine.XR.Interaction.Toolkit
    ///     -SelectEnterEventArgs
    ///     -OnFirstSelectEntered

    //——【ToAdd字段】——
    public class BaseEventArgs { }

    public class RESelectEnterEventArgs : BaseEventArgs { }
    public class RESelectExitEventArgs : BaseEventArgs { }

    //——TransformGizmoManager——
    public interface IRuntimeEditorSelectEnterHandler
    {
        void OnRuntimeEditorSelectEntered(RESelectEnterEventArgs args);
    }
    public interface IRuntimeEditorSelectExitHandler
    {
        void OnRuntimeEditorSelectExited(RESelectExitEventArgs args);
    }
}