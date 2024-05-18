using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using Threeyes.Core.Editor;
#endif

namespace Threeyes.Action
{
    using Threeyes.EventPlayer;
    /// <summary>
    /// PS:
    /// -因为Action由太多功能，所以仅提供常见方法，用户有需要可以自行是实现
    /// </summary>
    [AddComponentMenu(EditorDefinition_Action.AssetMenuPrefix_Action_EventPlayer + "Sequence_EventPlayer_SOAction")]
    public class Sequence_EventPlayer_SOAction :
        Sequence_EventPlayerBase<EventPlayer_SOAction>
    {
        /// <summary>
        /// Set all one by one
        /// </summary>
        /// <param name="isEnter"></param>
        public async void SetAllByOrderAsync()
        {
            await SetAllByOrderAsyncFunc(true);
        }
        public async void ResetAllByOrderAsync()
        {
            await SetAllByOrderAsyncFunc(false);
        }
        /// <summary>
        /// Set all at the same time, and wait until all completed
        /// 
        /// PS:
        /// -以下方法不需要在EPS同步显示当前正在调用的ID，因为是所有ID一起执行，没有先后顺序
        /// </summary>
        /// <param name="isEnter"></param>
        public async void SetAllSynchronouslyAsync()
        {
            await SetAllSynchronouslyAsyncFunc(true);
        }
        public async void ResetAllSynchronouslyAsync()
        {
            await SetAllSynchronouslyAsyncFunc(false);
        }

        protected virtual async Task SetAllByOrderAsyncFunc(bool isSetOrReset)
        {
            //——实现2：逐个调用，回调执行下个方法，能通知序号等事件更新——
            var listData = ListData;//先缓存，避免中途被销毁导致报错
            for (int i = 0; i != listData.Count; i++)
            {
                if (!this)//该物体被销毁
                    return;
                EventPlayer_SOAction targetEP = listData[i];
                if (!targetEP)//该目标无效或被销毁：跳过
                    continue;

                //Set target index
                playOnSetData = false;//禁止调用Set时同时调用targetEP.Play
                Set(i);//通知序号或事件等进行更新。（Warning:如果Reset Type不为Null，且子EP面对多个同一目标，可能会导致动画被异常停用！）
                await targetEP.PlayAsync(isSetOrReset);//主动调用

                //Reset
                playOnSetData = true;
            }
            //Debug.Log("SetAllByOrderAsync complete");
        }

        protected virtual async Task SetAllSynchronouslyAsyncFunc(bool isSetOrReset)
        {
            //Debug.Log("SetAllSynchronouslyAsync begin");
            var listData = ListData;//先缓存，避免中途被销毁导致报错
            List<Task> listTask = new List<Task>();
            foreach (EventPlayer_SOAction ep in listData)
            {
                listTask.Add(ep.PlayAsync(isSetOrReset));
            }
            await Task.WhenAll(listTask.ToArray());

            //Debug.Log("SetAllSynchronouslyAsync complete");
            //PS：用户可通过继承或调用实现回调
        }

        #region Override
        bool playOnSetData = true;
        bool stopOnResetData = true;
        protected override void SetData_CustomFunc(EventPlayer_SOAction data, int index)
        {
            if (playOnSetData)
                base.SetData_CustomFunc(data, index);
        }
        protected override void ResetData_CustomFunc(EventPlayer_SOAction data, int index)
        {
            if (stopOnResetData)
                base.ResetData_CustomFunc(data, index);
        }
        #endregion

        #region Editor Method
#if UNITY_EDITOR
        //——MenuItem——
        protected const int intActionMenuOrder = 1000;
        static string instGroupName = "ActionEPS ";
        [MenuItem(EventPlayer.strMenuItem_Root_Extend + "Action/EventPlayerSequence", false, intActionMenuOrder + 1)]
        public static void CreateEventPlayerSequence()
        {
            EditorTool.CreateGameObject<Sequence_EventPlayer_SOAction>(instGroupName);
        }
        [MenuItem(EventPlayer.strMenuItem_Root_Extend + "Action/EventPlayerSequence Child", false, intActionMenuOrder + 2)]
        public static void CreateEventPlayerSequenceChild()
        {
            EditorTool.CreateGameObjectAsChild<Sequence_EventPlayer_SOAction>(instGroupName);
        }

        //——Hierarchy GUI——
        public override string ShortTypeName { get { return "ActEPS"; } }

        public override void SetHierarchyGUIProperty(StringBuilder sB)
        {
            base.SetHierarchyGUIProperty(sB);
        }

#endif
        #endregion
    }
}
