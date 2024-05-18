using System;
using System.Diagnostics;
using Threeyes.Core;
using UnityEngine;
namespace Threeyes.Core
{
    /// <summary>
    /// 定义项目依赖的库及对应的宏定义
    /// 
    /// Ref：【ConditionalCompilationUtility】：https://github.com/Unity-Technologies/conditionalcompilationutility
    /// </summary>

    [Conditional("UNITY_CCU")]                                    // | This is necessary for CCU to pick up the right attributes
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class OptionalDependencyAttribute : Attribute        // | Must derive from System.Attribute
    {
        public string dependentClass;//Required field specifying the fully qualified dependent class（指定类名或接口名，包括命名空间（即FullName，如Threeyes.EventPlayer.EventPlayer）)（与Asmdef文件属性无关）
        public string define;//Required field specifying the define to add（对应的宏定义）

        public string usedBy;//Which library is being used by（使用该库的对应库名）（类似PackageMananger-Dependency）
        public OptionalDependencyAttribute(string dependentClass, string define, string usedBy = "")
        {
            this.dependentClass = dependentClass;
            this.define = define;
            this.usedBy = usedBy;
        }
    }
}
