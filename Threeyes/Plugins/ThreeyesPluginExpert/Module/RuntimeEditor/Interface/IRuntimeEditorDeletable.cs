using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Threeyes.RuntimeEditor
{
    public interface IRuntimeEditorDeletable
    {
        //ToAdd:子接口继承IUndoProvider并返回UndoCommand
        void RuntimeEditorDelete();
    }
}
