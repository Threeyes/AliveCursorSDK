using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Threeyes.Persistent
{
	/// <summary>
	/// 作为场景实例Manager，管理子PersistentData并生成对应的Controller
	/// 
	/// PS：
	/// 1.可以有多个实例，分别管理不同文件夹及类型的资源
	/// </summary>
	[DefaultExecutionOrder(-20000)]//PS：子类都需要添加这行，确保在所有Awake之前、InstanceManager之后调用
	public abstract class PersistentControllerManagerBase : ComponentGroupBase<IPersistentData>, ISetInstance, System.IDisposable
	{
		public UnityAction actBeforeInit;//便于更改设置（如根据平台，增加对应后缀）
		public UnityAction actAfterSaveDefaultValue;//便于更改设置（如根据平台，增加对应后缀）

		public List<IPersistentController> ListController { get { return listController; } set { listController = value; } }
		protected List<IPersistentController> listController = new List<IPersistentController>();

		[Header("Config")]
		public bool isAutoInitAndLoad = true;//自动Init并Load所有子PD，适用于针对提前定义
		public bool isDisposeOnDestroy = true;//OnDestroy时自动销毁，否则需要手动调用

		#region Instance
		bool isInit = false;
		protected PersistentControllerManagerGroup cacheManagerGroup;
		public void SetInstance()
		{
			if (isInit)
				return;
			SetInstanceFunc();
			isInit = true;
		}
		protected virtual void SetInstanceFunc()
		{
			cacheManagerGroup = PersistentControllerManagerGroup.Instance;//缓存起来，避免在程序退出时访问单例导致其创建物体而报错：（Some objects were not cleaned up when closing the scene...)
			cacheManagerGroup.Add(this);
		}
		#endregion.

		public abstract void Init(List<IPersistentData> listPersistentData);
		protected virtual void OnDestroy()
		{
			if (isDisposeOnDestroy)
			{
				Dispose();
			}
		}
		public virtual void Dispose()
		{
			DisposeAllValue();
			cacheManagerGroup?.Remove(this);//PS：如果ManagerGroup同时被销毁，那就不需要Remove；否则可能只是移除该物体
		}

		[ContextMenu("LoadAllValue")]
		public void LoadAllValue()
		{
			listController.FindAll(m => m != null).ForEach(m => m.LoadValue()
			);
		}

		[ContextMenu("SaveDefaultValue")]
		public void SaveDefaultValue()//保存所有默认值
		{
			listController.FindAll(m => m != null).ForEach(m => m.SaveDefaultValue());
			actAfterSaveDefaultValue.Execute();
		}

		[ContextMenu("SaveAllValue")]
		public void SaveAllValue()//保存所有当前值
		{
			listController.FindAll(m => m != null).ForEach(m => m.SaveValue());
		}

		[ContextMenu("DeleteAllKey")]
		public void DeleteAllKey()
		{
			listController.FindAll(m => m != null).ForEach(m => m.DeleteKey());
		}
		public void DisposeAllValue()
		{
			listController.FindAll(m => m != null).ForEach(m => m.Dispose());//调用Dispose
		}


	}

	/// <summary>
	/// 
	/// ToUpdate:如果子类要在Load前完成SO数据类的事件监听，有以下2种实现：
	/// 1.增加类似InstanceManager的辅助类，但执行顺序比其更优先
	/// 2.取消isAutoInitAndLoad，改为自行调用LoadAllValue方法
	/// 
	/// </summary>
	/// <typeparam name="TControllerFactory"></typeparam>
	/// <typeparam name="TControllerOption"></typeparam>
	[DefaultExecutionOrder(-20000)]//PS：子类都需要添加这行，确保在所有Awake之前、InstanceManager之后调用
	public abstract class PersistentControllerManagerBase<TControllerFactory, TControllerOption> : PersistentControllerManagerBase
		  where TControllerFactory : PersistentControllerFactoryBase, new()
		where TControllerOption : PersistentControllerOption
	{
		public TControllerFactory persistentControllerFactory;
		public TControllerOption controllerOption;//controller统一的配置

		protected override void SetInstanceFunc()//优先于Awake初始化
		{
			base.SetInstanceFunc();
			persistentControllerFactory = CreateFactory();
		}

		protected virtual void Awake()
		{
			if (isAutoInitAndLoad)
			{
				InitAndLoad();
			}
		}

		public void InitAndLoad()
		{
			Init();
			LoadAllValue();
		}
		public void Init()
		{
			Init(ListComp);//利用所有的子物体创建对应Controller
		}
		public override void Init(List<IPersistentData> listPersistentData)
		{
			actBeforeInit.Execute();
			foreach (var pd in listPersistentData)
			{
				IPersistentController controller = InitElement(pd, controllerOption);
				if (controller != null)
					listController.Add(controller);
			};
		}
		protected virtual TControllerFactory CreateFactory()
		{
			return new TControllerFactory();
		}
		protected virtual IPersistentController InitElement(IPersistentData persistentData, TControllerOption controllerOption)
		{
			if (!persistentData.IsValid)
			{
				Debug.LogWarning("The persistentData is not valid: " + persistentData);
				return null;
			}
			if (listController.Exists(c => c.PersistentData.Key == persistentData.Key))//在已创建的Controller中查找相同的Key是否已存在，保证同时只存在一个Key
			{
				Debug.LogWarning($"Key [{persistentData.Key}] already exists");
				return null;
			}

			IPersistentController controller = persistentControllerFactory.Create(persistentData, controllerOption);
			return controller;
		}
	}
}