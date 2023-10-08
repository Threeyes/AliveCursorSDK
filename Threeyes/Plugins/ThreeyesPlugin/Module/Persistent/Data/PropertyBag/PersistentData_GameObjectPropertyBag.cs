using System.Collections;
using System.Collections.Generic;
using Threeyes.Data;
using UnityEngine;
namespace Threeyes.Persistent
{
    /// <summary>
    /// PS:该类暂时不继承PersistentDataComplexBase，因为是直接针对PersistentValue，且不允许RuntimeEditor修改所以不需要克隆。后期考虑如何整合
    /// </summary>
    public class PersistentData_GameObjectPropertyBag : PersistentDataBase<GameObjectPropertyBag, GameObjectPropertyBagEvent>
        , IFilePathModifierHolder
    {
        public virtual FilePathModifier FilePathModifier { get { if (filePathModifier_PD == null) filePathModifier_PD = new FilePathModifier_PD(this); return filePathModifier_PD; } set { Debug.LogError("This property can't set!"); /*暂时不允许设置，避免用户魔改*/} }
        private FilePathModifier_PD filePathModifier_PD;

       

#if UNITY_EDITOR
        //——MenuItem——
        static string instName = "GOPropertyBagPD ";
        [UnityEditor.MenuItem(strMenuItem_Root_PropertyBag + "GameObject", false, intBasicMenuOrder)]
        public static void CreateInst()
        {
            Editor.EditorTool.CreateGameObjectAsChild<PersistentData_GameObjectPropertyBag>(instName);
        }

        //——Hierarchy GUI——
        public override string ShortTypeName { get { return "GOPropertyBag"; } }
#endif
    }
}