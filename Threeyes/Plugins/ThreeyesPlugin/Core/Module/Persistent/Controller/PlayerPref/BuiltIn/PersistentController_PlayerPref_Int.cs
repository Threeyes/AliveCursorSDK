using UnityEngine;
namespace Threeyes.Persistent
{
    public class PersistentController_PlayerPref_Int : PersistentController_PlayerPref<int>
    {
        public PersistentController_PlayerPref_Int(IPersistentData<int> persistentData, PersistentControllerOption option) : base(persistentData, option) { }

        protected override int GetValueFunc()
        {
            return PlayerPrefs.GetInt(Key, DefaultValue);
        }
        protected override void SaveValueFunc(int value)
        {
            PlayerPrefs.SetInt(Key, value);
        }
    }
}
