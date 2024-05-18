using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threeyes.RuntimeEditor
{
    public interface IRuntimeEditorManager
    {
        bool IsActive { get; }

    }

    public interface IRuntimeEditor_ModeActiveHandler
    {
        /// <summary>
        /// Enter/Exit RuntimeEditor Mode
        /// </summary>
        /// <param name="isActive"></param>
        void OnRuntimeEditorModeChanged(bool isActive);
    }
}