using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Threeyes.Editor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
#if USE_VRTK
using VRTK;
#endif
/// <summary>
/// 点击Menu键，弹出场景菜单
/// 参考：
/// 030_Controls_RadialTouchpadMenu.unity
/// VRTK_RadialMenuController组件
/// Controller_Menu组件
/// 
/// Warning：
/// 1.Quest的摇杆需要按下才能选中下一个
/// </summary>
public class VR_UISceneMenuController : UISceneMenuController
{
    #region Property & Field

    [Header("VR Controller Setting")]
    [Range(0, 1)]
    public float baseHapticStrength = 0.2f;//按下按键时的震动强度
    public MenuButtonFunction menuButtonFunction = MenuButtonFunction.OpenSceneMenu;//菜单按钮的作用


    #region Wasted
    //#ToDelete:统一设置到ListData
    [Header("#以下引用已不可用！请使用标准的属性代替")]
    [System.Obsolete("Use ListData Instead")]
    public List<SOSceneInfo> listSceneInfo = new List<SOSceneInfo>();
    //旧版参数
    [System.Obsolete("Use tfContentParent Instead")]
    public Transform tfButtonPanel;
    [System.Obsolete("Use preUIElement Instead")]
    public GameObject preSceneButton;
    #endregion

    #endregion

#if UNITY_EDITOR
    void OnValidate()
    {
#pragma warning disable CS0618
        //针对旧版本引用进行更新
        if (EditorVersionUpdateTool.TransferObject(this, ref tfButtonPanel, ref tfContentParent))
        {
            //Debug.Log("自动移动将旧版的tfButtonPanel");
        }
        if (EditorVersionUpdateTool.TransferObject(this, ref preSceneButton, ref preUIElement))
        {
            //Debug.Log("自动移动将旧版的preSceneButton");
        }
        if (EditorVersionUpdateTool.TransferList(this, ref listSceneInfo, ref listData))
        {
            //Debug.Log("自动将旧版的listSceneInfo数据移动到ListData中");
        }
#pragma warning restore CS0618
    }
#endif

#if USE_VRTK

    protected VRTK_ControllerEvents controllerRef;
    void MenuPressed(object sender, ControllerInteractionEventArgs e)
    {
        OnMenuPress();
    }
    private void TriggerClicked(object sender, ControllerInteractionEventArgs e)
    {
        if (!isShowing)
            return;

        ClickCurrentButton();
        VRInterface.Viberation(controllerRef, baseHapticStrength * 0.8f);
    }

    private void TouchpadPressed(object sender, ControllerInteractionEventArgs e)
    {
        if (!isShowing)
            return;

        SetDelta(e.touchpadAxis.y < 0 ? +1 : -1);
        VRInterface.Viberation(controllerRef, baseHapticStrength * 0.5f);
    }

#endif

    #region Public Func

    /// <summary>
    /// 将点击菜单键的功能改为重载场景
    /// </summary>
    public void SetMenuButtonFunctionToReloadScene()
    {
        menuButtonFunction = MenuButtonFunction.ReloadScene;
    }

    /// <summary>
    /// 根据传入值的正/负决定选择上一个/下一个
    /// </summary>
    /// <param name="delta"></param>
    public void SelectDelta(float delta)
    {
        if (!IsShowing)
            return;

        //根据传入值，选择上一个/下一个（反值）
        int sign = (int)Mathf.Sign(delta);
        SetDelta(-sign);
    }

    public void OnMenuPress()
    {
        switch (menuButtonFunction)
        {
            case MenuButtonFunction.OpenSceneMenu:
                //因为是方向键负责震动，体验不好，因此按下菜单键时暂不震动
                //AttemptHapticPulse(baseHapticStrength);
                Show(!isShowing);
                break;
            case MenuButtonFunction.ReloadScene:
                EnvironmentManager.ReloadSceneStatic();
                break;
        }
    }

    #endregion

    #region Override 

    protected override void Start()
    {
#if USE_VRTK
        controllerRef = GetComponentInParent<VRTK_ControllerEvents>();
        if (controllerRef)//避免用作其他用途
        {
            controllerRef.ButtonTwoPressed += MenuPressed; //菜单键
            controllerRef.TouchpadPressed += TouchpadPressed;//方向键
            controllerRef.TriggerClicked += TriggerClicked;//Trigger键
        }
#endif

        base.Start();
    }

    /// <summary>
    /// 选中默认的按键
    /// </summary>
    void SelectDefaultButton()
    {
        if (listUIElement.Count > 0)
        {
            //优先选中与当前的场景相对应的按钮
            Scene curScene = SceneManager.GetActiveScene();
            for (int i = 0; i != listUIElement.Count; i++)
            {
                if (listUIElement[i].data.buildName == curScene.name)
                {
                    Set(i);
                    return;
                }
            }

            //如果不匹配，选择第一个
            Set(0);
        }
    }


    #region Show/Hide

    protected override void ShowFunc(bool isShow)
    {
        base.ShowFunc(isShow);
        if (isShow)
        {
            StartCoroutine(IESelectDefaultButton());
        }
        else
            EventSystem.current.SetSelectedGameObject(null);//重置选择
    }

    IEnumerator IESelectDefaultButton()
    {
        yield return null;
        yield return null;

        //要等待UI初始化完成
        SelectDefaultButton();//显示后选中默认的按钮
    }
    #endregion

    #endregion

    #region Define

    public enum MenuButtonFunction
    {
        OpenSceneMenu,//打开场景按钮
        ReloadScene,//重载当前场景
    }

    #endregion
}
