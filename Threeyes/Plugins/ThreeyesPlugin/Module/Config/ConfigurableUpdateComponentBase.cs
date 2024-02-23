using Threeyes.Core;
using UnityEngine;

namespace Threeyes.Config
{
    /// <summary>
    /// Component with configable SO, along with different type of update method
    /// </summary>
    /// <typeparam name="TComp"></typeparam>
    /// <typeparam name="TSOConfig"></typeparam>
    /// <typeparam name="TConfig"></typeparam>
    public abstract class ConfigurableUpdateComponentBase<TComp, TSOConfig, TConfig> : ConfigurableComponentBase<TComp, TSOConfig, TConfig>
        where TComp : Component
        where TSOConfig : SOConfigBase<TConfig>
    {
        public UpdateMethodType updateMethodType = UpdateMethodType.Late;
        protected float DeltaTime
        {
            get {
                switch (updateMethodType)
                {
                    case UpdateMethodType.Default:
                    case UpdateMethodType.Late:
                        return Time.deltaTime;
                    case UpdateMethodType.Fixed:
                        return Time.fixedDeltaTime;
                    default:
                        Debug.LogError(updateMethodType + " Not Define!");
                        return 0;
                }
            }
        }

        protected virtual void Update()
        {
            if (updateMethodType != UpdateMethodType.Default)
                return;
            UpdateFunc();
        }
        protected virtual void LateUpdate()
        {
            if (updateMethodType != UpdateMethodType.Late)
                return;
            UpdateFunc();
        }
        protected virtual void FixedUpdate()
        {
            if (updateMethodType != UpdateMethodType.Fixed)
                return;
            UpdateFunc();
        }

        protected virtual void UpdateFunc()
        {
            //PS:可能会新增方法
        }
    }
}