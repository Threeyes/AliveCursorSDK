using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 管理GameObject
/// </summary>
public class GameObjectHelper : MonoBehaviour 
{
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

    public void DeStroy()
    {
        GameObject.Destroy(Go);
    }

    //常用于数据回传
    public void DeStroy(GameObject go)
    {
        if (go)
            GameObject.Destroy(go);

    }
    public void DeStroy(Component component)
    {
        if (component)
            GameObject.Destroy(component.gameObject);
    }

}
