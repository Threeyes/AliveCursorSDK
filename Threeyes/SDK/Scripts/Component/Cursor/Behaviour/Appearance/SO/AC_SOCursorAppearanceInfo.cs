using UnityEngine;
using Newtonsoft.Json;
/// <summary>
/// Custom Appearance relate to system cursor type
/// </summary>
[CreateAssetMenu(menuName = AC_EditorDefinition.AssetMenuPrefix_AC_Cursor_Behaviour_Appearance + "Info", fileName = "CursorAppearanceInfo", order = 1)]
public class AC_SOCursorAppearanceInfo : ScriptableObject
{
    public AC_SystemCursorAppearanceType cursorAppearanceType = AC_SystemCursorAppearanceType.None;//Target SystemCursorType
    [JsonIgnore] public Texture texture;//Image for relate SystemCursorType
    public Color color = Color.white;//Color that mix with texture

    //ToAdd: 增加RuntimeEdit加载外部贴图的功能
}
