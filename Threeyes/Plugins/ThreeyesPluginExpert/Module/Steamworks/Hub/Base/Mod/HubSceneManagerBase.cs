using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Threeyes.Steamworks
{
    public class HubSceneManagerBase<T> : HubManagerWithLifeCycleBase<T>, IHubSceneManager
        where T: HubSceneManagerBase<T>
    {
		#region Interface
		public Scene HubScene { get { return hubScene; } }
		public Scene CurModScene { get { return curModScene; } }

		protected Scene hubScene;
		protected Scene curModScene;

		protected override void SetInstanceFunc()
		{
			base.SetInstanceFunc();
			hubScene = gameObject.scene;//Use self scene
		}

		#endregion
	}
}