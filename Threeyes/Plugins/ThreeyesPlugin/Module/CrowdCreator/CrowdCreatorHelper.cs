using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif
/// <summary>
/// 根据不同参数创建人群
/// </summary>
public class CrowdCreatorHelper : MonoBehaviour
{
    public CrowdCreator crowdCreator;
    public SimpleWaypoint wayPointStart;//起始路标点
    public Range_Vector3 spawnRange = new Range_Vector3(new Vector3(-0.5f, 0, -0.5f), new Vector3(0.5f, 0, 0.5f));//开始创建人群的范围
    public Range_Int randomPeopleNum = new Range_Int(1, 3);//单次生成数量
    //Todo：加一个创建的间隔
    public Range_Float randomSpeed = new Range_Float(0.8f, 1.5f);//奔跑速度(Todo：改名为rangeSpeed）

    public List<string> listDefaultAnimParamBool = new List<string>();//初始化时的Bool参数，空为不设置
    public List<string> listDefaultAnimParamTrigger = new List<string>();//初始化时的Trigger参数，参数空为不设置

    public bool isRandomRotation = false;//出生时朝向随机，适用于静止的人群
    public bool isShout = true;//尖叫
    public bool isAutoDespawn = true;//到达终点后自动销毁
    public bool isMoveAlong = true;//出生后沿着WayPoint行走

    [Header("整体控制")]
    public bool isDestroyAllOnStopCreate = true;//是否在调用CreateRandomPeopleGroup为false之后立即销毁所有创建的人物
    public int maxPeople = 10;//人数上限
    public List<People> listPeopleCreated = new List<People>();//已创建的人数
    private void Awake()
    {
        if (!crowdCreator)
            crowdCreator = GetComponentInParent<CrowdCreator>();
    }

    [ContextMenu("CreateRandomPeopleGroup")]
    void CreateRandomPeopleGroup()
    {
        CreateRandomPeopleGroup(true);
    }
    [ContextMenu("DespawnAllPeople")]
    void DespawnAllPeople()
    {
        //销毁所有人物
        if (isDestroyAllOnStopCreate)
            crowdCreator.DespawnAllPeople();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="isCreate">true创建，false销毁所有人物</param>
    public void CreateRandomPeopleGroup(bool isCreate)
    {
        if (!isCreate)
        {
            DespawnAllPeople();
        }
        else
        {
            int newRandomValue = randomPeopleNum.RandomValue;
            //限制人数上限
            if (maxPeople > 0)
            {
                if (listPeopleCreated.Count >= maxPeople)
                    return;
                else
                {
                    int leftValue = maxPeople - listPeopleCreated.Count;//剩余数量
                    newRandomValue = Mathf.Min(leftValue, newRandomValue);
                }
            }

            //记录已经生成的People数量
            List<People> listPeople = crowdCreator.CreateRunningPeopleGroup(newRandomValue, randomSpeed, wayPointStart, spawnRange, isShout, isAutoDespawn, isMoveAlong, isRandomRotation);
            listPeopleCreated.AddRange(listPeople);
            foreach (People p in listPeople)
            {
                SetDefaultAnim(p);

                p.onDespawn.AddListener(
                () =>
                {
                    listPeopleCreated.Remove(p);
                });

            }
        }
    }


    void SetDefaultAnim(People people)
    {
        Animator animator = people.m_Animator;

        if (listDefaultAnimParamBool.Count > 0)
        {
            string param = listDefaultAnimParamBool.GetRandom();
            if (!param.IsNullOrEmpty())
                people.m_Animator.SetBool(param, true);
        }

        if (listDefaultAnimParamTrigger.Count > 0)
        {
            string param = listDefaultAnimParamTrigger.GetRandom();
            if (!param.IsNullOrEmpty())
                people.m_Animator.SetTrigger(param);
        }

    }
#if UNITY_EDITOR

    //static Vector3 deltaGizmo = new Vector3(0, 0.1f, 0);
    [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.Selected)]
    static void MyGizmo(CrowdCreatorHelper crowdCreatorHelper, GizmoType gizmoType)  //参数1为“XX”组件，可以随意选，参数2 必须写，不用赋值  
    {
        Gizmos.color = Color.green;   //绘制时颜色  
        Range_Vector3 tmpSpawnRange = crowdCreatorHelper.spawnRange;

        if (crowdCreatorHelper.wayPointStart)
        {
            Vector3 center = crowdCreatorHelper.wayPointStart.transform.position + (tmpSpawnRange.MinValue + tmpSpawnRange.MaxValue) / 2;
            Gizmos.DrawCube(center, tmpSpawnRange.MaxValue - tmpSpawnRange.MinValue + new Vector3(0, 0.2f, 0));
        }
    }
#endif

}