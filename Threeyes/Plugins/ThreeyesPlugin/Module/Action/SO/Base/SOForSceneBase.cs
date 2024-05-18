using UnityEngine;
using System.Text;
using System;

#if USE_NaughtyAttributes
using NaughtyAttributes;
#endif

namespace Threeyes.Action
{
    /// <summary>
    /// SO that interactive with scene object (eg: SOAction)
    /// 
    /// Warning：
    /// -该库不应放在Core中，否则Core需要引用NaughtAttributes
    /// </summary>
    public abstract class SOForSceneBase : ScriptableObject
    {
#if USE_NaughtyAttributes
        [ResizableTextArea]
#endif
        public string remark;//(PS:Remark 仅用于SO实例，Component子类就单独增加Remark组件）

        public static void OnManualDestroy(SOForSceneBase so, ObjectID objectID)
        {
            so.OnManualDestroy(objectID);
        }
        /// <summary>
        /// 手动调用的Destroy，保证指定物体的关联实例能销毁（如Tween），避免切换场景后相关进程仍在执行
        /// </summary>
        /// <param name="target"></param>
        protected virtual void OnManualDestroy(ObjectID objectID) { }

        #region Editor Method
#if UNITY_EDITOR

        /// <summary>
        /// Inspector Common Tips
        /// </summary>
        /// <param name="sB"></param>
        public virtual void SetInspectorGUICommonTextArea(StringBuilder sB, GameObject target, string id = "") { }

#endif
        #endregion
    }

    public static class SOForSceneExtension
    {
        /// <summary>
        /// Call Destroy if the param if not null
        /// </summary>
        /// <param name="so"></param>
        /// <param name="target"></param>
        public static void OnManualDestroy(this SOForSceneBase so, ObjectID objectID)
        {
            if (so)
                SOForSceneBase.OnManualDestroy(so, objectID);
        }
    }


    /// <summary>
    /// Unique ID for any object
    /// 
    /// 用途：
    /// -标记唯一的实例，便于追踪对比
    /// </summary>
    [Serializable]
    public struct ObjectID : IEquatable<ObjectID>
    {
        public static ObjectID Empty { get { return new ObjectID(null, ""); } }

        public bool IsValid { get { return objId != 0; } }//只需要确保objId即可

        int objId;//Unique id for object
        string id;//[Optional]Extra custom id

        public ObjectID(object obj, string id = "")
        {
            ///PS:
            ///-如果obj是UnityEngine.Object，则其GetHashCode返回的值与GetInstanceID相同
            objId = obj != null ? obj.GetHashCode() : 0;
            this.id = id;
        }

        public override string ToString()
        {
            string result = objId.ToString();
            if (id != "")
                result += "_" + id;
            return result;
        }

        #region IEquatable
        public override bool Equals(object obj)
        {
            if (obj is ObjectID other)
            {
                return Equals(other);
            }
            return false;
        }
        public override int GetHashCode()//用于Dictionary.ContainsKey的内部比较
        {
            int result = objId;
            //base.GetHashCode();
            //if (objId != "")
            //    result += objId.GetHashCode();
            if (id != "")
                result += id.GetHashCode();
            return result;
        }
        public bool Equals(ObjectID other)
        {
            return Equals(objId, other.objId) && Equals(id, other.id);
        }

        public static bool operator ==(ObjectID a, ObjectID b)
        {
            return a.Equals(b) == true;
        }
        public static bool operator !=(ObjectID a, ObjectID b)
        {
            return a.Equals(b) == false;
        }
        #endregion
    }
}