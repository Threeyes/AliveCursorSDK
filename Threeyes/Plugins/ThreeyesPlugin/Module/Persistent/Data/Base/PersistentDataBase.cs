using UnityEngine;
using UnityEngine.Events;
using System;
using System.Text;
using Threeyes.Data;
using Threeyes.Core;
#if USE_NaughtyAttributes
using NaughtyAttributes;
#endif
namespace Threeyes.Persistent
{
    public abstract partial class PersistentDataBase : MonoBehaviour, IPersistentData, IHierarchyViewInfo
    {
        /// <summary>
        /// 
        /// PS:供有需要的时候使用（如PropertyBags）
        /// 
        /// </summary>
        public virtual FilePathModifier FilePathModifier
        {
            get
            {
                return new FilePathModifier_PD(this);//PS:改为动态创建，方便更新路径
                //if (filePathModifier_PD == null)
                //    filePathModifier_PD = new FilePathModifier_PD(this);
                //return filePathModifier_PD;
            }
            set { Debug.LogError("This property can't set!"); /*暂时不允许设置，避免用户魔改*/}
        }
        //private FilePathModifier_PD filePathModifier_PD;
        public abstract Type ValueType { get; set; }
        public string PersistentDirPath { get; set; }

        public string Key { get { return key; } set { key = value; } }
        public string Tooltip { get { return tooltip; } set { tooltip = value; } }
        [SerializeField] protected string key = "";//key
        [SerializeField] protected string tooltip;//Detail for this config

        public virtual bool IsValid { get { return PathTool.IsValidFileName(Key); } }

        public virtual void Init() { }

        public virtual void Dispose() { }
        public virtual void Clear()
        {
            //filePathModifier_PD = null;//清空，方便链接其他路径
        }

#if UNITY_EDITOR
        //——MenuItem——
        public const string strMenuItem_Root = "GameObject/PersistentData/";
        public const string strMenuItem_Root_Basic = strMenuItem_Root + "Basic/";
        public const string strMenuItem_Root_Basic_Ex = strMenuItem_Root_Basic + "Ex/";
        public const string strMenuItem_Root_File = strMenuItem_Root + "File/";
        public const string strMenuItem_Root_Object = strMenuItem_Root + "Object/";
        public const string strMenuItem_Root_PropertyBag = strMenuItem_Root + "PropertyBag/";
        public const int intBasicMenuOrder = 100;
        public const int intFileMenuOrder = 200;
        public const int intScriptableObjectMenuOrder = 300;


        //——Hierarchy GUI——
        protected static StringBuilder sbCache = new StringBuilder();
        public virtual string ShortTypeName { get { return ""; } }
        public virtual void SetHierarchyGUIProperty(StringBuilder sB)
        {
            //["Key"]
            string keyValue = $"\"{(Key.IsNullOrEmpty() ? "Null" : Key)}\"";//PS:需要将双引号囊括进去，避免无法显示以空格开头的错误
            sB.Append(keyValue);


            if (!IsValid)
            {
                sB.WrapWarningRichText();
            }
            else
            {
                //ToUpdate:通过返回bool的分部方法判断是否有效（需要等到C#9支持返回非void的分部方法 https://github.com/dotnet/csharplang/blob/main/proposals/csharp-9.0/extending-partial-methods.md）
                //检查是否有其他错误（如重复Key）
                SetHierarchyGUIPropertyEx(sB);
            }
        }

        //——Inspector GUI——
        public virtual void SetInspectorGUIHelpBox_Error(StringBuilder sB)
        {
            string strErrorInfo = "";
            if (!PathTool.IsValidFileName(Key, ref strErrorInfo))//Key不能是无法识别的本地文件命名规则
            {
                sB.Append("Key: " + strErrorInfo);
                sB.Append("\r\n");
            }
            else
            {
                SetInspectorGUIHelpBox_ErrorEx(sB);
            }
        }

        partial void SetHierarchyGUIPropertyEx(StringBuilder sB);
        partial void SetInspectorGUIHelpBox_ErrorEx(StringBuilder sB);
#endif
    }

