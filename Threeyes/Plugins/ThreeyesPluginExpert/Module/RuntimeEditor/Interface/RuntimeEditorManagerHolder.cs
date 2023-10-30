using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Threeyes.RuntimeEditor
{
    public static class RuntimeEditorManagerHolder
    {
        public static IRuntimeEditorManager RuntimeEditorManager { get; internal set; }
    }
}