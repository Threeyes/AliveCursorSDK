using Threeyes.Core;
using Threeyes.Core.Editor;
using Threeyes.Data;
namespace Threeyes.Persistent
{
    /// <summary>
    /// PS:���п�ö�ٵģ�int��string��Texture���ȶ�����ͨ�������ʵ��
    /// </summary>
    public class PersistentData_Option : PersistentDataBase<int, IntEvent, DataOption_OptionInfo>
    {
        #region Editor Method
#if UNITY_EDITOR

        //����MenuItem����
        static string instName = "OptionPD ";
        [UnityEditor.MenuItem(strMenuItem_Root_Basic + "Option", false, intBasicMenuOrder + 7)]
        public static void CreateInst()
        {
            EditorTool.CreateGameObjectAsChild<PersistentData_Option>(instName);
        }

        //����Hierarchy GUI����
        public override string ShortTypeName { get { return "Option"; } }

#endif
        #endregion

    }
}

