using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using System.Threading.Tasks;
#if USE_JsonDotNet
using Newtonsoft.Json;
#endif

namespace Threeyes.Action
{
    /// <summary>
    /// 所有行为的基类
    /// Basic class for every Action
    /// 
    /// PS:该类已经剔除所有target为null的调用，子类的Enter/Exit不需要再次判断
    /// </summary>
    [JsonObject(MemberSerialization.Fields)]//PS:h会影响子类
    public abstract class SOActionBase : SOForSceneBase, ISaveLoadConfig
    {
        public bool IsActiveEnter { get { return isActiveEnter; } set { isActiveEnter = value; } }
        public bool IsActiveExit { get { return isActiveExit; } set { isActiveExit = value; } }

        [SerializeField] protected bool isActiveEnter = true;
        [SerializeField] protected bool isActiveExit = true;

        public async Task<ActionRuntimeData> EnterAsync(bool isEnter, GameObject target, object value = null, UnityAction actOnComplete = null, List<IActionModifier> listModifier = null, string id = "")
        {
            //Make sure the target exists before inner Enter/Exit function work
            if (!target)
            {
                Debug.LogWarning("Require target to work!");
                return null;
            }
            return await EnterAsync(isEnter ? ActionState.Enter : ActionState.Exit, target, value, actOnComplete, listModifier, id);
        }
        /// <summary>
        /// Enter/Exit action
        /// PS:需要统一调用这个方法
        /// </summary>
        public ActionRuntimeData Enter(bool isEnter, GameObject target, object value = null, UnityAction actOnComplete = null, List<IActionModifier> listModifier = null, string id = "")
        {
            //Make sure the target exists before inner Enter/Exit function work
            if (!target)
            {
                Debug.LogWarning("Require target to work!");
                return ActionRuntimeData.None;
            }
            return Enter(isEnter ? ActionState.Enter : ActionState.Exit, target, value, actOnComplete, listModifier, id);
        }

        protected virtual async Task<ActionRuntimeData> EnterAsync(ActionState actionState, GameObject target, object value = null, UnityAction actOnComplete = null, List<IActionModifier> listModifier = null, string id = "")
        {
            var runtimeData = Enter(actionState, target, value, actOnComplete, listModifier, id);

            while (true)
            {
                await Task.Yield();

                //1.判断是否还在指定状态，如果不是就退出
                ActionRuntimeData actionFrameData = GetRuntimeData(target);
                if (actionFrameData != null)
                {
                    if (actionFrameData.actionState != actionState)//Has switch to other state
                        return ActionRuntimeData.None;
                    var progress = actionFrameData.GetProgress();
                    if (progress == StateProgress.Complete)//完成
                        return runtimeData;
                }
                else//the target not save on this action, or the target has been destroyed
                {
                    return ActionRuntimeData.None;
                }
            }
        }

        /// <summary>
        /// 需要统一从这个位置进入
        /// </summary>
        /// <param name="isEnter"></param>
        /// <param name="target"></param>
        /// <param name="value">modify value</param>
        /// <param name="actOnComplete">完成之后的回调</param>
        /// 
        protected virtual ActionRuntimeData Enter(ActionState actionState, GameObject target, object value = null, UnityAction actOnComplete = null, List<IActionModifier> listModifier = null, string id = "")
        {
            ActionRuntimeData runtimeData = CreateRuntimeData(actionState, target, value, actOnComplete, listModifier, id);
            CacheRuntimeData(runtimeData);

            if (actionState == ActionState.Enter)
            {
                if (IsActiveEnter)
                    EnterFunc(runtimeData);
            }
            else
            {
                if (IsActiveExit)
                    ExitFunc(runtimeData);
            }

            return runtimeData;
        }

