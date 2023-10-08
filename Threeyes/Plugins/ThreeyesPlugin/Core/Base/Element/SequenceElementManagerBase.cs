using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Threeyes.Pool;
/// <summary>
/// 管理动态生成的多个内容
/// 
/// Todo:
/// -整合到Threeyes.Core中
/// 
/// </summary>
/// <typeparam name="TElement">实例化元素</typeparam>
/// <typeparam name="TData">元素数据</typeparam>
public abstract class SequenceElementManagerBase<TElement, TData> : SequenceBase<TData>, IShowHide
    where TElement : ElementBase<TData>
    where TData : class
{
    public virtual Transform TfElementParent { get { return tfElementParent; } }//待实例化元素的父物体
    public virtual GameObject PreElement { get { return preElement; } set { preElement = value; } }//元素预制物
    [HideInInspector] public UnityEvent onInitAllElement;//生成所有元素时调用
    [HideInInspector] public UnityEvent onInitElement;//生成新元素时调用

    [Header("Init")]
    public bool isHideOnStart = false;//开始时隐藏
    //初始化设置（2选1）
    public bool isInitOnStart = true;//是否使用已有数据生成元素  （设置为false可保留已存在的元素）
    public bool isInitUsingExistElementOnStart = false;//是否针对已有元素进行初始化（与上面互斥）
    public Transform tfElementParent;//元素的父物体
    public GameObject preElement;//元素预制物

    [Header("Pool")]
    public bool usePool = false;//Pool支持，默认为false
    public int defaultCapacity = 10;
    public int maxSize = 100;

    [Header("Runtime")]
    public List<TElement> listElement = new List<TElement>();//缓存动态实例化后的元素组件

    protected virtual ObjectPool<GameObject> Pool//PS:子类可更换成自己喜欢的Pool类型
    {
        get
        {
            if (pool == null)
            {
                pool = new GameObjectPool(createFunc: () => Instantiate(preElement), defaultCapacity: defaultCapacity, maxSize: maxSize);
            }
            return pool;
        }
    }
    ObjectPool<GameObject> pool;

    #region Unity Method

    protected virtual void Start()
    {
        StartFunc();
    }

    protected virtual void StartFunc()
    {
        if (isInitOnStart)//使用已有数据生成元素
        {
            Init();
        }
        else if (isInitUsingExistElementOnStart)//针对已有元素进行初始化
        {
            InitUsingExistElement();
        }

        if (isHideOnStart)//（创建以后隐藏）
            Hide();
    }

    private void OnDestroy()
    {
        if (usePool && pool != null)
            pool.Dispose();
    }
    #endregion

    #region Public Method

    /// <summary>
    /// 通过传入的数据，生成元素
    /// </summary>
    /// <param name="tempListData"></param>
    public virtual void Init(List<TData> tempListData)
    {
        ListData = tempListData;
        Init();
    }

    /// <summary>
    /// 使用已有的数据，生成元素
    /// </summary>
    public virtual void Init()
    {
        //重置元素和数据
        ResetData();
        ResetElement();

        //初始化元素
        InitElement();
    }

    /// <summary>
    /// 针对已有的元素，使用其自身data进行初始化
    /// </summary>
    public virtual void InitUsingExistElement()
    {
        listData.Clear();//清空数据

        listElement = TfElementParent.GetComponentsInChildren<TElement>(true).ToList();
        for (int i = 0; i != listElement.Count; i++)
        {
            TElement element = listElement[i];
            InitData(element, element.data, i);//使用Element已有的数据及参数进行初始化
            listData.Add(element.data);
        }
    }

    /// <summary>
    /// 重设数据
    /// </summary>
    public virtual void ResetData()
    {
        listElement.Clear();
    }

    /// <summary>
    /// 删除所有元素
    /// </summary>
    public virtual void ResetElement()
    {
        while (TfElementParent.childCount > 0)//使用While而不是foreach，避免访问越界
        {
            Transform tfSon = TfElementParent.GetChild(0);
            DestroyElementFunc(tfSon.gameObject);//PS:初始化调用时，可能物体只是临时物体，所以不能通过获取TElement的方式删除
        }
    }

    /// <summary>
    /// 移除指定元素
    /// </summary>
    /// <param name="element"></param>
    public virtual void RemoveElement(TElement element)
    {
        if (!element)//元素为空
            return;
        if (ListData.Contains(element.data))
            ListData.Remove(element.data);
        if (listElement.Contains(element))
            listElement.Remove(element);

        DestroyElementFunc(element.gameObject);
    }

    /// <summary>
    /// 使用已有数据，初始化全部元素
    /// </summary>
    public virtual void InitElement()
    {
        for (int i = 0; i != ListData.Count; i++)
        {
            TData data = ListData[i];
            if (data == null)
            {
                Debug.LogError("空引用！");
                continue;
            }

            var element = InitElement(PreElement, data, i);
            if (element)
            {
                listElement.Add(element);
            }
        }
        onInitAllElement.Invoke();
    }
    #endregion

    #region Inner Function

    protected virtual TElement InitElement(GameObject goPre, TData data, bool isSetData = true)
    {
        int index = listElement.Count;
        return InitElement(goPre, data, index, true);
    }

    /// <summary>
    /// 生成元素并初始化
    /// </summary>
    /// <param name="goPre"></param>
    /// <param name="data"></param>
    /// <param name="index">Index</param>
    /// <param name="isSetData"></param>
    protected virtual TElement InitElement(GameObject goPre, TData data, int index, bool isSetData = true)
    {
        TElement element = CreateElementFunc(goPre);
        if (element && isSetData)
        {
            InitData(element, data, index);
        }
        onInitElement.Invoke();
        return element;
    }

    /// <summary>
    /// 生成元素并存储到listElement中
    /// </summary>
    /// <param name="goPre"></param>
    /// <returns></returns>
    protected virtual TElement CreateElementFunc(GameObject goPre)
    {
        GameObject goInst = CreateElementGameObjectFunc(goPre);
        goInst.transform.SetParent(TfElementParent);
        TElement element = goInst.GetComponent<TElement>();
        return element;
    }
    protected virtual GameObject CreateElementGameObjectFunc(GameObject goPre)
    {
        return usePool ? Pool.Get() : Instantiate(goPre);
    }

    protected virtual void DestroyElementFunc(GameObject go)
    {
        go.GetComponent<TElement>()?.OnBeforeDestroy();//手动调用方法

        if (usePool)
            Pool.Release(go);
        else
        {
            go.transform.SetParent(null);//PS：因为物体在下一帧被销毁，因此需要先移出父物体，避免影响新元素生成
            Destroy(go);
        }
    }


    /// <summary>
    /// 传入数据初始化指定Element
    /// </summary>
    /// <param name="element"></param>
    /// <param name="data"></param>
    protected virtual void InitData(TElement element, TData data, int index)
    {
        element.Init(data);
    }

    /// <summary>
    /// 获取指定index的元素
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    protected virtual TElement GetElementAt(int index)
    {
        TElement element = listElement[index];
        if (element)
            return element;
        else
        {
            Debug.LogError("没有找到相关元素!");
            return null;
        }
    }

    #endregion

    #region Override IShowHideInterface

    public bool IsShowing { get { return isShowing; } set { isShowing = value; } }
    public bool isShowing = false;

    [HideInInspector] public BoolEvent onShowHide;
    [HideInInspector] public UnityEvent onShow;
    [HideInInspector] public UnityEvent onHide;

    public void Show()
    {
        Show(true);
    }
    public void Hide()
    {
        Show(false);
    }
    public void ToggleShow()
    {
        Show(!IsShowing);
    }
    public void Show(bool isShow)
    {
        IsShowing = isShow;

        if (isShow)
            onShow.Invoke();
        else
            onHide.Invoke();
        onShowHide.Invoke(isShow);

        ShowFunc(isShow);
    }
    protected virtual void ShowFunc(bool isShow)
    {
        gameObject.SetActive(isShow);
    }


    #endregion

    #region Editor Method
#if UNITY_EDITOR

    public override void SetInspectorGUIUnityEventProperty(GUIPropertyGroup group)
    {
        base.SetInspectorGUIUnityEventProperty(group);
        group.listProperty.Add(new GUIProperty(nameof(onInitAllElement)));
        group.listProperty.Add(new GUIProperty(nameof(onInitElement)));
        group.listProperty.Add(new GUIProperty(nameof(onShowHide)));
        group.listProperty.Add(new GUIProperty(nameof(onShow)));
        group.listProperty.Add(new GUIProperty(nameof(onHide)));
    }

#endif
    #endregion
}

/// <summary>
/// 针对ContentElementBase,增加序号以及单例
/// </summary>
/// <typeparam name="TManager"></typeparam>
/// <typeparam name="TElement"></typeparam>
/// <typeparam name="TData"></typeparam>
public abstract class SequenceElementManagerBase<TManager, TElement, TData> : SequenceElementManagerBase<TElement, TData>
    where TManager : SequenceElementManagerBase<TElement, TData>
    where TElement : SequenceElementBase<TManager, TElement, TData>
    where TData : class
{
    protected override void InitData(TElement element, TData data, int index)
    {
        //设置相关引用
        element.Index = index;
        element.Manager = this as TManager;

        base.InitData(element, data, index);
    }
}