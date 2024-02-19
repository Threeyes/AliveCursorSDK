using System.Collections;
using System.Collections.Generic;
using Threeyes.Data;
using Threeyes.Persistent;
using UnityEngine;
using UnityEngine.Events;
//namespace Threeyes.Persistent
namespace Threeyes.RuntimeSerialization

{
    /// <summary>
    /// PS:该类暂时不继承PersistentDataComplexBase，因为是直接针对PersistentValue，且不允许RuntimeEditor修改所以不需要克隆。后期考虑如何整合
    /// 
    /// Todo:
    /// -提供一个可选的RuntimeSerialization_GameObject字段，用于直接针对某个根物体进行序列化/反序列化，适用于任意
    /// -
    /// 
    /// 【Wasted原因】：
    ///     -还是由RTS_GO管理序列化事项，因为便于自定义数据的序列化/反序列化格式，且能够避免PD_GameObjectPropertyBag与Json耦合
    ///     -放在这个文件夹的原因：Persistent无法获知Threeyes.RuntimeSerialization命名空间
    ///     -查看string格式的GameObjectPropertyBag方式：
    ///         -- 通过VSCode+BeautifyJson，按【Ctrl+Shift+P/BeautifyJson/回车/回车】来格式化字符类型Json以便阅读【Warning：该插件不完善，使用按下Ctrl+Shift+J格式化后会自动保存源文件，导致Unity无法正确读取。可以检查完成后按撤销还原】
    /// </summary>
    [System.Obsolete("Use PersistentData_String instead!", false)]
    public class PersistentData_GameObjectPropertyBag : PersistentDataBase<GameObjectPropertyBag, GameObjectPropertyBagEvent, DataOption_GameObjectPropertyBag>
    {
        public override GameObjectPropertyBag ValueToSaved
        {
            get
            {
                return base.ValueToSaved;
            }
        }

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

    #region Define
    [System.Serializable]
    public class GameObjectPropertyBagEvent : UnityEvent<GameObjectPropertyBag> { }
    #endregion
}