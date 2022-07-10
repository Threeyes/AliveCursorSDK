using UnityEngine;
/// <summary>
///保存 Transform 的相关信息
/// </summary>
[System.Serializable]
public struct TransformInfo
{
    //Data
    public RecordType recordType;

    [HideInInspector]
    public Transform tfThis;
    public Transform tfParent;

    //World
    public Vector3 position;
    public Vector3 eulerAngles;
    public Quaternion rotation;
    public Vector3 lossyScale;

    //Local
    public Vector3 localPosition;
    public Vector3 localEulerAngles;
    [HideInInspector]
    public Quaternion localRotation;

    //Common
    [SerializeField]
    public Vector3 localScale;//因为lossyScale不能set，因此对该值保存一份

    public bool isInit;

    public static void Init(TransformInfo transformInfo)
    {
        transformInfo.position = default(Vector3);
    }
    public TransformInfo(Transform target, RecordType recordType = RecordType.All):this()
    {
        if (!target)
        {
            Debug.LogError("Target is Null!");
        }

        this.recordType = recordType;
        this.Record(target);
    }

    /// <summary>
    /// 记录
    /// </summary>
    /// <param name="target"></param>
    public TransformInfo Record(Transform target)
    {
        try
        {
            this.tfThis = target;
            tfParent = target.parent;

            if (recordType == RecordType.World || recordType == RecordType.All)
            {
                position = target.position;
                eulerAngles = target.eulerAngles;
                rotation = target.rotation;
                lossyScale = target.lossyScale;
            }

            if (recordType == RecordType.Local || recordType == RecordType.All)
            {
                localPosition = target.localPosition;
                localEulerAngles = target.localEulerAngles;
                localRotation = target.localRotation;
            }

            localScale = target.localScale;
            isInit = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
        return this;
    }


    public void Reset(bool isSetParent = true, bool isSetLocalPosition = true, bool isSetLocalEulerAngles = true, bool isSetLocalScale = false)
    {
        Set(tfThis, isSetParent, isSetLocalPosition, isSetLocalEulerAngles, isSetLocalScale);
    }

    public void ResetWorld()
    {
        SetWorld(tfThis);
    }

    public void SetWorld(Transform target)
    {
        if (!target)
            return;

        target.position = position;
        target.eulerAngles = eulerAngles;
    }
    /// <summary>
    /// 将保存的参数赋值到target
    /// </summary>
    /// <param name="target"></param>
    public void Set(Transform target, bool isSetParent = true, bool isSetLocalPosition = true, bool isSetLocalEulerAngles = true, bool isSetLocalScale = false)
    {
        //小技巧：如果issetparent为false，可以先setparent，再设置位置，最后将parent设为默认
        Transform cacheParent = target.parent;
        if (tfParent)
        {
            target.parent = tfParent;
        }

        //计算相对于父物体的位移值
        if (isSetLocalPosition)
        {
            target.localPosition = localPosition;
        }
        if (isSetLocalEulerAngles)
        {
            target.localEulerAngles = localEulerAngles;
        }
        if (isSetLocalScale)
        {
            target.localScale = localScale;
        }

        //重置父物体引用
        if (!isSetParent && tfParent)
            target.SetParent(cacheParent, true);
    }

    /// <summary>
    /// 记录的类型
    /// </summary>
    public enum RecordType
    {
        Local = 0x01,
        World = 0x02,
        All = 0x04,
    }
}
