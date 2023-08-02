using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Threeyes.Pool;
/// <summary>
/// ����̬���ɵĶ������
/// 
/// Todo:
/// -���ϵ�Threeyes.Core��
/// 
/// </summary>
/// <typeparam name="TElement">ʵ����Ԫ��</typeparam>
/// <typeparam name="TData">Ԫ������</typeparam>
public abstract class SequenceElementManagerBase<TElement, TData> : SequenceBase<TData>, IShowHide
    where TElement : ElementBase<TData>
    where TData : class
{
    public virtual Transform TfElementParent { get { return tfElementParent; } }//��ʵ����Ԫ�صĸ�����
    public virtual GameObject PreElement { get { return preElement; } set { preElement = value; } }//Ԫ��Ԥ����
    [HideInInspector] public UnityEvent onInitAllElement;//��������Ԫ��ʱ����
    [HideInInspector] public UnityEvent onInitElement;//������Ԫ��ʱ����

    [Header("Init")]
    public bool isHideOnStart = false;//��ʼʱ����
    //��ʼ�����ã�2ѡ1��
    public bool isInitOnStart = true;//�Ƿ�ʹ��������������Ԫ��  ������Ϊfalse�ɱ����Ѵ��ڵ�Ԫ�أ�
    public bool isInitUsingExistElementOnStart = false;//�Ƿ��������Ԫ�ؽ��г�ʼ���������滥�⣩
    public Transform tfElementParent;//Ԫ�صĸ�����
    public GameObject preElement;//Ԫ��Ԥ����

    [Header("Pool")]
    public bool usePool = false;//Pool֧�֣�Ĭ��Ϊfalse
    public int defaultCapacity = 10;
    public int maxSize = 100;

    [Header("Runtime")]
    public List<TElement> listElement = new List<TElement>();//���涯̬ʵ�������Ԫ�����

    protected virtual ObjectPool<GameObject> Pool//PS:����ɸ������Լ�ϲ����Pool����
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
        if (isInitOnStart)//ʹ��������������Ԫ��
        {
            Init();
        }
        else if (isInitUsingExistElementOnStart)//�������Ԫ�ؽ��г�ʼ��
        {
            InitUsingExistElement();
        }

        if (isHideOnStart)//�������Ժ����أ�
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
    /// ͨ����������ݣ�����Ԫ��
    /// </summary>
    /// <param name="tempListData"></param>
    public virtual void Init(List<TData> tempListData)
    {
        ListData = tempListData;
        Init();
    }

    /// <summary>
    /// ʹ�����е����ݣ�����Ԫ��
    /// </summary>
    public virtual void Init()
    {
        //����Ԫ�غ�����
        ResetData();
        ResetElement();

        //��ʼ��Ԫ��
        InitElement();
    }

    /// <summary>
    /// ������е�Ԫ�أ�ʹ��������data���г�ʼ��
    /// </summary>
    public virtual void InitUsingExistElement()
    {
        listData.Clear();//�������

        listElement = TfElementParent.GetComponentsInChildren<TElement>(true).ToList();
        for (int i = 0; i != listElement.Count; i++)
        {
            TElement element = listElement[i];
            InitData(element, element.data, i);//ʹ��Element���е����ݼ��������г�ʼ��
            listData.Add(element.data);
        }
    }

    /// <summary>
    /// ��������
    /// </summary>
    public virtual void ResetData()
    {
        listElement.Clear();
    }

    /// <summary>
    /// ɾ������Ԫ��
    /// </summary>
    public virtual void ResetElement()
    {
        while (TfElementParent.childCount > 0)//ʹ��While������foreach���������Խ��
        {
            Transform tfSon = TfElementParent.GetChild(0);
            DestroyElementFunc(tfSon.gameObject);//PS:��ʼ������ʱ����������ֻ����ʱ���壬���Բ���ͨ����ȡTElement�ķ�ʽɾ��
        }
    }

    /// <summary>
    /// �Ƴ�ָ��Ԫ��
    /// </summary>
    /// <param name="element"></param>
    public virtual void RemoveElement(TElement element)
    {
        if (!element)//Ԫ��Ϊ��
            return;
        if (ListData.Contains(element.data))
            ListData.Remove(element.data);
        if (listElement.Contains(element))
            listElement.Remove(element);

        DestroyElementFunc(element.gameObject);
    }

    /// <summary>
    /// ʹ���������ݣ���ʼ��ȫ��Ԫ��
    /// </summary>
    public virtual void InitElement()
    {
        for (int i = 0; i != ListData.Count; i++)
        {
            TData data = ListData[i];
            if (data == null)
            {
                Debug.LogError("�����ã�");
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
    /// ����Ԫ�ز���ʼ��
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
    /// ����Ԫ�ز��洢��listElement��
    /// </summary>
    /// <param name="goPre"></param>
    /// 
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
        go.GetComponent<TElement>()?.OnBeforeDestroy();//�ֶ����÷���

        if (usePool)
            Pool.Release(go);
        else
        {
            go.transform.SetParent(null);//PS����Ϊ��������һ֡�����٣������Ҫ���Ƴ������壬����Ӱ����Ԫ������
            Destroy(go);
        }
    }


    /// <summary>
    /// �������ݳ�ʼ��ָ��Element
    /// </summary>
    /// <param name="element"></param>
    /// <param name="data"></param>
    protected virtual void InitData(TElement element, TData data, int index)
    {
        element.Init(data);
    }

    /// <summary>
    /// ��ȡָ��index��Ԫ��
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
            Debug.LogError("û���ҵ����Ԫ��!");
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
/// ���ContentElementBase,��������Լ�����
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
        //�����������
        element.Index = index;
        element.Manager = this as TManager;

        base.InitData(element, data, index);
    }
}