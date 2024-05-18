using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Threeyes.Core;

namespace Threeyes.Action
{
    //以下是待办事项
    /// <summary>
    /// 
    /// </summary>
    public class ActionRuntimeData
    {
        /// <summary>
        /// Get Action progress on specify ActionState
        /// 获取进度
        /// </summary>
        /// <param name="actionState"></param>
        /// <returns></returns>
        public virtual StateProgress Progress { get { return stateProgress; } internal set { stateProgress = value; } }

        /// <summary>
        /// Check if Action target is valid
        /// </summary>
        public bool IsValid { get { return baseReceiver != null; } }

        public ActionState actionState;
        public object baseReceiver;//target to receive action
        public ObjectID objectID;
        public object value;//[Optional] The Input value to modify endValue (eg: whellscroll)
        public UnityAction actOnComplete;

        //#Runtime
        protected StateProgress stateProgress = StateProgress.None;
        internal Task task;//Related task0
        internal CancellationTokenSource cancellationTokenSource;

        public virtual void Init(ActionState actionState, object baseReceiver, ObjectID objectID, object value, UnityAction actOnComplete)
        {
            this.actionState = actionState;
            this.baseReceiver = baseReceiver;
            this.value = value;
            this.actOnComplete = actOnComplete;
            this.objectID = objectID;
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
        /// <summary>
        /// The clone of the origin config, you can safely change it's fields
        /// </summary>
        public TActionConfig Config { get { return config; } set { config = value; } }
        [SerializeField] TActionConfig config;
    }

    /// <summary>
    /// Action Runtime Data (simulate to Playable.FrameData), along with receiver
    /// 运行时数据，与作用物体绑定
    /// </summary>
    /// <typeparam name="TActionConfig"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TActionReceiver">Target to Receive Action</typeparam>
    public class ActionRuntimeData<TActionConfig, TValue, TActionReceiver> : ActionRuntimeData<TActionConfig, TValue>
        where TActionConfig : ActionConfigBase<TValue>
    {
        /// <summary>
        /// Get receiver from target
        /// </summary>
        public virtual TActionReceiver Receiver { get { return receiver; } }
        public TActionReceiver receiver = default(TActionReceiver);

        public override void Init(ActionState actionState, object baseReceiver, ObjectID objectID, object value, UnityAction actOnComplete)
        {
            //Init Receiver
            if (baseReceiver is TActionReceiver receiverInst)
                receiver = receiverInst;
            else
            {
                Debug.LogError($"{baseReceiver} does not inherit class [{typeof(TActionReceiver)}]!");
            }

            base.Init(actionState, baseReceiver, objectID, value, actOnComplete);//因为其会调用ApplyModifiers，所以放最后
        }
    }
}