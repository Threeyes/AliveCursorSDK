using System.Collections;
using System.Collections.Generic;
using Threeyes.Core;
using UnityEngine;

namespace Threeyes.ShowHide
{
    /// <summary>
    /// 管理特定的显隐组件(每个总根物体有一个）
    /// </summary>
    public class HideOnAwakeManager : MonoBehaviour
    {
        private void Awake()
        {
            transform.Recursive(
                (tf) =>
             {
                 HideOnAwake[] arrHideOnAwke = tf.GetComponents<HideOnAwake>();
                 foreach (var hoa in arrHideOnAwke)
                 {
                     hoa.TryHideOnAwake();
                 }
             });
        }

        public static void Init(GameObject go)
        {
            if (!go)
                return;
            Transform tfRoot = go.transform.root;//给跟物体加一个Manager
            tfRoot.gameObject.AddComponentOnce<HideOnAwakeManager>();
        }
    }
}