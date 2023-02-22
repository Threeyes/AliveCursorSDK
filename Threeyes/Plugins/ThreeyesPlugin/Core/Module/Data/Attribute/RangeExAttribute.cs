using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Threeyes.Data
{
	/// <summary>
	/// Attribute used to make a float or int variable in a script be restricted to a
	//     specific range.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class RangeExAttribute : PropertyExAttribute
	{
		public string MinMemberName { get; private set; }
		public string MaxMemberName { get; private set; }
		public string UseRangeMemberName { get; private set; }
		public string DataOptionMemberName { get; private set; }

		//ToAdd:增加获取最大值、最小值的方法，都是可选项

		/// <summary>
		/// Define the field/property to return range config
		///
		/// Format:
		/// -float Min{get;}
		/// 
		/// </summary>
		/// <param name="minMemberName"></param>
		/// <param name="maxMemberName"></param>
		/// <param name="useRangeMemberName"></param>
		/// <param name="dataOptionMemberName">DataOption_Float or DataOption_Int</param>
		public RangeExAttribute(string minMemberName = null, string maxMemberName = null, string useRangeMemberName = null, string dataOptionMemberName = null)
		{
			MinMemberName = minMemberName;
			MaxMemberName = maxMemberName;
			UseRangeMemberName = useRangeMemberName;
			DataOptionMemberName = dataOptionMemberName;
		}

		public float? GetMinValue(object obj)
		{
			if (DataOptionMemberName.NotNullOrEmpty())//Try get data from DataOption
			{
				object dataOption = GetDataOptionFunc(obj);
				if (dataOption is DataOption_Float dataOption_Float)
				{
					return dataOption_Float.MinValue;
				}
				else if (dataOption is DataOption_Int dataOption_Int)
				{
					return dataOption_Int.MinValue;
				}
				else
				{
					return null;
				}
			}

			var value = GetMemberValue<float>(obj, MinMemberName);
			if (value != null)
				return (float)value;
			return null;
		}
		public float? GetMaxValue(object obj)
		{
			if (DataOptionMemberName.NotNullOrEmpty())
			{
				object dataOption = GetDataOptionFunc(obj);
				if (dataOption is DataOption_Float dataOption_Float)
				{
					return dataOption_Float.MaxValue;
				}
				else if (dataOption is DataOption_Int dataOption_Int)
				{
					return dataOption_Int.MaxValue;
				}
				else
				{
					return null;
				}
			}

			var value = GetMemberValue<float>(obj, MaxMemberName);
			if (value != null)
				return (float)value;
			return null;
		}
		public bool? GetUseRangeValue(object obj)
		{
			if (DataOptionMemberName.NotNullOrEmpty())
			{
				object dataOption = GetDataOptionFunc(obj);
				if (dataOption is DataOption_Float dataOption_Float)
				{
					return dataOption_Float.UseRange;
				}
				else if (dataOption is DataOption_Int dataOption_Int)
				{
					return dataOption_Int.UseRange;
				}
				else
				{
					return null;
				}
			}

			var value = GetMemberValue<bool>(obj, UseRangeMemberName);
			if (value != null)
				return (bool)value;
			return null;
		}


		object GetDataOptionFunc(object obj)
		{
			if (obj == null || DataOptionMemberName.IsNullOrEmpty())
				return null;

			return ReflectionTool.GetFieldOrPropertyValue(obj, DataOptionMemberName);//Not sure the type of DataOption (float or int)
		}
	}
}