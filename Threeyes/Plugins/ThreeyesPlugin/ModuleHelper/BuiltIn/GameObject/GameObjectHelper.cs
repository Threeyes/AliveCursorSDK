using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threeyes.ModuleHelper
{
    /// <summary>
    /// 管理GameObject
    /// </summary>
    public class GameObjectHelper : MonoBehaviour
    {
        /// <summary>
        /// Set the desire child active, whild the other childs will remain deactive
        /// </summary>
        /// <param name="index"></param>
        public void SetChildActiveSolo(int index)
        {
            for (int i = 0; i != transform.childCount; i++)
            {
                Transform tfChild = transform.GetChild(i);
                tfChild.gameObject.SetActive(i == index);
            }
        }


        public GameObject go;
        public GameObject Go
        {
            get
            {
                if (!go)
                    go = gameObject;
                return go;
            }
        }

        public void DestroySelf()
        {
            Destroy(Go);
        }

        //常用于数据回传
        public void DestroyTarget(GameObject go)
        {
            if (go)
                Destroy(go);

        }
        public void DestroyComponent(Component component)
        {
            if (component)
                Destroy(component.gameObject);
        }

        #region Obsolete
        [System.Obsolete("Incorrect spelling! Use DestroySelf instead.", true)]
        public void DeStroy()
        {
            Destroy(Go);
        }

        [System.Obsolete("Incorrect spelling! Use DestroyTarget instead.", true)]
        public void DeStroy(GameObject go)
        {
            if (go)
                Destroy(go);
        }
        [System.Obsolete("Incorrect spelling! Use DestroyComponent instead.", true)]
        public void DeStroy(Component component)
        {
            if (component)
                Destroy(component.gameObject);
        }
        #endregion
    }
}