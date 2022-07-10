using UnityEngine;

public abstract class AC_GenerableObject<TConfigInfo> : MonoBehaviour
{
    public TConfigInfo Config { get { return config; } }
    [SerializeField] protected TConfigInfo config;

    public virtual void Init(TConfigInfo config)
    {
        this.config = config;
    }
}