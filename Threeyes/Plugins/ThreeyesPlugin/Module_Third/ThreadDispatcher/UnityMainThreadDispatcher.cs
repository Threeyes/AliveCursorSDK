/*
Copyright 2015 Pim de Witte All Rights Reserved.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Events;

public static class UnityMainThreadDispatcherExtension
{
	//兼容两种Action
	public static void ExecuteInMainThread(this UnityAction aciton)
	{
		UnityMainThreadDispatcher.Instance().Enqueue(aciton);
	}
	public static void ExecuteInMainThread(this Action aciton)
	{
		UnityMainThreadDispatcher.Instance().Enqueue(aciton);
	}
}

/// Author: Pim de Witte (pimdewitte.com) and contributors, https://github.com/PimDeWitte/UnityMainThreadDispatcher
/// <summary>
/// A thread-safe class which holds a queue with actions to execute on the next Update() method. It can be used to make calls to the main thread for
/// things such as UI Manipulation in Unity. It was developed for use in combination with the Firebase Unity plugin, which uses separate threads for event handling
/// </summary>
public class UnityMainThreadDispatcher : MonoBehaviour
{

	private static readonly Queue<Action> _executionQueue = new Queue<Action>();

	public void Update()
	{
		lock (_executionQueue)
		{
			while (_executionQueue.Count > 0)
			{
				_executionQueue.Dequeue().Invoke();
			}
		}
	}

	/// <summary>
	/// Locks the queue and adds the IEnumerator to the queue
	/// </summary>
	/// <param name="action">IEnumerator function that will be executed from the main thread.</param>
	public void Enqueue(IEnumerator action)
	{
		lock (_executionQueue)
		{
			_executionQueue.Enqueue(
			() =>
				{
					StartCoroutine(action);
				});
		}
	}

	/// <summary>
	/// Locks the queue and adds the Action to the queue
	/// </summary>
	/// <param name="action">function that will be executed from the main thread.</param>
	public void Enqueue(Action action)
	{
		Enqueue(ActionWrapper(action));
	}
	public void Enqueue(UnityAction action)
	{
		Enqueue(ActionWrapper(action));
	}
	IEnumerator ActionWrapper(UnityAction a)
	{
		a();
		yield return null;
	}
	IEnumerator ActionWrapper(Action a)
	{
		a();
		yield return null;
	}


	private static UnityMainThreadDispatcher _instance = null;

	public static bool Exists()
	{
		return _instance != null;
	}

	/// <summary>
	/// Warning:
	/// 1. 因为该方法是在多线程中被调用，因此无法实时创建实例且错误难以捕捉，只能是用户提前创建对应的物体
	/// </summary>
	/// <returns></returns>
	public static UnityMainThreadDispatcher Instance()
	{
		if (!Exists())
		{
			Debug.LogError("UnityMainThreadDispatcher could not find the UnityMainThreadDispatcher object. Please ensure you have added the MainThreadExecutor Prefab to your scene before game start.");
		}
		return _instance;
	}


	void Awake()
	{
		if (_instance == null)
		{
			_instance = this;
			DontDestroyOnLoad(this.gameObject);
		}
	}

	void OnDestroy()
	{
		_instance = null;
	}
}
