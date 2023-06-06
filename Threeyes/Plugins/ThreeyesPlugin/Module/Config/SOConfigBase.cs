using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threeyes.Config
{
    public class SOConfigBase<TConfig> : ScriptableObject
    {
        public TConfig config;
    }
}