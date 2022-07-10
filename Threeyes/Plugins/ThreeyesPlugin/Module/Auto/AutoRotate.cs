using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 自动旋转
/// </summary>
public class AutoRotate : MonoBehaviour
{
    [Tooltip("Angular velocity in degrees per seconds")]
    public float degPerSec = 60.0f;

    [Tooltip("Rotation axis")]
    public Vector3 rotAxis = Vector3.up;
    public Space relativeTo = Space.Self;
    // Use this for initialization
    private void Start()
    {
        rotAxis.Normalize();
    }

    // Update is called once per frame
    private void Update()
    {
        transform.Rotate(rotAxis, degPerSec * Time.deltaTime, relativeTo);
    }
}
