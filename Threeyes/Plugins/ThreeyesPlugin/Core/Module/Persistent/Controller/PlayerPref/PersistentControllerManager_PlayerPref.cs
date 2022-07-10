using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Threeyes.Persistent
{
    [RequireComponent(typeof(InstanceManager))]
    [DefaultExecutionOrder(-20000)]
    public class PersistentControllerManager_PlayerPref: PersistentControllerManagerBase<PersistentControllerFactory_PlayerPref, PersistentControllerOption_PlayerPref>
    {

    }
}