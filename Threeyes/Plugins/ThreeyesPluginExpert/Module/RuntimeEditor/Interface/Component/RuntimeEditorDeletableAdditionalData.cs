using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Threeyes.RuntimeEditor
{
    /// <summary>
    /// Optional config for IRuntimeEditorDeletable
    /// </summary>
    public class RuntimeEditorDeletableAdditionalData : MonoBehaviour
    {
        public bool IsShowWarningOnDelete { get { return isShowWarningOnDelete; } set { isShowWarningOnDelete = value; } }
        [SerializeField] bool isShowWarningOnDelete = false;

    }
}
