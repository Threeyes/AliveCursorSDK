using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 管理一组相同的Component
/// </summary>
public class ComponentGroup : MonoBehaviour
{
    public List<Component> listComponent = new List<Component>();

    public List<Behaviour> listBehaviour = new List<Behaviour>();//可以调用enabled属性，进行开关

    public void SetActive(bool isActive)
    {
        foreach (Behaviour b in listBehaviour)
        {
            b.gameObject.SetActive(isActive);
        }
        foreach (Component c in listComponent)
        {
            c.gameObject.SetActive(isActive);
        }

    }

    public void Enable(bool isEnable)
    {
        foreach (Behaviour b in listBehaviour)
        {
            b.enabled = isEnable;
        }
    }

    public void Remove()
    {
        foreach (Component c in listComponent)
        {
            Destroy(c);
        }
        foreach (Behaviour b in listBehaviour)
        {
            Destroy(b);
        }

    }
}
