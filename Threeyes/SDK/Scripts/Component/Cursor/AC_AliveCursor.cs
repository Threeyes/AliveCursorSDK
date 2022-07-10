using UnityEngine;

/// <summary>
/// The Root Manager for mod item
/// </summary>
public class AC_AliveCursor : MonoBehaviour
{
    public static AC_AliveCursor Instance;
    public virtual void Init()
    {
        Instance = this;//Update Instance
    }

}