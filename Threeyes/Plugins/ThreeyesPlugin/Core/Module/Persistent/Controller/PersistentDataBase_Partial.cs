using System.Linq;
using System.Text;
using UnityEngine;
namespace Threeyes.Persistent
{
    /// <summary>
    /// 功能：
    /// 1.将于Controller类的相关方法放到该分布类中，保证PD主类不知道Controller的存在，便于分割
    /// </summary>
    public partial interface IPersistentData
    {
        /// <summary>
        /// Is the Key exist once in PersistentManager
        /// </summary>
        bool IsSole { get; }
    }
    public abstract partial class PersistentDataBase : MonoBehaviour
    {
        public bool IsSole
        {
            get
            {
                var manager = transform.GetComponentInParent<PersistentControllerManagerBase>();
                if (manager && manager.ListComp.FindAll(pd => pd.Key == key).Count() > 1)
                    return false;
                return true;
            }
        }

#if UNITY_EDITOR
        partial void SetHierarchyGUIPropertyEx(StringBuilder sB)
        {
            if (!IsSole)
            {
                sB.WrapWarningRichText();
            }
        }

        partial void SetInspectorGUIHelpBox_ErrorEx(StringBuilder sB)
        {
            if (!IsSole)//Key在同一PDManager下重复
            {
                sB.Append("Key already exists under the same PersistentControllerManager! ");
                sB.Append("\r\n");
            }
        }
#endif

    }
}