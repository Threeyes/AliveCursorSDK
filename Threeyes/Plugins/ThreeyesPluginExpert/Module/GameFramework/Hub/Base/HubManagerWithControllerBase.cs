using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threeyes.GameFramework
{
    public class HubManagerWithControllerBase<T, TControllerInterface, TDefaultController> : HubManagerBase<T>, IHubManagerWithController<TControllerInterface>
        where T : HubManagerWithControllerBase<T, TControllerInterface, TDefaultController>
        where TDefaultController : TControllerInterface
    {
        public TControllerInterface ActiveController { get { return modController != null ? modController : defaultController; } }
        protected TControllerInterface modController;//Mod自定义的Controller（可空）
        [SerializeField] protected TDefaultController defaultController;//使用具体类型，便于场景引用
    }
}