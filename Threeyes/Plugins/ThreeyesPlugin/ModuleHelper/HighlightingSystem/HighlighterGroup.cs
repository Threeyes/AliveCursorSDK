#if USE_HighlightingSystem
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HighlightingSystem;
public class HighlighterGroup : ComponentGroupBase<Highlighter>
{
    public void Enable(bool isEnable)
    {
        ForEachChildComponent<Highlighter>(h => h.enabled = isEnable);
    }
}
#endif