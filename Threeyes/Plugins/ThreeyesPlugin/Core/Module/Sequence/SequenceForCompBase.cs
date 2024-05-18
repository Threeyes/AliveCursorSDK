using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Threeyes.Core
{
    /// <summary>
    /// Sequence For Component
    /// </summary>
    /// <typeparam name="TComp"></typeparam>
    public class SequenceForCompBase<TComp> : SequenceBase<TComp>
        where TComp : Component
    {
        #region Property & Field
        public override List<TComp> ListData
        {
            get
            {
                TryInitData();
                return listData;
            }
            set { base.ListData = value; }
        }
        protected bool IsLoadChildOnAwake { get { return isLoadChildOnAwake; } set { isLoadChildOnAwake = value; } }

        [SerializeField] protected bool isLoadChildOnAwake = true;//Auto load child's Comp on awake, Set to false if you want to custom list value

        #endregion

        #region Inner Method

        /// <summary>
        /// GetComponentFromChild
        /// </summary>
        /// <returns></returns>
        protected virtual List<TComp> GetComponentsFromChild()
        {
            List<TComp> tempList = new List<TComp>();
            //Only get the first layer by default, including hiding object
            foreach (Transform tfChild in transform)
            {
                var comp = tfChild.GetComponent<TComp>();
                if (comp)
                {
                    tempList.Add(comp);
                }
            }
            return tempList;
        }

        #endregion

        #region Unity Method
        protected virtual void Awake()
        {
            TryInitData();
        }

        bool hasInit = false;
        void TryInitData()
        {
            if (!IsLoadChildOnAwake)
                return;

            if (Application.isPlaying)
            {
                if (hasInit)//Avoid init multiTime on Play
                    return;
                hasInit = true;
            }

            listData = GetComponentsFromChild();
            UpdateState();
        }
        #endregion

        #region Editor Method
#if UNITY_EDITOR
        public override void SetHierarchyGUIProperty(StringBuilder sB)
        {
            TryInitData();//Manual Update Info(可能是这里出发了Scene的需要保存标识)（ToFix：可能会影响isLoadChildOnAwake的功能）
            base.SetHierarchyGUIProperty(sB);
        }
#endif
        #endregion
    }
}