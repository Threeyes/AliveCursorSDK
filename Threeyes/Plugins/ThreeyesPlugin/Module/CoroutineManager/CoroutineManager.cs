using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threeyes.Coroutine
{
    /// <summary>
    /// 用于开启协程的单例类，防止协程因为物体隐藏而被中断
    /// 
    /// ToUpdate:
    /// -待确认StopCoroutine在停止后会不会把cacheEnum设置为null，如果不会则以前代码中的TryStopCoroutine需要主动设置为null
    /// </summary>
    public class CoroutineManager : MonoBehaviour
    {
        static CoroutineManager _Instance;
        public static CoroutineManager Instance
        {
            get
            {
                if (!_Instance)
                {
                    _Instance = GameObject.FindObjectOfType<CoroutineManager>();
                    if (!_Instance)
                    {
                        GameObject newGo = new GameObject(typeof(CoroutineManager).ToString(), typeof(CoroutineManager));
                        _Instance = newGo.GetComponent<CoroutineManager>();
                    }
                    if (Application.isPlaying)
                    {
                        DontDestroyOnLoad(_Instance.gameObject);
                        _Instance.gameObject.hideFlags = HideFlags.HideInHierarchy;
                    }
                }
                return _Instance;
            }
        }

        /// <summary>
        /// 开启协程
        /// 注意：在重新载入场景时，原有的组件可能会为空，因此要判断if(this)才能继续执行
        /// </summary>
        /// <param name="routine"></param>
        /// <returns></returns>
        public static UnityEngine.Coroutine StartCoroutineEx(IEnumerator routine)
        {
            if (routine == null)
                return null;

            if (!Application.isPlaying)
            {
                return null;
            }

            return Instance.StartCoroutine(routine);
        }

        public static void StopCoroutineEx(UnityEngine.Coroutine routine)
        {
            if (routine == null)
                return;

            if (Application.isPlaying)
            {
                Instance.StopCoroutine(routine);
            }
        }
        /// <summary>
        /// Only Execute routine for once, will Stop previous Coroutine
        /// </summary>
        /// <param name="routine"></param>
        /// <param name="cacheRoutine">Use this field to cache output routine</param>
        public static void StartCoroutineSoloEx(IEnumerator routine, ref UnityEngine.Coroutine cacheRoutine)
        {
            if (cacheRoutine != null)
                StopCoroutineEx(cacheRoutine);

            cacheRoutine = StartCoroutineEx(routine);
        }
    }
}