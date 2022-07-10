using UnityEngine;
namespace Threeyes.Persistent
{
    public class PersistentController_PlayerPref_Bool : PersistentController_PlayerPref<bool>
    {
        public PersistentController_PlayerPref_Bool(IPersistentData<bool> persistentData, PersistentControllerOption option) : base(persistentData, option) { }

        protected override bool GetValueFunc()
        {
            var result = PlayerPrefs.GetInt(Key, BoolToInt(DefaultValue));

            return IntToBool(result);
        }


        protected override void SaveValueFunc(bool value)
        {
            PlayerPrefs.SetInt(Key, value == false ? 0 : 1);
        }

        #region Utility
        static bool IntToBool(int input)
        {
            return input == 0 ? false : true;
        }
        static int BoolToInt(bool value)
        {
            return value ? 1 : 0;
        }
        #endregion
    }
}
