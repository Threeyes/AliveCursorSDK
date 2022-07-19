using System;
namespace Threeyes.Data
{

	public static class DataTool
    {
        /// <summary>
        /// 获取类型的基类，常用于后续返回对应的DataOption
        /// </summary>
        /// <param name="originType"></param>
        /// 
        /// <returns></returns>
        public static Type GetBaseType(Type originType)
        {
            Type basicType = originType;

            ///针对Enum及对应的enum定义：
            /// 1.当originType是具体枚举定义：IsEnum返回true
            /// 2.当originType是Enum时，IsEnum返回false，此时本类型就是目标值(原理：内部调用了IsSubclassOf，This method also returns false if c and the current Type are equal.（https://docs.microsoft.com/en-us/dotnet/api/system.type.issubclassof?view=net-6.0）
            if (originType.IsEnum)
            {
                basicType = typeof(Enum);
            }
            return basicType;
        }
    }
}