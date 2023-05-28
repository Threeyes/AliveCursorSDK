using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Threeyes.Data;
using System;
#if USE_NaughtyAttributes
using NaughtyAttributes;
#endif
namespace Threeyes.Persistent
{

	/// <summary>
	///
	///
	/// ToDo:
	/// -应该从PersistentData_SO中共用大量参数及父类，减少RuntimeEdit得工作量
	///
	/// Warning:
	/// -Type.GetType只能获取已知的类型，因此针对Mod定义的未知的类型，或命名空间外的类型无效（解决办法：搜索：type.gettype from another assembly，结果是建议带上Assembly）
	/// -问题是UMod的包，其Assembly名是自定义且可变的（通过Test_脚本可打印，结果如：umod-compiled-86a7224b-13e3-4533-a8f8-1b129c623f9c），要使用该类，只能在运行时组合该objectTypeFullName，或者使用某种Action，让其进行预先处理，替换占位符umod为对应命名空间
	/// </summary>
	public class PersistentData_Object : PersistentDataBase<object, ObjectEvent, DataOption_Object>
	{
		public override Type ValueType
		{
			get
			{
				return Type.GetType(dataOption.objectTypeFullName);
			}
		}

		/// <summary>
		/// Target Script instance in Scene or Asset window 
		/// </summary>
		public UnityEngine.Object target;

	}
}