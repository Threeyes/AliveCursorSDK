using UnityEngine;
using Threeyes.Data;

namespace Threeyes.Persistent
{
	/// <summary>
	///
	/// ToAdd:
	/// -Add HDR support for default/persistent value
	/// </summary>
	public class PersistentData_Color : PersistentDataBase<Color, ColorEvent, DataOption_Color>
	{

		private void Reset()
		{
			defaultValue = Color.white;
		}

#if UNITY_EDITOR

		//——MenuItem——
		static string instName = "ColorPD ";
		[UnityEditor.MenuItem(strMenuItem_Root_Basic + "Color", false, intBasicMenuOrder + 8)]
		public static void CreateInst()
		{
			Editor.EditorTool.CreateGameObjectAsChild<PersistentData_Color>(instName);
		}

		//——Hierarchy GUI——
		public override string ShortTypeName { get { return "Color"; } }

#endif
	}
}