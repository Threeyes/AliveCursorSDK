using System.Collections;
using System.Collections.Generic;
using Threeyes.Core;
using UnityEngine;
///指定可激活的宏定义
///-测试新宏定义是否有效：宏定义名称随意写，看程序能否正常添加，好处是不会影响使用了正式宏定义的代码
///PS:
///-以下代码不能放到命名空间中
[assembly: OptionalDependency("NaughtyAttributes.INaughtyAttribute", "USE_NaughtyAttributes", "Threeyes.Action")]
[assembly: OptionalDependency("DG.Tweening.Core.DOTweenComponent", "USE_DOTween", "Threeyes.Action")]
[assembly: OptionalDependency("Newtonsoft.Json.JsonConvert", "USE_JsonDotNet", "Threeyes.Action")]

//[assembly: OptionalDependency("Threeyes.Action.SOActionBase", "USE_Threeyes_Action")]//ToAdd：自身的宏定义，后续有需要可以激活
namespace Threeyes.Action
{
}