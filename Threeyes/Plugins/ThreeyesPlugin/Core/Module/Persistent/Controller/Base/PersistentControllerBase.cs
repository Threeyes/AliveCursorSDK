using System;
using UnityEngine;
using UnityEngine.Events;

namespace Threeyes.Persistent
{
	/// <summary>
	/// 针对指定的Config进行读写，适用于将存储配置于读写分开的情况
	/// 
	/// PS：
	/// 1.DeleteKey、SaveValue等命名参考PlayerPref
	/// 2.不继承MonoBehaviour，便于适配其他场景
	/// </summary>
	/// <typeparam name="TPersistentConfig"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	public abstract class PersistentControllerBase<TValue, TOption> : IPersistentController<TValue>
		where TOption : PersistentControllerOption
	{
		//通知外部事件（如RuntimeEditUI）
		public event UnityAction<TValue> UIChanged;
		public event UnityAction<TValue> ValueChanged;

		public virtual PersistentControllerOption BaseOption { get { return option; } }
		public virtual TOption Option { get { return option; } }
		[SerializeField] protected TOption option;
		public IPersistentData PersistentData { get { return persistentData; } }
		public IPersistentData<TValue> RealPersistentData { get { return persistentData; } }
		protected IPersistentData<TValue> persistentData;
		public virtual bool HasKey { get { return Key.NotNullOrEmpty(); } }

		//——PersistentData field——
		protected string Key { get { return persistentData.Key; } }
		public TValue DefaultValue { get { return persistentData.DefaultValue; } set { persistentData.DefaultValue = value; } }
		public virtual TValue PersistentValue { get { return persistentData.PersistentValue; } protected set { persistentData.PersistentValue = value; } }//已保存或将要保存的持久化值（仅作为缓存及Inspector显示，不可直接访问！)
		public bool HasChanged { get { return persistentData.HasChanged; } set { persistentData.HasChanged = value; } }

		//Runtime
		public bool HasLoaded { get { return hasLoaded; } }
		bool hasLoaded = false;

		public PersistentControllerBase(IPersistentData<TValue> persistentData, PersistentControllerOption option)
		{
			if (option is TOption)
				InitOption(option as TOption);
			else
			{
				Debug.LogError("The option type is not: " + typeof(TOption) + " !");
			}

			this.persistentData = persistentData;
			InitPersistentData(this.persistentData);//（PS：部分PD的Init需要基于Option，所以放在最后）
		}
		protected virtual void InitOption(TOption option)
		{
			this.option = option;
		}
		protected virtual void InitPersistentData(IPersistentData<TValue> persistentData)
		{
			persistentData.Init();//调用初始化代码
		}

		public virtual void LoadValue(TValue overrideDefaultValue)
		{
			DefaultValue = overrideDefaultValue;
			LoadValue();
		}
		public void LoadValue()
		{
			var value = GetValue();

			//更新UI和更新值事件
			NotifyUIChanged(value);
			NotifyValueChanged(value, PersistentChangeState.Load);
			Debug.Log("Load" + (HasKey ? "" : " Default") + ":  [" + persistentData.Key + "] = " + PersistentValue);//ToDelete
			PersistentValue = value;
			hasLoaded = true;
		}

		/// <summary>
		/// 设置值
		/// PS:
		/// 1.由对应的UI主动调用，不会调用NotifyUIChanged避免死循环
		/// </summary>
		/// <param name="value"></param>
		public virtual void SetValue(TValue value)
		{
			if (Key.IsNullOrEmpty())
				return;
			NotifyValueChanged(value, PersistentChangeState.Set);
			Debug.Log("Set [" + persistentData.Key + "] = " + value);//ToDelete
			PersistentValue = value;
			HasChanged = true;
			if (BaseOption.IsSaveOnSet)
				SaveValue();
		}

		[ContextMenu("DeleteKey")]
		public virtual void DeleteKey()// 删除Key，常用于重置
		{
			if (HasKey)
			{
				DeleteKeyFunc();
				Debug.Log("Delete [" + persistentData.Key + "]");
			}
			//Reset to defaultValue（PS：不管有无Key都需要重置，避免用户未保存值时重置
			NotifyUIChanged(DefaultValue);
			NotifyValueChanged(DefaultValue, PersistentChangeState.Delete);
			PersistentValue = DefaultValue;
			HasChanged = false;//Reset
		}


		/// <summary>
		/// 将DefaultValue存储为Persistent文件，便于：
		/// 1.后期修改
		/// 2.生成初始化文件（如存放在StreamingAssets中）
		/// </summary>
		public virtual void SaveDefaultValue()
		{
			//跳过OnlySaveOnChanged等条件，强制写入
			SaveValueFunc(DefaultValue);
			NotifyDefaultValueSaved();
		}
		public virtual void SaveValue()
		{
			if (BaseOption.OnlySaveHasChanged && !HasChanged)//只有用户调用了SetValue导致HasChanged变化，才会保存，避免多余调用
				return;

			//如果TValue是引用类型且值是空，那就存储DefaultValue，否则会报错（可能原因：没有调用Load/Set导致没有正确设置）
			if (PersistentValue == null)
			{
				if (!hasLoaded)
				{
					Debug.LogError($"[{persistentData.Key}]'s value is null! Please invoke Load or Set before you save the value!");
					return;
				}
			}
			SaveValueFunc(PersistentValue);
			HasChanged = false;//Reset state incase SaveValue get called multi time
			Debug.Log("Save [" + persistentData.Key + "] = " + PersistentValue);//ToDelete
		}

		public virtual TValue GetValue()
		{
			return HasKey ? GetValueFunc() : DefaultValue;//模仿PlayerPrefs的处理方式：如果本地无Key，返回默认值
		}

		protected virtual void NotifyUIChanged(TValue value)
		{
			if (value == null)
				return;

			UIChanged.Execute(value);
			persistentData.OnUIChanged(value);
		}

		protected virtual void NotifyValueChanged(TValue value, PersistentChangeState persistentChangeState)
		{
			if (value == null)
				return;

			ValueChanged.Execute(value);//ToAdd:ValueChangedEx,增加State参数
			persistentData.OnValueChanged(value, persistentChangeState);
		}
		protected virtual void NotifyDefaultValueSaved()
		{
			persistentData.OnDefaultValueSaved();
		}
		public virtual void Dispose()
		{
			//在整个静态单例物体被卸载前（如Editor跳出Play）保存
			if (BaseOption.IsSaveOnDispose)
			{
				SaveValue();
			}
			persistentData.Dispose();
		}
		protected abstract TValue GetValueFunc();
		protected abstract void SaveValueFunc(TValue value);
		protected abstract void DeleteKeyFunc();
	}

