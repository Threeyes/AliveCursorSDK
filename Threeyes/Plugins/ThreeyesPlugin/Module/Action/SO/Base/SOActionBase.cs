using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using System.Threading.Tasks;
using Threeyes.Core;
using System.Threading;
#if USE_JsonDotNet
using Newtonsoft.Json;
#endif

namespace Threeyes.Action
{
    /// <summary>
    /// Basic class for Action
    /// 所有行为的基类
    /// 
    /// PS:
    /// - 该类已经剔除所有target为null的调用，子类的Enter/Exit不需要再次判断
    /// 
    /// ToUpdate：
    /// -listModifier整合成一个ModifierSettings类，参考Json
    /// -增加类似Tween的扩展方法，来设置可选参数（可以通过一个Action.Create来创建RuntimeData，然后可以设置是否自动启动）
    /// </summary>
#if USE_JsonDotNet
    [JsonObject(MemberSerialization.Fields)]//PS:会影响子类
#endif
    public abstract class SOActionBase : SOForSceneBase
    {
        public bool IsActiveEnter { get { return isActiveEnter; } set { isActiveEnter = value; } }
        public bool IsActiveExit { get { return isActiveExit; } set { isActiveExit = value; } }

        [SerializeField] protected bool isActiveEnter = true;
        [SerializeField] protected bool isActiveExit = true;

        public abstract Type ReceiverType { get; }

        //——Unity物体相关方法 (ToUpdate:以下与Unity相关的方法，可以挪到一个Manager，或Helper方法中，从而减少耦合)——
        /// <summary>
        /// Enter/Exit action
        /// </summary>
        public async void Enter(bool isEnter, GameObject target, object value = null, UnityAction actOnComplete = null, ActionModifierSettings modifierSettings = null, string id = "")
        {
            await EnterAsync(isEnter, target, value, actOnComplete, modifierSettings, id);
        }
        public async Task<ActionRuntimeData> EnterAsync(bool isEnter, GameObject target, object value = null, UnityAction actOnComplete = null, ActionModifierSettings modifierSettings = null, string id = "")
        {
            if (!target) //Make sure the target exists before inner Enter/Exit function work
            {
                Debug.LogError("target is null!");
                return null;
            }
            object receiver;
            ObjectID objectID = GetObjectID(target, out receiver, id);//尝试Receiver的唯一ID
            if (objectID == ObjectID.Empty)
                return null;

            return await EnterAsync(isEnter ? ActionState.Enter : ActionState.Exit, receiver, objectID, value, actOnComplete, modifierSettings);
        }

        /// <summary>
        /// Get ObjectID by target and ReceiverType
        /// </summary>
        /// <param name="target"></param>
        /// <param name="id"></param>
        /// <returns>return Empty if target doesn't have recevier</returns>
        public ObjectID GetObjectID(GameObject target, out object receiver, string id = "")
        {
            receiver = GetReceiver(target);
            if (receiver == null)//Check for receiver
            {
                Debug.LogError($"{target} doesn't have recevier [{ReceiverType}]!");
                return ObjectID.Empty;
            }
            return new ObjectID(receiver, id);//保存Receiver的唯一ID
        }
        public object GetReceiver(GameObject target)
        {
            if (!target) return null;
            return target.GetComponent(ReceiverType);//支持接口/组件
        }

