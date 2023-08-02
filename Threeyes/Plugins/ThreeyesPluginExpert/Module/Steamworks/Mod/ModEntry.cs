using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Threeyes.Steamworks
{
    public abstract class ModEntry : MonoBehaviour
    {
        public static ModEntry BaseInstance;

        public virtual void Init()//ToAdd：传入参数Args，按照Windows的规则
        {
            BaseInstance = this;//Init Instance（手动初始化）
        }

        //ToAdd：可以作为Setup方法传入参数EventArgs，按照Windows的规则
    }
    /// <summary>
    /// Mod的入口组件，常用于管理
    /// </summary>
    public abstract class ModEntry<T> : ModEntry
       where T : ModEntry<T>
    {
        public static T Instance;

        public override void Init()
        {
            base.Init();
            Instance = this as T;//Init Instance（手动初始化）
        }
    }
}