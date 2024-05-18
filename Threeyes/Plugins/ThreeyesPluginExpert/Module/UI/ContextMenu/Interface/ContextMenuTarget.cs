using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Threeyes.UI
{
    /// <summary>
    /// 指定右键菜单的目标
    /// </summary>
    public class ContextMenuTarget : MonoBehaviour, IContextMenuTarget
    {
        public GameObject Target { get { return target; } set { target = value; } }
        [SerializeField] GameObject target;


        GameObject IContextMenuTarget.GetTarget()
        {
            return Target ? Target : gameObject;
        }
    }
}