using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Threeyes.Persistent
{
    public abstract class PersistentController_PlayerPref<TValue> : PersistentControllerBase<TValue, PersistentControllerOption_PlayerPref>
    {
        public PersistentController_PlayerPref(IPersistentData<TValue> persistentData, PersistentControllerOption option) : base(persistentData, option) { }

        ////ToDelete:改为在ControllerManager的Reset中进行初始化
        ///// <summary>
        ///// Default Constructor:
        ///// 1. SaveOnSet: true
        ///// 2. SaveOnDispose: true
        ///// 3.onlySaveOnChanged: false (Because it will get save on dispose)
        ///// </summary>
        ///// <param name="persistentData"></param>
        //public PersistentController_PlayerPref(IPersistentData<TValue> persistentData) : base(persistentData, option: new PersistentControllerOption(isSaveOnSet: true, isSaveOnDispose: true, onlySaveHasChanged: false)) { }

        protected override void SaveValueFunc(TValue value)
        {
            PlayerPrefs.Save();
        }
        protected override void DeleteKeyFunc()
        {
            PlayerPrefs.DeleteKey(Key);
        }
    }


    [Serializable]
    public class PersistentControllerOption_PlayerPref : PersistentControllerOption
    {
        public PersistentControllerOption_PlayerPref()
        {
            isSaveOnSet = true;
            isSaveOnDispose = true;
            onlySaveHasChanged = false;
        }
    }
}