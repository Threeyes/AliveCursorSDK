using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// 适用于提前摆放在场景中并标识正确/错误的选项（如果提供3个子类，其中一个的isDesire为true）
/// 适用于UI及3D物体
/// </summary>
public class SelectableElementBase : MonoBehaviour
{
    public bool isDesire = false;//是否想要的物体

    public SOTipsInfo sOTipsInfoCorrect;

    public SOTipsInfo sOTipsInfoWrong;

    public UnityEvent onSelect;
    public BoolEvent onSelectCorrentWrong;

    ISelectableElementReceiver selectableItemReceiver;
    void Awake()
    {
        //获取ISelectableElementReceiver
        transform.ForEachParent(
          (tf) =>
          {
              if (selectableItemReceiver == null)
              {
                  selectableItemReceiver = tf.GetComponent<ISelectableElementReceiver>();
              }
          }, false
          );
    }

    [ContextMenu("TestSelect")]
    public void TestSelect()
    {
        if (isDesire)
            selectableItemReceiver.OnSelectCorrect(this, sOTipsInfoCorrect);
        else
            selectableItemReceiver.OnSelectWrong(this, sOTipsInfoWrong);

        onSelect.Invoke();
    }

    public void OnSelect()
    {
        if (selectableItemReceiver == null)
        {
            Debug.LogError("selectableItemReceiver is null!");
        }
        if (!selectableItemReceiver.CanSelect)
            return;

        if (isDesire)
            selectableItemReceiver.OnSelectCorrect(this, sOTipsInfoCorrect);
        else
            selectableItemReceiver.OnSelectWrong(this, sOTipsInfoWrong);


        //延后到点击按键后再调用
        onSelect.Invoke();
    }
}

public interface ISelectableElementReceiver
{
    /// <summary>
    /// 当前能否选择物体（如上层UI菜单弹出时，不可选择该物体）
    /// </summary>
    bool CanSelect { get; }
    void OnSelectCorrect(SelectableElementBase selectableElement, SOTipsInfo sOTipsInfo = null);
    void OnSelectWrong(SelectableElementBase selectableElement, SOTipsInfo sOTipsInfo);

}