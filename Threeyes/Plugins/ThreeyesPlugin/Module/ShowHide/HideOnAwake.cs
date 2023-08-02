using UnityEngine;

namespace Threeyes.ShowHide
{
    /// <summary>
    /// 程序运行时隐藏
    /// 适用于在运行时隐藏，在特定条件下才显示的物体
    /// 
    /// Todo:
    /// 1.通知子物体全部隐藏
    /// 2.
    /// </summary>
    public class HideOnAwake : ShowAndHide
    {
        [Header("Init Hide Setting")]
        public bool isHideOnAwake = true;
        protected override void Awake()
        {
            base.Awake();
            HideOnAwakeManager.Init(gameObject);
        }

        bool hasHideOnAwake = false;
        public virtual void TryHideOnAwake()
        {
            if (hasHideOnAwake)
                return;

            if (isHideOnAwake)
            {
                Hide();
                hasHideOnAwake = true;
            }
        }
    }
}