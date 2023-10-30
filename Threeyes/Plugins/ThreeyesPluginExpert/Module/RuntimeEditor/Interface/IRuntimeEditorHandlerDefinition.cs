using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Threeyes.RuntimeEditor
{
    public class BaseEventArgs { }

    //——TransformGizmoManager——
    public interface IRuntimeEditorSelectEnterHandler
    {
        void OnRuntimeEditorSelectEnter(BaseEventArgs args);
    }
    public interface IRuntimeEditorSelectExitHandler
    {
        void OnRuntimeEditorSelectExit(BaseEventArgs args);
    }
}