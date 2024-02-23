using System.Collections;
using System.Collections.Generic;
using Threeyes.Core;
using UnityEngine;
namespace Threeyes.Common
{
    [CreateAssetMenu(menuName = EditorDefinition_Common.AssetMenuPrefix_SO_Common + "PrefabInfoGroup", fileName = "PrefabInfoGroup")]
    public class SOPrefabInfoGroup : SOGroupBase<SOPrefabInfo>
    {
        public string remark;//开发者内部注释
        [Space]
        public string title;//目录名
    }
}