
using UnityEngine;
namespace Threeyes.Persistent
{
    public class PersistentController_PlayerPref_String : PersistentController_PlayerPref<string>
    {
        public PersistentController_PlayerPref_String(IPersistentData<string> persistentData, PersistentControllerOption option) : base(persistentData, option) { }

        protected override string GetValueFunc()
        {
            return PlayerPrefs.GetString(Key, DefaultValue);
        }
        protected override void SaveValueFunc(string value)
        {
            PlayerPrefs.SetString(Key, value);
        }
    }

}