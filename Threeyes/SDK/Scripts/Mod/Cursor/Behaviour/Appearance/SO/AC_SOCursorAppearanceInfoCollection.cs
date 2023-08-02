using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;
/// <summary>
/// Custom appearance for all system built in cursor type
/// </summary>
[CreateAssetMenu(menuName = AC_EditorDefinition.AssetMenuPrefix_Root_Cursor_Behaviour_Appearance + "InfoCollection", fileName = "CursorAppearanceInfoCollection", order = 2)]
public class AC_SOCursorAppearanceInfoCollection : SOCollectionBase<AC_SystemCursorAppearanceType, AC_SOCursorAppearanceInfo>
{
    [JsonIgnore] public virtual List<AC_SOCursorAppearanceInfo> ListData { get { return listData; } set { listData = value; } }

    [Header("PS：Arrow is the default state, which normally should be excluded from ListData")]
    [SerializeField] protected List<AC_SOCursorAppearanceInfo> listData = new List<AC_SOCursorAppearanceInfo>();

    public override AC_SOCursorAppearanceInfo this[AC_SystemCursorAppearanceType en] => ListData.FirstOrDefault(
        (s) =>
    {
        if (!s)//PS: In case error was thrown if null
            return false;
        return s.cursorAppearanceType == en;
    });
}
