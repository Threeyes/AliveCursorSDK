using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Threeyes.Persistent
{
    /// <summary>
    /// Manage all PersistentControllerManager
    /// </summary>
    [RequireComponent(typeof(InstanceManagerGroup))]
    public class PersistentControllerManagerGroup : ComponentGroupBase<PersistentControllerManagerBase>
    {
        #region Singleton
        public static PersistentControllerManagerGroup Instance
        {
            get
            {
                if (!_Instance)
                {
                    _Instance = GameObject.FindObjectOfType<PersistentControllerManagerGroup>();
                    if (!_Instance)
                    {
                        GameObject newGo = new GameObject(nameof(PersistentControllerManagerGroup).ToString(), typeof(PersistentControllerManagerGroup));
                        _Instance = newGo.GetComponent<PersistentControllerManagerGroup>();
                    }
                    //if (Application.isPlaying)
                    //    DontDestroyOnLoad(_Instance.gameObject);//ps:一个场景应该只存在一个，用于管理当前场景的对应PD，不能永久停留
                }
                return _Instance;
            }
        }
        static PersistentControllerManagerGroup _Instance;
        #endregion

        public List<PersistentControllerManagerBase> listPCM = new List<PersistentControllerManagerBase>();//管理运行时创建的所有PCM (PS:PersistentControllerManagerBase创建后会自动加入到这个List中，便于统一管理)

        public void Add(PersistentControllerManagerBase persistentControllerManagerBase)
        {
            listPCM.AddOnce(persistentControllerManagerBase);
        }
        public void Remove(PersistentControllerManagerBase persistentControllerManagerBase)
        {
            listPCM.Remove(persistentControllerManagerBase);
        }

        [ContextMenu("LoadAllValue")]
        public void LoadAllValue()
        {
            ForEachStaticChildComponent<PersistentControllerManagerBase>((c) => c.LoadAllValue());
        }

        /// <summary>
        /// 注意:SO等类型可能在运行时才会设置DefaultValue，因此建议在运行后调用该方法
        /// </summary>
        [ContextMenu("SaveDefaultValue")]
        public void SaveDefaultValue()
        {
            ForEachStaticChildComponent<PersistentControllerManagerBase>((c) => c.SaveDefaultValue());

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

        [ContextMenu("SaveAllValue")]
        public void SaveAllValue()
        {
            ForEachStaticChildComponent<PersistentControllerManagerBase>((c) => c.SaveAllValue());
        }


        [ContextMenu("DeleteAllKey")]
        public void DeleteAllKey()
        {
            ForEachStaticChildComponent<PersistentControllerManagerBase>((c) => c.DeleteAllKey());
        }

        //针对所有注册在listPCM的元素，包括运行时生成的（PS：不查找子类，是因为有些PCM是prefab需要运行时生成，或者不能放到这个物体下，所以统一通过代码注册）
        void ForEachStaticChildComponent<T2>(UnityAction<T2> func)
        {
            listPCM.ForEach((c) =>
            {
                if (c != null)
                    c.ForEachSelfComponent(func);
            });
        }
    }
}