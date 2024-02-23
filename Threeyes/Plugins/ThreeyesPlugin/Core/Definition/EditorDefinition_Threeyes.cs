using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threeyes.Core
{
    /// <summary>
    /// 以Threeyes为根的菜单目录名称
    /// </summary>
    public static class EditorDefinition_Threeyes
    {
        public const string Menu_Root = "Threeyes/";

        //——Asset Menu——
        public const string AssetMenuPrefix_Root = Menu_Root;
        public const string AssetMenuPrefix_Root_Module = AssetMenuPrefix_Root + "Module/";
    }
}