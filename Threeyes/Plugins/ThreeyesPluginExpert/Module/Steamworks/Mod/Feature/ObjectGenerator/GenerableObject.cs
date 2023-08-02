using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threeyes.Steamworks
{
    public class GenerableObject<TConfigInfo> : MonoBehaviour
    {
        public TConfigInfo Config { get { return config; } }
        [SerializeField] protected TConfigInfo config;

        public virtual void Init(TConfigInfo config)
        {
            this.config = config;
        }
    }
}