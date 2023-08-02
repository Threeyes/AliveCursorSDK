using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threeyes.Steamworks
{
    public interface IEnvironmentManager :
        IHubManagerWithController<IEnvironmentController>
        , IHubEnvironmentManager
        , IHubManagerModInitHandler
    {
    }
}