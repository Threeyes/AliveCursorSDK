using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Threeyes.Localization
{
    public class LocalizationManagerHolder : MonoBehaviour
    {
        public static ILocalizationManager LocalizationManager { get; internal set; }
    }
}