    /// <summary>
    /// 存储持久化数据
    /// （只起到配置文件的作用，无实际调用方法。）
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TEvent"></typeparam>
    public abstract class PersistentDataBase<TValue, TEvent> : PersistentDataBase, IPersistentData<TValue>
    where TEvent : UnityEvent<TValue>
    {
        public System.Func<TValue> GetSavedValue;//override需要保存的数据，适用于代码替换
        public virtual TValue DefaultValue { get { return defaultValue; } set { defaultValue = value; } }
        public virtual TValue PersistentValue { get { return persistentValue; } set { persistentValue = value; } }
        public virtual TValue ValueToSaved
        {
            get
            {
                if (GetSavedValue != null)//优先使用有自定义方法获取值
                    return GetSavedValue();
                return PersistentValue;
            }
        }//持久化时需要存储的值
        public bool SaveAnyway { get { return saveAnyway; } }
        public bool HasChanged { get { return hasChanged; } set { hasChanged = value; } }

        public bool HasLoadedFromExtern { get { return hasLoadedFromExtern; } set { hasLoadedFromExtern = value; } }

        public override Type ValueType { get { return typeof(TValue); } set { /*除非必要，暂不设置*/} }

        public UnityEvent<TValue> EventOnUIChanged { get { return onUIChanged; } }
        public UnityEvent<TValue> EventOnValueChanged { get { return onValueChanged; } }

        [SerializeField] protected TValue defaultValue = default(TValue);//默认值

        [SerializeField] protected TEvent onUIChanged;//当通过某个UI元素进行SetValue，则需要通过该事件调用UI的SetValueWithoutNotify方法（注意该事件不应该间接调用到该类实例中的Set方法，因为会导致循环调用）
        [SerializeField] protected TEvent onValueChanged;//值被更新时调用（注意该事件不应该间接调用到该类实例中的Set方法，因为会导致循环调

        [Header("Runtime")]
#if USE_NaughtyAttributes
        [AllowNesting]
        [ReadOnly]
#endif
        [SerializeField] protected bool hasChanged = false;//Check if the user has set the value, use this to decide to save or not
#if USE_NaughtyAttributes
        [AllowNesting]
        [ReadOnly]
#endif
        [SerializeField] protected bool hasLoadedFromExtern = false;//Check if the value has been loaded from extern, ignore using the default value

#if USE_NaughtyAttributes
        [AllowNesting]
        [ReadOnly]
#endif
        [SerializeField] protected TValue persistentValue;

        [Header("Config")]
        [SerializeField] protected bool saveAnyway = false;//让Controller忽略hasChanged字段，直接保存。适用于需要修改复杂类的成员但无法直接调用PD.SetDirty的情况

        public virtual void OnDefaultValueSaved() { }
        /// <summary>
        /// 同步绑定UI的值
        /// </summary>
        /// <param name="value"></param>
        public virtual void OnUIChanged(TValue value)
        {
            onUIChanged.Invoke(value);//调用UI更新事件
        }
        /// <summary>
        /// 值被更新后调用
        /// </summary>
        /// <param name="value"></param>
        public virtual void OnValueChanged(TValue value, PersistentChangeState persistentChangeState)
        {
            onValueChanged.Invoke(value);
        }

        /// <summary>
        /// 直接更改数值后，需要标记为修改，好让Controller存储
        /// PS:
        /// -如果通过UIField_XXX修改字段不需要调用此方法
        /// </summary>
        [ContextMenu("SetDirty")]
        public virtual void SetDirty()
        {
            HasChanged = true;
        }

        /// <summary>
        /// 清空并重置字段，方便复用
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            //重置为默认值，并清空其他Action回调
            defaultValue = default(TValue);
            persistentValue = default(TValue);
        }

        #region Editor Method
#if UNITY_EDITOR

        //Wanring: SetInspectorGUIHelpBox_Error需要IsValid的条件同步
        public override void SetHierarchyGUIProperty(StringBuilder sB)
        {
            //ToAdd：显示Default Value状态
            base.SetHierarchyGUIProperty(sB);
        }
#endif
        #endregion
    }

    /// <summary>
    /// PD WithOption
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TEvent"></typeparam>
    /// <typeparam name="TDataOption">TValue's realte DataOption</typeparam>
    public abstract class PersistentDataBase<TValue, TEvent, TDataOption> : PersistentDataBase<TValue, TEvent>, IDataOptionHolder<TDataOption>
        where TEvent : UnityEvent<TValue>
        where TDataOption : IDataOption
    {
        public virtual IDataOption BaseDataOption { get { return dataOption; } }
        public virtual TDataOption DataOption { get { return dataOption; } }
        [SerializeField] protected TDataOption dataOption;
    }
}