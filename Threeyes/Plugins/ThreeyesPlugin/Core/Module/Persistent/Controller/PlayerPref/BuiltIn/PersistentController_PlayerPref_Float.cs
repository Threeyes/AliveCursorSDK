using UnityEngine;
namespace Threeyes.Persistent
{
    public class PersistentController_PlayerPref_Float : PersistentController_PlayerPref<float>
    {
        public PersistentController_PlayerPref_Float(IPersistentData<float> persistentData, PersistentControllerOption option) : base(persistentData, option) { }

        protected override float GetValueFunc()
        {
            return PlayerPrefs.GetFloat(Key, DefaultValue);
        }
        protected override void SaveValueFunc(float value)
        {
            PlayerPrefs.SetFloat(Key, value);
        }
    }
}