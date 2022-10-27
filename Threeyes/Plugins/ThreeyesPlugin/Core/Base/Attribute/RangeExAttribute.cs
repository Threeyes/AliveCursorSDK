using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
[AttributeUsage(AttributeTargets.Field)]
public class RangeExAttribute : PropertyAttribute
{
	public string MinPropertyName { get; private set; }
	public string MaxPropertyName { get; private set; }


	//ToAdd:增加获取最大值、最小值的方法，都是可选项

	/// <summary>
	/// Define the property to return min/max value
	///
	/// Format:
	/// -float Min{get;}
	/// 
	/// </summary>
	/// <param name="minMethodName"></param>
	/// <param name="maxMethodName"></param>
	public RangeExAttribute(string minMethodName = null, string maxMethodName = null)
	{
		MinPropertyName = minMethodName;
		MaxPropertyName = maxMethodName;
	}


	public float? GetMinValue(object obj)
	{
		return GetValueFunc(obj, MinPropertyName);
	}
	public float? GetMaxValue(object obj)
	{
		return GetValueFunc(obj, MaxPropertyName);
	}
	static float? GetValueFunc(object obj, string propertyName)
	{
		if (obj == null)
			return null;

		object result = ReflectionTool.GetPropertyValue(obj, propertyName, null);
		if (result != null)
			return (float)result;
		return null;
	}

	//ToUpdate
	public MethodInfo GetCallbackMethodInfo(FieldInfo fieldInfoAttribute, string methodName)
	{
		if (fieldInfoAttribute == null || methodName.IsNullOrEmpty()) return null;

		Type objType = fieldInfoAttribute.DeclaringType;
		MethodInfo methodInfo = objType.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		Type paramType = fieldInfoAttribute.FieldType;//该Field的类型就是对应的参数类型
		if (methodInfo != null && paramType != null)
		{
			ParameterInfo[] arrParameterInfo = methodInfo.GetParameters();
			if (methodInfo.ReturnType == typeof(float) && arrParameterInfo.Length == 0)
				return methodInfo;
		}
		Debug.LogError($"Can't find matched method [{methodName}] in {objType.FullName}!");
		return null;
	}
}
