using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Threeyes.Core;
namespace Threeyes.RuntimeEditor
{
    /// <summary>
    /// Mark a gameobject as deletable
    /// </summary>
    public class RuntimeEditorDeletable : MonoBehaviour, IRuntimeEditorDeletable
    {
        public void RuntimeEditorDelete()
        {
            gameObject.DestroyAtOnce();
        }
    }
}