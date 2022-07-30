using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AC_GameObjectHelper : MonoBehaviour
{
	/// <summary>
	/// Set the desire child active, whild the other childs will remain deactive
	/// </summary>
	/// <param name="index"></param>
	public void SetChildActiveSolo(int index)
	{
		for (int i = 0; i != transform.childCount; i++)
		{
			Transform tfChild = transform.GetChild(i);
			tfChild.gameObject.SetActive(i == index);
		}
	}

	public void Destroy()
	{
		Destroy(gameObject);
	}
}