	public class PersistentControllerOption
	{
		public bool IsSaveOnSet { get { return isSaveOnSet; } set { isSaveOnSet = value; } }
		public bool IsSaveOnDispose { get { return isSaveOnDispose; } set { isSaveOnDispose = value; } }
		public bool OnlySaveHasChanged { get { return onlySaveHasChanged; } set { onlySaveHasChanged = value; } }

		[Tooltip("Save the value after Set")] [SerializeField] protected bool isSaveOnSet = false;
		[Tooltip("Save the value before Dispose")] [SerializeField] protected bool isSaveOnDispose = true;
		[Tooltip("True: only save the value when it has changed;  otherwise save the value anyway, so that you can save all PD into the persistent file and modify them later, useful for runtime generate data")]
		[SerializeField] protected bool onlySaveHasChanged = true;//适用于需要在打包后在程序外部目录生成数据，且让用户手动更改内容。如果需要在SteamingAsset中存储，一般是提前生成，该值设置为false

		public PersistentControllerOption()//PS：在Inspector中创建时会调用这个构造函数
		{
			isSaveOnSet = false;
			isSaveOnDispose = true;
			onlySaveHasChanged = true;
		}
		public PersistentControllerOption(bool isSaveOnSet = false, bool isSaveOnDispose = true, bool onlySaveHasChanged = true)
		{
			this.isSaveOnSet = isSaveOnSet;
			this.isSaveOnDispose = isSaveOnDispose;
			this.onlySaveHasChanged = onlySaveHasChanged;
		}
	}
}