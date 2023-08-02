using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Obsolete("Use the class without AC_ prefix instead!", true)]
public class AC_GameObjectHelper : GameObjectHelper
{
	public void Destroy()
	{
		Destroy(gameObject);
	}
}
