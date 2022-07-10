using System;
using UnityEngine;
//using Threeyes.EventPlayer;//Todelete，通过预制物制作
#if UNITY_EDITOR
using UnityEditor;
#endif

#if USE_VRTK
using VRTK;
#endif


/// <summary>
/// 自动设置物体的EP Group、被射线Hover的属性
/// </summary>
public class AutoSetSelectable_VR : MonoBehaviour
{
#if UNITY_EDITOR

    /// <summary>
    /// 
    /// </summary>
    public bool isPointerHover = false;
    //public void AutoSet()
    //{
    //    List<VIU_InteractableObject_UnityEvents> listUE = GameObject.FindObjectsOfType<VIU_InteractableObject_UnityEvents>().ToList();

    //    foreach (var ue in listUE)
    //    {
    //        SetTarget(ue);
    //    }
    //}

    private static void SetTarget(Transform tf)
    {
        if (tf.GetComponent<AutoSet_Exclude>())
            return;

        #region UnityEvent
        Transform tfEPGroup = tf.AddChildOnce("EP Group");
        Transform tfEPGUse = tfEPGroup.AddChildOnce("EPG Use");
        //EventPlayer epUse = tfEPGUse.AddComponentOnce<EventPlayer>();
        //epUse.IsGroup = true;
        //epUse.IsPlayOnce = true;
        #endregion

#if USE_VRTK&&VRTK_VERSION_3_1_0_OR_NEWER

        VRTK.VRTK_InteractableObject vRTK_InteractableObject = tf.AddComponentOnce<VRTK.VRTK_InteractableObject>();//用于Hover高亮
        VR_InteractableObject_UnityEvents ue = tf.AddComponentOnce<VR_InteractableObject_UnityEvents>();
        ue.OnHoverUnHover.AddPersistentListenerOnce(vRTK_InteractableObject, "ToggleHighlight");//调用VRTK的高亮方法
        //epUse.RegisterPersistentListenerOnce(ue.OnUse, EventPlayer_EventType.Play);


#if USE_VRTK

        vRTK_InteractableObject.isGrabbable = false;
        vRTK_InteractableObject.isUsable = true;
        vRTK_InteractableObject.pointerActivatesUseAction = true;

#endif

#if USE_VIU

        //设置一体机的射线出发脚本
        var io = ue.AddComponentOnce<VIU_InteractableObject>();
        io.isGrabbable = false;
        io.holdButtonToGrab = false;
        io.isUsable = true;
        io.pointerActivatesUseAction = true;

#endif

#endif

    }


    [ContextMenu("AutoSetCurSelect")]
    public void AutoSetCurSelectInst()
    {
        AutoSetCurSelect();
    }

    [MenuItem(EditorDefinition.TopMenuItemPrefix + "VR AutoSetCurSelect  #%q")]
    public static void AutoSetCurSelect()
    {
        GameObject curSelect = Selection.activeGameObject;
        if (curSelect)
        {
            SetTarget(curSelect.transform);
        }
    }
#endif
}
