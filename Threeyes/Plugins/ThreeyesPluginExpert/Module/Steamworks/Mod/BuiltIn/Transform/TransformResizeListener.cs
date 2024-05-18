using System.Collections;
using System.Collections.Generic;
using Threeyes.Core;
using UnityEngine;
using UnityEngine.Events;

namespace Threeyes.Steamworks
{
    /// <summary>
    /// Notify transform's size change event
    /// </summary>
    public class TransformResizeListener : MonoBehaviour
    {
        public UnityEvent onScaleChanged = new UnityEvent();

        //Runtime
        Vector3 curSize;//Global size
        private void Awake()
        {
            ///ToAdd:
            ///-增加选项：是否在初始化时记录，否则需要自行设置curSize（但要等uMod还原后才能进行LateUpdate检测）
            //Record size on start
            curSize = transform.lossyScale;
        }

        private void LateUpdate()
        {
            if (transform.lossyScale != curSize)//仅当尺寸变化时（包括开始时的变化）才会更改材质，避免创建多余的克隆体
            {
                curSize = transform.lossyScale;
                onScaleChanged.Invoke();
            }

        }
    }
}