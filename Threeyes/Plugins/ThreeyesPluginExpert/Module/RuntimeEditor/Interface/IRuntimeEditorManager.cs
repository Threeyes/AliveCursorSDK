using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threeyes.RuntimeEditor
{
    public interface IRuntimeEditorManager
    {
        bool IsActive { get; }

        //ToAdd：当前选中的物体List
    }
}