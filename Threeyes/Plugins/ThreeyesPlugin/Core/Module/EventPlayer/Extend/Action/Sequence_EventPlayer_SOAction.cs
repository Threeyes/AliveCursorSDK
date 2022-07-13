using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Threeyes.Action;
using UnityEngine;

namespace Threeyes.EventPlayer
{
	public class Sequence_EventPlayer_SOAction : Sequence_EventPlayer
	{
		//ToAdd:Pause 
	
	    /// <summary>
	    /// Set all one by one
	    /// </summary>
	    /// <param name="isEnter"></param>
	    public async void SetAllByOrderAsync(bool isEnter)
	    {
	        //Debug.Log("SetAllByOrderAsync begin");
	        foreach (EventPlayer_SOAction ep in ListData)
	        {
	            SOActionBase sOActionBase = ep.SOAction;
	            if (sOActionBase)
	            {
	                //ToAdd:��ActionEventPlayer���ж�Ӧ��Task��װ
	                await sOActionBase.EnterAsync(isEnter, ep.GOTarget);
	            }
	        }
	        //Debug.Log("SetAllByOrderAsync complete");
	    }
	

	    /// <summary>
	    /// Set all at the same time
	    /// </summary>
	    /// <param name="isEnter"></param>
	    public async void SetAllSynchronouslyAsync(bool isEnter)
	    {
	        //Debug.Log("SetAllSynchronouslyAsync begin");
	        List<Task> listTask = new List<Task>();
	        foreach (EventPlayer_SOAction ep in ListData)
	        {
	            SOActionBase sOActionBase = ep.SOAction;
	            if (sOActionBase)
	            {
	                //ToAdd:��ActionEventPlayer���ж�Ӧ��Task��װ
	                listTask.Add(sOActionBase.EnterAsync(isEnter, ep.GOTarget));
	            }
	        }
	        await Task.WhenAll(listTask.ToArray());
	        //Debug.Log("SetAllSynchronouslyAsync complete");
	    }
	
	}
}
