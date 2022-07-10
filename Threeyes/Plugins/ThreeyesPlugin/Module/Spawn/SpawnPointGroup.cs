using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
public class SpawnPointGroup : ComponentGroupBase<SpawnPoint>
{
    public Vector2 vt2SpawnTimeRange = new Vector2(3, 5);

    public int SpawnCount { get { return spawnCount; } set { spawnCount = value; } }

    public int totalSpawnCount = -1;//最多总生成数量，-1代表无限
    public int spawnCount = 1;//每次生成数量

    public UnityEvent onAllTargetSpawned;//所有生成完毕
    public UnityEvent onAllTargetDestroyed;//所有预制物都被销毁
    public bool isStartSpawn = false;
    public BoolEvent onStartSpawn;

    void ResetData()
    {
        isDetectSpawnTargetCount = false;
        tempSpawnCount = 0;
    }

    /// <summary>
    /// 该物体自动生成
    /// </summary>
    /// <param name="isOn"></param>
    public void StartSpawn(bool isOn)
    {
        isStartSpawn = isOn;
        if (isOn)
        {
            ResetData();
            Spawn(SpawnCount);
        }
        onStartSpawn.Invoke(isOn);
    }

    float spawnTime = 0;
    float lastSpawnTime = 0;
    private void Update()
    {
        //检测预制物的状态
        if (isDetectSpawnTargetCount)
        {
            int spawnTargetCount = 0;
            ListComp.ForEach((sp) => spawnTargetCount += sp.listSpawnTarget.Count);//剩余的SpawnTarget
            if (spawnTargetCount == 0)
            {
                onAllTargetDestroyed.Invoke();
                isDetectSpawnTargetCount = false;
            }
        }

        //生成预制物
        if (isStartSpawn)
        {
            spawnTime -= Time.deltaTime;
            if (spawnTime <= 0)
            {
                spawnTime = Random.Range(vt2SpawnTimeRange.x, vt2SpawnTimeRange.y);

                Spawn(SpawnCount);
            }
        }
    }

    /// <summary>
    /// 子物体自动生成
    /// </summary>
    /// <param name="isOn"></param>
    public void StartSpawnChild(bool isOn)
    {
        ForEachChildComponent((sp) => sp.StartSpawn(isOn));
    }

    bool isDetectSpawnTargetCount = false;//检测剩余生成物的数量
    int tempSpawnCount = 0;
    /// <summary>
    /// 随机生成
    /// </summary>
    /// <param name="spawnCount"></param>
    public void Spawn(int spawnCount)
    {
        //Todo:统计占用
        List<SpawnPoint> listSpawnPointUsed = new List<SpawnPoint>();
        List<SpawnPoint> listAllSpawnPoint = ListComp;
        if (listAllSpawnPoint.Count <= 0)
            return;

        for (int i = 0; i != spawnCount; i++)
        {
            //找到可用的点
            var arrAvaliableSP = from sp in listAllSpawnPoint
                                 where sp.IsAvaliable()
                                 select sp;

            var listAvaliableSP = arrAvaliableSP.ToList();
            if (listAvaliableSP.Count <= 0)
                return;

            SpawnPoint spawnPoint = listAvaliableSP.ToList().GetRandom();
            if (spawnPoint)
            {
                spawnPoint.SpawnAtOnce();
                tempSpawnCount++;
                listAllSpawnPoint.Remove(spawnPoint);

                //判断
                if (totalSpawnCount > 0 && tempSpawnCount >= totalSpawnCount)
                {
                    onAllTargetSpawned.Invoke();
                    isDetectSpawnTargetCount = true;
                    StartSpawn(false);
                    break;
                }
            }
        }
    }

}
