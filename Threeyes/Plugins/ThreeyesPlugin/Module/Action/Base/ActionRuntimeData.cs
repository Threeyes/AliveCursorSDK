using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
namespace Threeyes.Action
{
    public class ActionRuntimeData
    {
        public static ActionRuntimeData None = new ActionRuntimeData() { actionState = ActionState.None };

        //ToUpdate：标识除了GameObject还加上可选id，这样就算作用于同一个物体也能区分（可以通过Enter等方法传入）

        public ActionState actionState;
        public GameObject target;
        public object value;// The Input value to modify endValue (eg: whellscroll)
        public UnityAction actOnComplete;
        public List<IActionModifier> listModifier = new List<IActionModifier>();
        public string id = "";//Identify the target (ToUse)

        public void Init(ActionState actionState, GameObject target, object value, UnityAction actOnComplete, List<IActionModifier> listModifier, string id)
        {
            this.actionState = actionState;
            this.target = target;
            this.value = value;
            this.actOnComplete = actOnComplete;
            if (listModifier != null)
                this.listModifier = listModifier;
            this.id = id;
        }

        /// <summary>
        /// Get Action progress on specify ActionState
        /// 获取进度
        /// </summary>
        /// <param name="actionState"></param>
        /// <returns></returns>
        public virtual StateProgress GetProgress()
        {
            //Default is complete after the Enter/Exit get invoked
            StateProgress progress = StateProgress.Complete;
            return progress;
        }
    }

    /// <summary>
    /// Action Runtime Data (simulate to Playable.FrameData)
    /// 运行时数据，与作用物体绑定
    /// </summary>
    /// <typeparam name="TActionConfig"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class ActionRuntimeData<TActionConfig, TValue> : ActionRuntimeData
        where TActionConfig : ActionConfigBase<TValue>
    {
        public TActionConfig config;

        protected IEnumerable<T> GetListModifier<T>()//筛选符合类型的Modifier
                  where T : class
        {
            return from item in listModifier
                   where item is T
                   select item as T;
        }

        /// <summary>
        /// Return modified endValue
        /// </summary>
        public TValue EndValue
        {
            get
            {
                TValue endValue = config.EndValue;

                //#1: Use income value to scale the endValue
                if (value != null)
                {
                    endValue = config.ScaleEndValue(endValue, value);
                }

                //#2: Use IActionModifier_EndScale to scale the endValue
                foreach (var actionModifier in GetListModifier<IActionModifier_Common>())
                {
                    endValue = config.ScaleEndValue(endValue, actionModifier.GetObjEndValueScale(actionState));
                }
                return endValue;
            }
        }
    }

    /// <summary>
    /// Action Runtime Data (simulate to Playable.FrameData), along with receiver
    /// 运行时数据，与作用物体绑定
    /// </summary>
    /// <typeparam name="TActionConfig"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TActionReceiver">Receive Action</typeparam>
    public class ActionRuntimeData<TActionConfig, TValue, TActionReceiver> : ActionRuntimeData<TActionConfig, TValue>
        where TActionConfig : ActionConfigBase<TValue>
    {
        /// <summary>
        /// Get receiver from target
        /// </summary>
        public virtual TActionReceiver Receiver { get { return target ? target.GetComponent<TActionReceiver>() : default(TActionReceiver); } }
    }
}