#if UNITY_EDITOR
using System;
using System.Linq;
using System.Text;
using Threeyes.Persistent;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
namespace Threeyes.Editor
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(PersistentDataBase), true)]//editorForChildClasses
	public class InspectorView_PersistentData : InspectorViewSyncWithHierarchyBase
	{
		private PersistentDataBase _target;
		private void OnEnable()
		{
			if (target == null)
				return;
			if (target is PersistentDataBase)
				_target = (PersistentDataBase)target;
			else
				Debug.LogError(target.name + " (" + target.GetType() + ") is not type of " + nameof(PersistentDataBase));
		}
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			if (!_target)
				return;
			if (_target is PersistentData_Enum)//ToUpdate:后期直接隐藏string类型的值，可以通过NaughtAttribute或不同的继承实现
				OnInspectorGUI_Enum(_target as PersistentData_Enum);

			DrawHelpBox();
		}

		StringBuilder sbError = new StringBuilder();
		private void DrawHelpBox()
		{
			//当配置非法时，出现提示框
			sbError.Length = 0;
			_target.SetInspectorGUIHelpBox_Error(sbError);
			if (sbError.Length > 0)
				EditorGUILayout.HelpBox(sbError.ToString(), MessageType.Error);
		}

		public void OnInspectorGUI_Enum(PersistentData_Enum inst)
		{
			//Draw Enum
			DrawEnum(inst, "DefaultValue", (pd) => pd.DefaultValue, (pd, e) => pd.DefaultValue = e);
			DrawEnum(inst, "PersistentValue", (pd) => pd.PersistentValue);//Readonly
		}
		void DrawEnum(PersistentData_Enum inst, string label, Func<PersistentData_Enum, Enum> getter, UnityAction<PersistentData_Enum, Enum> setter = null)
		{
			var option = inst.DataOption;
			Type enumType = option.EnumType;
			if (enumType != null)
			{
				//PS：如果开发者更改了枚举定义名，会导致返回null，从而将值重置为默认值
				Enum lastValue = getter(inst);
				if (lastValue != null)
				{
					try
					{
						Enum result = option.UseFlag ? EditorGUILayout.EnumFlagsField(label, lastValue) : EditorGUILayout.EnumPopup(label, lastValue);//针对Flag有不同的绘制diamagnetic

						if (!lastValue.Equals(result))//更新值
						{
							Undo.RecordObject(target, "OnInspectorGUI_Enum");
							setter.Execute(inst, result);
						}
					}
					catch (Exception e)
					{
						Debug.LogError(e);//ToDelete
					}
				}
				else
				{
					//设置为首个默认值
					setter.Execute(inst, option.ListEnumValue.FirstOrDefault());
				}
			}
		}
	}
}
#endif