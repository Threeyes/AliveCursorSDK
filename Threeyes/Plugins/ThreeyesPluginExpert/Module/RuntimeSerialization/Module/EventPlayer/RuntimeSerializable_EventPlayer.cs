using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
namespace Threeyes.RuntimeSerialization
{
    using Threeyes.EventPlayer;
    [DisallowMultipleComponent]
    public partial class RuntimeSerializable_EventPlayer : RuntimeSerializableMonoBehaviour<EventPlayer, EventPlayerPropertyBag>, IRuntimeSerializableOptionHolder<RuntimeSerializableOption_EventPlayer>
    {
        ///Todo：增加自定义配置：
        ///     -在反序列化后，根据参数调用一次Play方法
        public RuntimeSerializableOption_EventPlayer Option { get { return option; } }
        [SerializeField] protected RuntimeSerializableOption_EventPlayer option;

    }

    #region Define (放在外面方便继承)
    [System.Serializable]
    public class EventPlayerPropertyBag : MonoBehaviourPropertyBag<EventPlayer>
    {
        ///PS：
        ///-先保存基本状态，尽量在还原时不调用其方法，后期再扩充

        public bool isPlayed = false;// Cache the play state
        public bool isActive = true;
        public bool canPlay = true;
        public bool canStop = true;

        public bool isPlayOnAwake = false;
        public bool isPlayOnce = false;
        public bool isReverse = false;

        public bool isGroup = false;
        public bool isIncludeHide = true;
        public bool isRecursive = false;

        public string _id = "";//避免与父类的id重名
        public bool isInvokeByID = false;
        public TargetIDLocationType targetIDLocation = TargetIDLocationType.Global;
        public string targetId = "";

        public EventPlayer_State state = EventPlayer_State.Idle;

        public EventPlayerPropertyBag()
        {
        }

        public override void Init(EventPlayer container)
        {
            base.Init(container);
            isPlayed = container.IsPlayed;
            isActive = container.IsActive;
            canPlay = container.CanPlay;
            canStop = container.CanStop;

            isPlayOnAwake = container.IsPlayOnAwake;
            isPlayOnce = container.IsPlayOnce;
            isRecursive = container.IsReverse;

            isGroup = container.IsGroup;
            isIncludeHide = container.IsIncludeHide;
            isRecursive = container.IsRecursive;

            _id = container.ID;
            isInvokeByID = container.IsInvokeByID;
            targetIDLocation = container.TargetIDLocation;
            targetId = container.TargetID;

            state = container.State;
        }

        public override void Accept(ref EventPlayer container)
        {
            base.Accept(ref container);
            container.IsPlayed = isPlayed;
            container.IsActive = isActive;
            container.CanPlay = canPlay;
            container.CanStop = canStop;

            container.IsPlayOnAwake = isPlayOnAwake;
            container.IsPlayOnce = isPlayOnce;
            container.IsReverse = isRecursive;

            container.IsGroup = isGroup;
            container.IsIncludeHide = isIncludeHide;
            container.IsRecursive = isRecursive;

            container.ID = _id;
            container.IsInvokeByID = isInvokeByID;
            container.TargetIDLocation = targetIDLocation;
            container.TargetID = targetId;

            container.State = state;
        }
    }
    [System.Serializable]
    public class RuntimeSerializableOption_EventPlayer : RuntimeSerializableOption
    {
        public bool invokeEventOnDeserialized = true;//Call event using deserialized state&property on Deserialized completed（PS：可用在需要通过EP还原状态的实现上）

        [JsonConstructor]
        public RuntimeSerializableOption_EventPlayer()
        {
            invokeEventOnDeserialized = true;
        }
    }

    #endregion
}