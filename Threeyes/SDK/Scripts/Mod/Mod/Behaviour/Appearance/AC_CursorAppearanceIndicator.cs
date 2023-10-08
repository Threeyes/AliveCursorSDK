using NaughtyAttributes;
using Threeyes.Action;
using UnityEngine;
/// <summary>
/// Display system cursor info
/// </summary>
[AddComponentMenu(AC_EditorDefinition.ComponentMenuPrefix_Root_Mod_Behaviour_Appearance + "AC_CursorAppearanceIndicator", 2)]
public class AC_CursorAppearanceIndicator : AC_CursorAppearanceBehaviourCollection
	, IAC_CursorState_ChangedHandler
	, IShowHide
{
	#region Property & Field
	[Foldout(foldoutName_ActionTarget)] public GameObject actionTargetContentParent;//Content's parent, will be self if null

	[Foldout(foldoutName_SOAction)] [Expandable] public SOActionBase soActionContentParent;//action for actionTargetContentParent gameobject, mianly for Show/Hide
	[Foldout(foldoutName_SOAction)] [Expandable] public AC_SOCursorAppearanceInfoCollection soCursorAppearanceInfoCollection;//CursorAppearanceInfo to match current systemCursorAppearanceType

	[Foldout(foldoutName_UnityEvent)] public BoolEvent onShowHide;
	[Foldout(foldoutName_UnityEvent)] public TextureEvent onAppearanceTextureChanged;//Triggered when CursorAppearanceInfo changed
	[Foldout(foldoutName_UnityEvent)] public ColorEvent onAppearanceColorChanged;//Triggered when CursorAppearanceInfo changed


	//#Run Time
	protected AC_SystemCursorAppearanceType lastCursorAppearanceType;
	protected AC_SOCursorAppearanceInfo lastSOCursorAppearanceInfo;

	#endregion

	#region Unity Method
	protected virtual void OnEnable()
	{
		//Init: Force Hide on active
		ShowFunc(false);
		isShowing = false;
	}
	protected virtual void OnDisable()
	{
		Hide();
	}
	#endregion

	#region Override
	public virtual void OnCursorStateChanged(AC_CursorStateInfo cursorStateInfo)
	{
		if (cursorStateInfo.cursorState != AC_CursorState.Working)//Only show on Working(Which cursor will follow system cursor and will be able to change appearance)
			Hide();
	}
	public override void OnSystemCursorAppearanceChanged(bool isSystemCursorShowing, AC_SystemCursorAppearanceInfo systemCursorAppearanceInfo)
	{
		if (systemCursorAppearanceInfo.stateChange == AC_SystemCursorAppearanceInfo.StateChange.Exit)
			return;
		//Todo:调用Action移动到父类，不需要判断状态
		///ToUpdate:
		///1.增加枚举，确定是否只有在Working时可用（名称为ValidMode）
		///2.将以下判断放到InvokeBehaviour中
		if (AC_ManagerHolder.StateManager.CurCursorState != AC_CursorState.Working)//Don't show on Working state (PS:Get this field at runtime because may not listion to OnCursorStateChanged on time)
			return;
		if (soCursorAppearanceInfoCollection == null)
		{
			Debug.LogError(name + "'s cursorAppearanceInfoGroup is null!");
			return;
		}
		AC_SystemCursorAppearanceType curCursorAppearanceType = systemCursorAppearanceInfo.systemCursorAppearanceType;
		if (isSystemCursorShowing /*&& curCursorAppearanceType != AC_SystemCursorAppearanceType.None*/)//忽略系统光标显示或None的情况（因为这时候系统光标会显示，AC会隐藏）
		{
			AC_SOCursorAppearanceInfo soCursorAppearanceInfo = soCursorAppearanceInfoCollection[curCursorAppearanceType];//查找是否包含待显示的类型
			if (soCursorAppearanceInfo != null && soCursorAppearanceInfo != lastSOCursorAppearanceInfo)
			{
				ShowCursor(curCursorAppearanceType);
				lastSOCursorAppearanceInfo = soCursorAppearanceInfo;
			}
			else//AppearanceInfo库中不存在该光标类型（如自定义光标）：隐藏
			{
				Hide();
			}
		}
		else//系统光标被隐藏 || 光标类型未知：隐藏
		{
			Hide();
		}
		lastCursorAppearanceType = curCursorAppearanceType;
	}

	protected virtual void LoadCursorFunc(AC_SystemCursorAppearanceType cursorAppearanceType)
	{
		soActionCollection?[cursorAppearanceType]?.Enter(true, GetActionTarget(cursorAppearanceType));

		//更新信息
		AC_SOCursorAppearanceInfo soCursorAppearanceInfo = soCursorAppearanceInfoCollection[cursorAppearanceType];
		if (soCursorAppearanceInfo)
		{
			onAppearanceTextureChanged.Invoke(soCursorAppearanceInfo.texture);
			onAppearanceColorChanged.Invoke(soCursorAppearanceInfo.color);
		}
	}
	#endregion

	#region Inner Method
	protected virtual GameObject GetContentParentActionTarget() { return actionTargetContentParent ?? gameObject; }//ToDo:移动到父物体
	protected virtual void ShowCursor(AC_SystemCursorAppearanceType systemCursorAppearanceType)
	{
		Show();//Show Root first
		if (lastSOCursorAppearanceInfo.NotNull())
			UnloadLastCursorFunc();//需要先退出上次光标的Action并重置其Tween，否则如果其Tween会影响下一SystemCursor显示（如Loop）
		LoadCursorFunc(systemCursorAppearanceType);//显示当前光标
	}
	protected virtual void UnloadLastCursorFunc()
	{
		soActionCollection?[lastCursorAppearanceType]?.Enter(false, GetActionTarget(lastCursorAppearanceType));//通常会停止该State的行为（如停止旋转）
	}
	#endregion

	#region Override IShowHideInterface

	public bool IsShowing { get { return isShowing; } set { isShowing = value; } }
	public bool isShowing = false;

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
		if (isShow == isShowing)
			return;

		ShowFunc(isShow);
		IsShowing = isShow;
	}
	protected virtual void ShowFunc(bool isShow)
	{
		onShowHide.Invoke(isShow);

		if (!isShow)
		{
			if (lastSOCursorAppearanceInfo.NotNull())
			{
				UnloadLastCursorFunc();//停止Action
				lastSOCursorAppearanceInfo = null;//重置数据
			}
		}

		//使用Action处理父类的显隐（而不是直接隐藏该物体），便于用户自行实现Alpha、Scale等效果
		soActionContentParent?.Enter(isShow, GetContentParentActionTarget());
	}

	#endregion

	#region Editor Method
#if UNITY_EDITOR
	//——MenuItem——
	static string instName = "UICAB_Collection ";
	[UnityEditor.MenuItem(AC_EditorDefinition.HierarchyMenuPrefix_Root_Mod_Behaviour_Appearance + "UICursorAppearanceBehaviourCollection", false, 2)]
	public static void CreateUIInst()
	{
		Threeyes.Editor.EditorTool.CreateGameObjectAsChild<AC_CursorAppearanceIndicator>(instName);
	}
#endif
	#endregion
}
