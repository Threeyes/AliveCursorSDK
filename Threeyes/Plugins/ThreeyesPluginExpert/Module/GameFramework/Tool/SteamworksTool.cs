using Threeyes.Core;
using Threeyes.Localization;
using UnityEngine;

namespace Threeyes.GameFramework
{
    public static class SteamworksTool
    {
        //——Facepunch.Steamworks——
        /// <summary>
        /// 尝试将Localization的多语言转为Steamworks需要的APILanguageCode，用于Query.InLanguage的参数
        /// 
        /// Warning:
        /// -Lean Localization的TranslationCode仅用于自动转换，不应用作该方法的参数。这里改为使用LeanLanguage实例的名称
        /// Ref:
        /// -多语言相关表:https://partner.steamgames.com/doc/store/localization/languages
        /// </summary>
        /// <param name="strLocalizationLanguageName">多语言的名称，如English、Chinese</param>
        /// <returns>Steamworks要求的code</returns>
        public static string GetAPILanguageCode(string strLocalizationLanguageName = null)
        {
            string strAPILanguageCode = "english";

            if (strLocalizationLanguageName.IsNullOrEmpty())
                strLocalizationLanguageName = LocalizationManagerHolder.LocalizationManager.CurrentLanguage;

            if (strLocalizationLanguageName.IsNullOrEmpty())
                return strAPILanguageCode;

            switch (strLocalizationLanguageName)
            {
                case "Alienese": strAPILanguageCode = "english"; break;
                case "Arabic": strAPILanguageCode = "arabic"; break;
                case "Chinese": strAPILanguageCode = "schinese"; break;//PS：该语言包含了所有中文语系（大陆、台湾、香港等）
                case "English": strAPILanguageCode = "english"; break;
                case "French": strAPILanguageCode = "french"; break;
                case "German": strAPILanguageCode = "german"; break;
                case "Italian": strAPILanguageCode = "italian"; break;
                case "Japanese": strAPILanguageCode = "japanese"; break;
                case "Korean": strAPILanguageCode = "koreana"; break;
                case "Portuguese": strAPILanguageCode = "portuguese"; break;
                case "Russian": strAPILanguageCode = "russian"; break;
                case "Spanish": strAPILanguageCode = "spanish"; break;
                default: Debug.LogError(strLocalizationLanguageName + " not define!"); break;
            }

            return strAPILanguageCode;
        }
    }
}