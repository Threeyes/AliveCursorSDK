using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// 创建人群
/// </summary>
public class CrowdCreator : MonoBehaviour
{
    public SOPrefabGroup prePeoples;
    public UnityAction onDespawnAllPeople;//Despawn所有生成的People

    public class CrowdCreateParam
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="randomPeopleNum"></param>
    /// <param name="rangeSpeed"></param>
    /// <param name="wayPoint">路标点</param>
    /// <param name="isShout">是否尖叫</param>
    /// <param name="isAutoDespawn">是否到达目的地后自动销毁</param>
    /// <param name="tfParent">父物体，注意不能有缩放！！！</param>
    public List<People> CreateRunningPeopleGroup(int peopleCount, Range_Float rangeSpeed, SimpleWaypoint wayPoint, Range_Vector3 spawnRange,
        bool isShout = true, bool isAutoDespawn = true, bool isMoveAlong = true, bool isRandomRotation = false, Transform tfParent = null)
    {
        List<People> listPeople = new List<People>();
        if (!prePeoples || prePeoples.ListData.Count == 0)
        {
            Debug.LogError("无预制物！");
            return listPeople;
        }

        for (int i = 0; i != peopleCount; i++)
        {
            People people = CreatePeople(wayPoint.transform, spawnRange, tfParent, isRandomRotation);
            //主动释放所有
            onDespawnAllPeople +=
                        () =>
                        {
                            if (people != null)
                                people.Despawn();
                        };//要防止调用已经自己释放的People

            //初始化设置
            PeopleState peopleState = people.state;
            peopleState.MoveSpeed = rangeSpeed.RandomValue;
            peopleState.isAutoDespawn = isAutoDespawn;

            if (isMoveAlong)
                people.MoveAlong(wayPoint);

            if (i == 0 && isShout)
                people.TryShout();//只让第一个人喊叫

            listPeople.Add(people);
        }
        return listPeople;
    }

    /// <summary>
    /// 创建单个人
    /// </summary>
    /// <param name="tfStart">出生点</param>
    /// <param name="spawnRange">出生点的位移</param>
    /// <param name="tfParent">父物体</param>
    /// <returns></returns>
    People CreatePeople(Transform tfStart, Range_Vector3 spawnRange, Transform tfParent = null, bool isRandomRotation = false)
    {
        GameObject pre = prePeoples.listPrefab.GetRandom();
        Vector3 pos = tfStart.position + spawnRange.RandomValue;
        Quaternion rot = isRandomRotation ? Quaternion.Euler((new Vector3(0, UnityEngine.Random.Range(0, 359), 0))) : tfStart.rotation;

        return People.Spawn(pre, pos, rot, tfParent);
    }

    /// <summary>
    /// 销毁所有人类
    /// </summary>
    public void DespawnAllPeople()
    {
        onDespawnAllPeople.Execute();
    }

}