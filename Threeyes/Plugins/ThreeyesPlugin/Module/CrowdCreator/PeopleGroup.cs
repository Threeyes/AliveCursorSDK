using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 控制所有子对象People
/// </summary>
public class PeopleGroup : ComponentGroupBase<People>
{
    public SimpleWaypoint wayPointStart;
    public PeopleState peopleState;
    public void SitDown(bool isSet)
    {
        ForEachChildComponent((p) => p.SitDown(isSet));
    }

    public void Hesitate(bool isSet)
    {
        ForEachChildComponent((p) => p.Hesitate(isSet));
    }
    public void DodgeOnce()
    {
        ForEachChildComponent((p) => p.DodgeOnce());
    }
    public void Dodge(bool isSet)
    {
        ForEachChildComponent((p) => p.Dodge(isSet));
    }

    public void Die()
    {
        ForEachChildComponent((p) => p.Die());
    }

    public void SetState()
    {
        ForEachChildComponent((p) => p.SetState(peopleState));
    }
    public void MoveAlong()
    {
        if (wayPointStart)
            ForEachChildComponent((p) => p.MoveAlong(wayPointStart));
        else
        {
            Debug.LogError("wayPointStart is Null!");
        }
    }
}