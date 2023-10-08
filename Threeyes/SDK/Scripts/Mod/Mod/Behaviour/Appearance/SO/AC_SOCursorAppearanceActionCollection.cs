using NaughtyAttributes;
using Threeyes.Action;
using UnityEngine;
/// <summary>
/// Manage all Cursor Appearance Action
/// </summary>
[CreateAssetMenu(menuName = AC_EditorDefinition.AssetMenuPrefix_Root_Mod_Behaviour_Appearance + "ActionCollection", fileName = "CursorAppearanceActionCollection", order = 0)]
public class AC_SOCursorAppearanceActionCollection : SOCollectionBase<AC_SystemCursorAppearanceType, SOActionBase>
{
	[Header("PS: Only set these value if you want the cursor type to have extra motion (eg: endless twinkle for AppStarting/Wait)")]
	[Expandable]
	public SOActionBase soActionNo;
	[Expandable]
	public SOActionBase soActionArrow;
	[Expandable]
	public SOActionBase soActionAppStarting;
	[Expandable]
	public SOActionBase soActionCrosshair;
	[Expandable]
	public SOActionBase soActionHelp;
	[Expandable]
	public SOActionBase soActionIBeam;
	[Expandable]
	public SOActionBase soActionSizeAll;
	[Expandable]
	public SOActionBase soActionSizeNESW;
	[Expandable]
	public SOActionBase soActionSizeNS;
	[Expandable]
	public SOActionBase soActionSizeNWSE;
	[Expandable]
	public SOActionBase soActionSizeWE;
	[Expandable]
	public SOActionBase soActionUpArrow;
	[Expandable]
	public SOActionBase soActionWait;
	[Expandable]
	public SOActionBase soActionHand;
	[Expandable]
	public SOActionBase soActionPen;
	[Expandable]
	public SOActionBase soActionScrollNS;
	[Expandable]
	public SOActionBase soActionScrollWE;
	[Expandable]
	public SOActionBase soActionScrollAll;
	[Expandable]
	public SOActionBase soActionScrollN;
	[Expandable]
	public SOActionBase soActionScrollS;
	[Expandable]
	public SOActionBase soActionScrollW;
	[Expandable]
	public SOActionBase soActionScrollE;
	[Expandable]
	public SOActionBase soActionScrollNW;
	[Expandable]
	public SOActionBase soActionScrollNE;
	[Expandable]
	public SOActionBase soActionScrollSW;
	[Expandable]
	public SOActionBase soActionScrollSE;
	[Expandable]
	public SOActionBase soActionArrowCD;

	//ToAdd:如果后续需要增加自定义光标，可以增加一个SOActionHolder类（用于指定SOAction和对应的Enum），然后存到List中
	public override SOActionBase this[AC_SystemCursorAppearanceType en]
	{
		get
		{
			switch (en)
			{
				case AC_SystemCursorAppearanceType.None: return null;
				case AC_SystemCursorAppearanceType.No: return soActionNo;
				case AC_SystemCursorAppearanceType.Arrow: return soActionArrow;
				case AC_SystemCursorAppearanceType.AppStarting: return soActionAppStarting;
				case AC_SystemCursorAppearanceType.Crosshair: return soActionCrosshair;
				case AC_SystemCursorAppearanceType.Help: return soActionHelp;
				case AC_SystemCursorAppearanceType.IBeam: return soActionIBeam;
				case AC_SystemCursorAppearanceType.SizeAll: return soActionSizeAll;
				case AC_SystemCursorAppearanceType.SizeNESW: return soActionSizeNESW;
				case AC_SystemCursorAppearanceType.SizeNS: return soActionSizeNS;
				case AC_SystemCursorAppearanceType.SizeNWSE: return soActionSizeNWSE;
				case AC_SystemCursorAppearanceType.SizeWE: return soActionSizeWE;
				case AC_SystemCursorAppearanceType.UpArrow: return soActionUpArrow;
				case AC_SystemCursorAppearanceType.Wait: return soActionWait;
				case AC_SystemCursorAppearanceType.Hand: return soActionHand;

				case AC_SystemCursorAppearanceType.Pen: return soActionPen;
				case AC_SystemCursorAppearanceType.ScrollNS: return soActionScrollNS;
				case AC_SystemCursorAppearanceType.ScrollWE: return soActionScrollWE;
				case AC_SystemCursorAppearanceType.ScrollAll: return soActionScrollAll;
				case AC_SystemCursorAppearanceType.ScrollN: return soActionScrollN;
				case AC_SystemCursorAppearanceType.ScrollS: return soActionScrollS;
				case AC_SystemCursorAppearanceType.ScrollW: return soActionScrollW;
				case AC_SystemCursorAppearanceType.ScrollE: return soActionScrollE;
				case AC_SystemCursorAppearanceType.ScrollNW: return soActionScrollNW;
				case AC_SystemCursorAppearanceType.ScrollNE: return soActionScrollNE;
				case AC_SystemCursorAppearanceType.ScrollSW: return soActionScrollSW;
				case AC_SystemCursorAppearanceType.ScrollSE: return soActionScrollSE;
				case AC_SystemCursorAppearanceType.ArrowCD: return soActionArrowCD;
				default:
					Debug.LogError(en + " Not Define!");
					return null;
			};
		}
	}
}