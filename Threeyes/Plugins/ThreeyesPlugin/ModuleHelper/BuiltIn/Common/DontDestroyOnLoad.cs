using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DontDestroyOnLoad : MonoBehaviour
{
    public bool isSingleton = true;//true:重载场景时不会激活多个实例
    public static List<string> listInstance = new List<string>();//缓存已经生成的单例

    //当前是否设置成功(可用于调用初始化单例的方法）
    public UnityEvent onSet;
    public BoolEvent onSetUnSet;
    private void Awake()
    {
        if (isSingleton)
        {
            if (!listInstance.Contains(name))
            {
                SetUpDontDestroyOnLoad();
                listInstance.Add(name);
                onSet.Invoke();
                onSetUnSet.Invoke(true);
            }
            else
            {
                gameObject.SetActive(false);
                onSetUnSet.Invoke(false);
            }
        }
        else
        {
            SetUpDontDestroyOnLoad();
        }
    }

    void SetUpDontDestroyOnLoad()
    {
        if (transform.parent)
            transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }
}
