using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
namespace Threeyes.Core
{
    /// <summary>
    /// For methods without parameters
    /// 
    /// PS:
    /// -按照UnityEvent的方式命名，无参不需要带后缀
    /// </summary>
    public class ReflectionMethodHolder : ReflectionMethodHolderBase
    {
        public override bool IsDesireMethod(MethodInfo methodInfo)
        {
            //Method format: void Method();//无返回值，无参数
            ParameterInfo[] parameterInfos = methodInfo.GetParameters();
            return parameterInfos.Length == 0 && methodInfo.ReturnType == typeof(void);
        }
    }
}