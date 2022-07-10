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
                    //    DontDestroyOnLoad(_Instance.gameObject);//ps:һ������Ӧ��ֻ����һ�������ڹ���ǰ�����Ķ�ӦPD����������ͣ��
                }
                return _Instance;
            }
        }
        static PersistentControllerManagerGroup _Instance;
        #endregion

        public List<PersistentControllerManagerBase> listPCM = new List<PersistentControllerManagerBase>();//��������ʱ����������PCM (PS:PersistentControllerManagerBase��������Զ����뵽���List�У�����ͳһ����)

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
        /// ע��:SO�����Ϳ���������ʱ�Ż�����DefaultValue����˽��������к���ø÷���
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

        //�������ע����listPCM��Ԫ�أ���������ʱ���ɵģ�PS�����������࣬����Ϊ��ЩPCM��prefab��Ҫ����ʱ���ɣ����߲��ܷŵ���������£�����ͳһͨ������ע�ᣩ
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