using System.Collections;
using System.Collections.Generic;
using Threeyes.Core;
using UnityEngine;

namespace Threeyes.Steamworks
{
    /// <summary>
    /// Control Creeper's state
    /// </summary>
    public class CreeperModelController : MonoBehaviour
        , IHubProgramActiveHandler
        , IHubSystemWindow_ChangeCompletedHandler
    {
        public Transform tfParent;//Parent of this gameobject, Control Model Scale (Default scale must be one)
        public CreeperTransformController creeperTransformController;

        [Header("Runtime")]
        public float baseScale = 1;//模型基础缩放值

        #region Callback
        public void OnProgramActiveChanged(bool isActive)
        {
            if (isActive)
                Resize();
            else
                gameObject.SetActive(false);
        }
        public void OnWindowChangeCompleted()
        {
            creeperTransformController.Teleport();
        }
        #endregion

        protected UnityEngine.Coroutine cacheEnumResize;
        public void Resize()
        {
            TryStopCoroutine_Resize();
            cacheEnumResize = CoroutineManager.StartCoroutineEx(IEResize());
        }
        protected virtual void TryStopCoroutine_Resize()
        {
            if (cacheEnumResize != null)
                CoroutineManager.StopCoroutineEx(cacheEnumResize);
        }
        IEnumerator IEResize()
        {
            //让Rig相关组件强制更新(缩放后需要重新显隐，否则RigBuilder不会更新)
            gameObject.SetActive(false);

            //直接缩放父物体
            tfParent.localScale = Vector3.one * baseScale;

            //更新关节
            creeperTransformController.MoveAllLeg();
            yield return null;//等待缩放不为0才能激活，否则会报错
            gameObject.SetActive(true);
        }
    }
}