        //——具体实现方法——
        public virtual async void Enter(ActionState actionState, object receiverBase, ObjectID objectID, object value = null, UnityAction actOnComplete = null, ActionModifierSettings modifierSettings = null)
        {
            await EnterAsync(actionState, receiverBase, objectID, value, actOnComplete, modifierSettings);
        }
        public virtual async Task<ActionRuntimeData> EnterAsync(ActionState actionState, object receiverBase, ObjectID objectID, object value = null, UnityAction actOnComplete = null, ActionModifierSettings modifierSettings = null)
        {
            TryStopRunningProgress(objectID);//先清除相同目标且运行中的状态

            ActionRuntimeData runtimeData = CreateRuntimeData(actionState, receiverBase, objectID, value, actOnComplete, modifierSettings);
            CacheRuntimeData(objectID, runtimeData);//保存其状态

            try
            {
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                Task<ActionRuntimeData> task = EnterTaskAsync(runtimeData).WithCancellation(cancellationTokenSource);
                runtimeData.task = task;//Save task
                runtimeData.cancellationTokenSource = cancellationTokenSource;
                return await task;
            }
            catch (TaskCanceledException)//Ignore
            {
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            return null;
        }

        /// <summary>
        /// 执行任务并更新进度
        /// </summary>
        /// <param name="runtimeData"></param>
        /// <returns></returns>
        async Task<ActionRuntimeData> EnterTaskAsync(ActionRuntimeData runtimeData)
        {
            //#1 尝试启动并运行，此时Progress为Begin（之后每一轮询都会等待一帧）
            bool hasRun = false;
            ActionState actionState = runtimeData.actionState;
            if (actionState == ActionState.Enter)
            {
                if (IsActiveEnter)
                {
                    runtimeData.Progress = StateProgress.Begin;//需要优先设置Progress，方便初始化判断
                    EnterFunc(runtimeData);
                    hasRun = true;
                }
            }
            else
            {
                if (IsActiveExit)
                {
                    runtimeData.Progress = StateProgress.Begin;
                    ExitFunc(runtimeData);
                    hasRun = true;
                }
            }

            if (!hasRun)//没有被调用：返回
                return null;

            while (true)
            {
                await Task.Yield();

                //判断是否还在指定状态，如果不是就退出
                if (runtimeData != null)
                {
                    if (!runtimeData.IsValid)//场景切换等导致目标被销毁:直接跳出
                        return null;

                    if (runtimeData.Progress == StateProgress.Complete)//完成
                    {
                        runtimeData.actOnComplete.TryExecute();//统一结束后调用，方便Animator等无完成事件的实现（因为回调时可能物体被销毁，因此要Try）
                        return runtimeData;
                    }
                }
                else//the target not save on this action, or the target has been destroyed：跳出
                {
                    return null;
                }
            }
        }

        protected virtual ActionRuntimeData CreateRuntimeData(ActionState actionState, object receiver, ObjectID objectID, object value, UnityAction actOnComplete, ActionModifierSettings modifierSettings)
        {
            ActionRuntimeData actionRuntimeData = new ActionRuntimeData();
            actionRuntimeData.Init(actionState, receiver, objectID, value, actOnComplete);
            return actionRuntimeData;
        }

        protected abstract void EnterFunc(ActionRuntimeData runtimeData);
        protected abstract void ExitFunc(ActionRuntimeData runtimeData);

        #region State
        /// <summary>
        /// Is the state complete?
        /// 状态是否完成,
        /// </summary>
        /// <param name="target"></param>
        /// <param name="actionState"></param>
        /// <returns></returns>
        public virtual bool IsComplete(ObjectID objectID, ActionState actionState)
        {
            return GetProgress(objectID, actionState) == StateProgress.Complete;
        }
        StateProgress GetProgress(ObjectID objectID, ActionState actionState)
        {
            //PS:可通过StateProgress>Begin判断是否正在运行或已经运行完成
            StateProgress stateProgress = StateProgress.None;
            ActionRuntimeData actionRuntimeData = GetRuntimeData(objectID);
            if (actionRuntimeData != null)
            {
                if (actionRuntimeData.actionState == actionState)
                {
                    stateProgress = actionRuntimeData.Progress;
                }
                else//actionState not match
                {

                }
            }
            else
            {
                //The target not saved in this action
            }
            return stateProgress;
        }
        #endregion

        #region Caching RuntimeData (确保统一ID，只能执行同一SO的Enter或Exit)
        /// <summary>
        /// 缓存运行的数据
        /// 注意：
        /// 1.针对同一GameObject，只能存储其1个ActionState的数据（也就是Enter/Exit互斥，保证只有一个状态的数据存在）
        /// 2.完成后数据会缓存在字典中，便于回溯其StateProgress。直到下一次调用才会被覆盖
        /// </summary>
#if USE_JsonDotNet
        [JsonIgnore]
#endif
        protected Dictionary<ObjectID, ActionRuntimeData> dicCacheRuntimeData = new Dictionary<ObjectID, ActionRuntimeData>();
        static int minCheckNullDicCount = 50;//需要检测Dic中空元素的阈值

        protected virtual void CacheRuntimeData(ObjectID objectID, ActionRuntimeData actionFrameData)
        {
            ///清除Key为null的键值对.如果数量到达一定程度，或者是调用次数超过阈值，就克隆一份新的带非空key的Dictionary
            ///如下实现的原因：
            /// 1.使用Dictionary.Remove(null)会报错
            /// 2.Linq.Count会消耗性能
            /// 
            /// ToUpdate：
            /// -可以把判断条件改为检测Receiver是否为空，即kP.Value.IsValid
            /// 
            ///Todo:
            ///-销毁无效的ActionRuntimeData（非必须，C#会回收无引用类实例）
            if (dicCacheRuntimeData.Count > minCheckNullDicCount)
            {
                Dictionary<ObjectID, ActionRuntimeData> tempD = new Dictionary<ObjectID, ActionRuntimeData>();
                foreach (var kP in dicCacheRuntimeData)
                {
                    if (!kP.Value.IsValid)//排除无效物体
                        continue;

                    if (!tempD.ContainsKey(kP.Key))
                        tempD[kP.Key] = kP.Value;
                }
                dicCacheRuntimeData = tempD;
                Debug.Log("Clear Null KeyPair");
                minCheckNullDicCount += 50;//Increase next check capacity
            }

            if (objectID.IsValid)
            {
                dicCacheRuntimeData[objectID] = actionFrameData;//保存或更新(覆盖）映射，方便回溯
            }
        }

        /// <summary>
        /// 尝试暂停相同标识且正在运行的进程，避免重复调用导致冲突
        /// </summary>
        /// <param name="runtimeData"></param>
        protected virtual void TryStopRunningProgress(ObjectID objectID)
        {
            ActionRuntimeData runtimeData = GetRuntimeData(objectID);
            if (runtimeData == null)
                return;
            if (runtimeData.task != null && !runtimeData.task.IsCompleted)//有Task执行中：尝试停止该Task
                runtimeData.cancellationTokenSource.TryCancelAndDispose();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="id"></param>
        /// <returns>如果无效，则返回空</returns>
        protected virtual ActionRuntimeData GetRuntimeData(ObjectID objectID)
        {
            if (dicCacheRuntimeData.ContainsKey(objectID))
                return dicCacheRuntimeData[objectID];
            return null;
        }

        /// <summary>
        /// 删除多余Tween，需要传入GameObject 
        /// PS:
        /// -因为要缓存并随时获取其状态，因此暂不调用该方法
        /// </summary>
        /// <param name="target"></param>
        /// <param name="id"></param>
        protected virtual void RemoveRuntimeData(ObjectID objectID)
        {
            if (dicCacheRuntimeData.ContainsKey(objectID))
                dicCacheRuntimeData.Remove(objectID);
        }
        #endregion

        #region Editor Utility

        #region Menu
#if UNITY_EDITOR
        /// <summary>
        /// 在Project中选择对应实例后调用，常用于需要批量转移参数的值
        /// </summary>
        [UnityEditor.MenuItem("Assets/Action/" + "Transfer Data")]
        public static void TransferData()
        {
            foreach (UnityEngine.Object obj in UnityEditor.Selection.objects)
            {
                SOActionBase sOActionBase = obj as SOActionBase;
                if (sOActionBase)
                {
                    sOActionBase.TransferFunc();
                }
            }
        }
        protected virtual void TransferFunc()
        {

        }
#endif
        #endregion
        #endregion
    }

    public class ActionModifierSettings
    {
        public List<IActionModifier> listModifier = new List<IActionModifier>();//（提供默认值，确保外部获取不为null）
        public Action<IActionConfig, ActionState, object> CustomModifyConfig = null;//Customized way to modify Configuration using Modifiers(ToUpdate:可以选择是覆盖还是添加，或者公开ActionConfigBase的相关方法以便调用)

        public ActionModifierSettings() { }

        public ActionModifierSettings(List<IActionModifier> listModifier, Action<IActionConfig, ActionState, object> modifyConfig = null)
        {
            if (listModifier != null)//避免为空导致获取报错
                this.listModifier = listModifier;
            CustomModifyConfig = modifyConfig;
        }
    }

    #region Define
    /// <summary>
    /// The state of action
    /// Action的状态
    /// </summary>
    [Flags]
    public enum ActionState
    {
        None = 0,

        Enter = 1 << 0,
        Exit = 1 << 1,

        All = ~0
    }

    /// <summary>
    /// Whether the ActionState Begin/Complete 
    /// 状态的进度
    /// </summary>
    public enum StateProgress
    {
        None = 0,
        Begin = 1,
        Processing = 2,
        Complete = 3,
    }


    [System.Serializable]
    public class SOActionEvent : UnityEvent<SOActionBase>
    {
    }

    #endregion
}
