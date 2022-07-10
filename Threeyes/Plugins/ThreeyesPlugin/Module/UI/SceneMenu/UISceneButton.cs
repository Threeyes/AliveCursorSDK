using UnityEngine.UI;
/// <summary>
/// 场景菜单按钮
/// </summary>
public class UISceneButton : ElementBase<SOSceneInfo>
{
    public Button button;
    public Text text;

    private void Awake()
    {
        button.onClick.AddListener(OnClick);
    }


    public void OnClick()
    {
        ClickFunc();
    }

    bool isPress = false;
    void ClickFunc()
    {
        if (isPress)
            return;

        print("加载场景: " + text.text);

        switch (data.sceneInfoType)
        {
            case SOSceneInfo.SceneInfoType.Normal:
                data.LoadScene(); break;

            case SOSceneInfo.SceneInfoType.Reload:
                EnvironmentManager.ReloadSceneStatic(); break;
            case SOSceneInfo.SceneInfoType.Quit:
                EnvironmentManager.QuitStatic(); break;

        }
        isPress = true;
    }
    public override void InitFunc(SOSceneInfo tempData)
    {
        base.InitFunc(tempData);
        text.text = tempData.displayName;
    }
}
