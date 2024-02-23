using System.Collections;
using System.Collections.Generic;
using Threeyes.Core;
using UnityEngine;
namespace Threeyes.Data
{
    public abstract class PropertyExAttribute : PropertyAttribute
	{
		protected static object GetMemberValue<TValue>(object obj, string fieldOrpropertyName)
		{
			if (obj == null || fieldOrpropertyName.IsNullOrEmpty())
				return null;
			return ReflectionTool.GetFieldOrPropertyValue(obj, fieldOrpropertyName, targetType: typeof(TValue));//Ensure the  type of Member is correct
		}
	}
}