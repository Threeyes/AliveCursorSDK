using System.Collections;
using System.Collections.Generic;
using Threeyes.Core;
using UnityEngine;
using UnityEngine.Events;

namespace Threeyes.ModuleHelper
{
    public class MonoBehaviourHelper : MonoBehaviour
    {
        public UnityEvent onEnable;
        public UnityEvent onDisable;
        public BoolEvent onEnableDisable;
        public UnityEvent onAwake;
        public UnityEvent onStart;
        public UnityEvent onApplicationQuit;

        private void OnEnable()
        {
            onEnable.Invoke();
            onEnableDisable.Invoke(true);
        }

        private void OnDisable()
        {
            onDisable.Invoke();
            onEnableDisable.Invoke(false);
        }

        private void Awake()
        {
            onAwake.Invoke();


        }
        // Use this for initialization
        void Start()
        {
            onStart.Invoke();
        }

        private void OnApplicationQuit()
        {
            onApplicationQuit.Invoke();
        }

    }
}