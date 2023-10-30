using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Threeyes.Data
{
	/// <summary>
	/// 功能：
	/// -指定额外的DataOption作为可设置参数
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class ColorUsageExAttribute : PropertyExAttribute
	{
		public string UseAlphaMemberName { get; private set; }
		public string UseHdrMemberName { get; private set; }
		public string DataOptionMemberName { get; private set; }

		public ColorUsageExAttribute(string useAlphaMemberName = null, string useHdrMemberName = null, string dataOptionMemberName = null)
		{
			UseAlphaMemberName = useAlphaMemberName;
			UseHdrMemberName = useHdrMemberName;
			DataOptionMemberName = dataOptionMemberName;
		}


		public bool? GetUseAlphaValue(object obj)
		{
			if (DataOptionMemberName.NotNullOrEmpty())
			{
				object dataOption = GetMemberValue<DataOption_Color>(obj, DataOptionMemberName);

				if (dataOption is DataOption_Color dataOption_Color)
				{
					return dataOption_Color.UseAlpha;
				}
				else
				{
					return null;
				}
			}

			var value = GetMemberValue<bool>(obj, UseAlphaMemberName);
			if (value != null)
				return (bool)value;
			return null;
		}
		public bool? GetUseHdrValue(object obj)
		{
			if (DataOptionMemberName.NotNullOrEmpty())
			{
				object dataOption = GetMemberValue<DataOption_Color>(obj, DataOptionMemberName);

				if (dataOption is DataOption_Color dataOption_Color)
				{
					return dataOption_Color.UseHDR;
				}
				else
				{
					return null;
				}
			}
			var value = GetMemberValue<bool>(obj, UseHdrMemberName);
			if (value != null)
				return (bool)value;
			return null;
		}

	}
}