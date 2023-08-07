#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Threeyes.Steamworks
{
    /// <summary>
    /// 在Simulator场景中提供Mod调试信息
    /// </summary>

    public class AssistantManagerSimulator : MonoBehaviour
    {
        /// <summary>
        /// 因为某些原因需要临时隐藏UI（如截图）
        /// </summary>
        /// <param name="isShow"></param>
        public virtual void TempShowInfoGroup(bool isShow)
        {

        }


        #region Utility
        protected static void ShowGameobjectWithoutSaving(GameObject go, bool isShow)
        {
            go.SetActive(isShow);
            EditorUtility.ClearDirty(go);//避免修改导致Scene需要保存
        }
        #endregion
    }
}
#endif
