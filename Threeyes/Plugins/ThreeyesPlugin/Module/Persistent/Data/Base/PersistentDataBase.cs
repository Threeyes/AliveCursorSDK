using UnityEngine;
using UnityEngine.Events;
using System;
using System.Text;
using Threeyes.Data;
#if USE_NaughtyAttributes
using NaughtyAttributes;
#endif
namespace Threeyes.Persistent
{
	public abstract partial class PersistentDataBase : MonoBehaviour, IPersistentData, IHierarchyViewInfo
	{
		public abstract Type ValueType { get; }
		public string PersistentDirPath { get; set; }

		public string Key { get { return key; } set { key = value; } }
		public string Tooltip { get { return tooltip; } set { tooltip = value; } }
		[SerializeField] protected string key = "";//key
		[SerializeField] protected string tooltip;//Detail for this config

		public virtual bool IsValid { get { return PathTool.IsValidFileName(Key); } }

		public virtual void Init() { }

		public virtual void Dispose() { }

#if UNITY_EDITOR
		//——MenuItem——
		public const string strMenuItem_Root = "GameObject/PersistentData/";
		public const string strMenuItem_Root_Basic = strMenuItem_Root + "Basic/";
		public const string strMenuItem_Root_Basic_Ex = strMenuItem_Root_Basic + "Ex/";
		public const string strMenuItem_Root_File = strMenuItem_Root + "File/";
		public const string strMenuItem_Root_SO = strMenuItem_Root + "ScriptableObject/";
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
		public virtual TValue DefaultValue { get { return defaultValue; } set { defaultValue = value; } }
		public virtual TValue PersistentValue { get { return persistentValue; } set { persistentValue = value; } }
		public bool HasChanged { get { return hasSetValue; } set { hasSetValue = value; } }
		public override Type ValueType { get { return typeof(TValue); } }

		public UnityEvent<TValue> EventOnUIChanged { get { return onUIChanged; } }
		public UnityEvent<TValue> EventOnValueChanged { get { return onValueChanged; } }

		[SerializeField] protected TValue defaultValue = default(TValue);//默认值

		[SerializeField] protected TEvent onUIChanged;//当通过某个UI元素进行SetValue，则需要通过该事件调用UI的SetValueWithoutNotify方法（注意该事件不应该间接调用到该类实例中的Set方法，因为会导致循环调用）
		[SerializeField] protected TEvent onValueChanged;//值被更新时调用（注意该事件不应该间接调用到该类实例中的Set方法，因为会导致循环调

#if USE_NaughtyAttributes
		[AllowNesting]
		[ReadOnly]
#endif
		[SerializeField] protected bool hasSetValue = false;//Check if the user has set the value, use this to decide to save or not
#if USE_NaughtyAttributes
		[AllowNesting]
		[ReadOnly]
#endif
		[SerializeField] protected TValue persistentValue;

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
	/// 
	/// </summary>
	/// <typeparam name="TValue"></typeparam>
	/// <typeparam name="TEvent"></typeparam>
	/// <typeparam name="TDataOption">TValue's realte DataOption</typeparam>
	public abstract class PersistentDataBase<TValue, TEvent, TDataOption> : PersistentDataBase<TValue, TEvent>, IDataOptionContainer<TDataOption>
		where TEvent : UnityEvent<TValue>
		where TDataOption : IDataOption
	{
		public virtual IDataOption BaseDataOption { get { return dataOption; } }
		public virtual TDataOption DataOption { get { return dataOption; } }
		[SerializeField] protected TDataOption dataOption;
	}
}