        protected virtual ActionRuntimeData CreateRuntimeData(ActionState actionState, GameObject target, object value, UnityAction actOnComplete, List<IActionModifier> listModifier, string id)
        {
            ActionRuntimeData actionRuntimeData = new ActionRuntimeData();
            actionRuntimeData.Init(actionState, target, value, actOnComplete, listModifier, id);
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
        public virtual bool IsComplete(GameObject target, ActionState actionState, string id = "")
        {
            return GetProgress(target, actionState, id) == StateProgress.Complete;
        }
        StateProgress GetProgress(GameObject target, ActionState actionState, string id = "")
        {
            //PS:可通过StateProgress>Begin判断是否正在运行或已经运行完成
            StateProgress stateProgress = StateProgress.None;
            ActionRuntimeData actionRuntimeData = GetRuntimeData(target);
            if (actionRuntimeData != null)
            {
                if (actionRuntimeData.actionState == actionState)
                {
                    stateProgress = actionRuntimeData.GetProgress();
                }
                else
                {
                    //T actionState not match
                }
            }
            else
            {
                //The target not saved in this action
            }
            return stateProgress;
        }

        #endregion

        #region Caching RuntimeData

        //ToUpdate: Key改为 包含(GameOjbect,id)的数据类

        /// <summary>
        /// 缓存运行的数据
        /// 注意：
        /// 1.针对同一GameObject，只能存储其1个ActionState的数据（也就是Enter/Exit互斥，保证只有一个状态的数据存在）
        /// 2.完成后数据会缓存在字典中，便于回溯其StateProgress。直到下一次调用才会被覆盖
        /// </summary>
        [JsonIgnore] protected Dictionary<GameObject, ActionRuntimeData> dicCacheRuntimeData = new Dictionary<GameObject, ActionRuntimeData>();
        static int minCheckNullDicCount = 50;

        protected virtual void CacheRuntimeData(ActionRuntimeData actionFrameData)
        {
            GameObject target = actionFrameData.target;

            ///清除Key为null的键值对
            ///原因:物体被清空导致Key为null，但是不影响执行。
            ///难点：
            /// 1.使用Dictionary.Remove(null)会报错
            /// 2.Linq.Count会消耗性能
            ///解决办法：如果数量到达一定程度，或者是调用次数超过阈值，就克隆一份新的带非空key的Dictionary
            if (dicCacheRuntimeData.Count > minCheckNullDicCount)
            {
                Dictionary<GameObject, ActionRuntimeData> tempD = new Dictionary<GameObject, ActionRuntimeData>();
                foreach (var kP in dicCacheRuntimeData)
                {
                    if (kP.Key == null)//排除Null Key
                        continue;
                    if (!tempD.ContainsKey(kP.Key))
                        tempD[kP.Key] = kP.Value;
                }
                dicCacheRuntimeData = tempD;
                Debug.Log("Clear Null KeyPair");
                minCheckNullDicCount += 50;//Increase next check number
            }

            if (target)//创建新Data
            {
                //if (!dicCacheRuntimeData.ContainsKey(target))
                dicCacheRuntimeData[target] = actionFrameData;//保存或更新映射
            }
        }

        protected virtual ActionRuntimeData GetRuntimeData(GameObject target, string id = "")
        {
            if (dicCacheRuntimeData.ContainsKey(target))
                return dicCacheRuntimeData[target];
            return null;
        }

        protected virtual void RemoveRuntimeData(GameObject target, string id = "")
        {
            //删除多余Tween，需要传入GameObject (PS:因为要缓存并随时获取其状态，因此先不调用该方法）
            if (target != null && dicCacheRuntimeData.ContainsKey(target))
            {
                dicCacheRuntimeData.Remove(target);
            }
        }

        #endregion

        #region Config
        /// <summary>
        /// The short name of the file type
        /// </summary>
        public virtual void SaveConfig() { }
        public virtual void LoadConfig() { }

        #endregion

        #region Editor Utility

        #region Menu
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/Action/" + "Save Selected Config")]
        public static void SaveSelectedConfig()
        {
            foreach (UnityEngine.Object obj in UnityEditor.Selection.objects)
            {
                SOActionBase sOActionBase = obj as SOActionBase;
                if (sOActionBase)
                {
                    sOActionBase.SaveConfig();
                }
            }
        }

        [UnityEditor.MenuItem("Assets/Action/" + "Load Selected Config")]
        public static void LoadSelectedConfig()
        {
            foreach (UnityEngine.Object obj in UnityEditor.Selection.objects)
            {
                SOActionBase sOActionBase = obj as SOActionBase;
                if (sOActionBase)
                {
                    sOActionBase.LoadConfig();
                }
            }
        }

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
}
