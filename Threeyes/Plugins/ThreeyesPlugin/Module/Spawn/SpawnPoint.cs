using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
public class SpawnPoint : MonoBehaviour
{
    public float spawnTime = 0;//剩余的生成时间
    public Vector2 spawnTimeRange = new Vector2(2, 5);
    public List<GameObject> listPre = new List<GameObject>();
    public Transform tfRoot;
    public BoolEvent onStartSpawn;
    bool isStartSpawn = false;

    public int maxSpawnCount = -1;//最大生成数(数值为1适用于驻留类的生成）
    public float minTimeBetweenSpawn = -1;//生成时间间隔判定（适用于行走类的生成）

    Transform TfRoot
    {
        get
        {
            if (!tfRoot)
                tfRoot = transform;
            return tfRoot;
        }
    }
    /// <summary>
    /// 现存的所有SpawnTarget
    /// </summary>
    public List<SpawnTarget> listSpawnTarget { get { return TfRoot.GetComponentsInChildren<SpawnTarget>().ToList(); } }

    private void Awake()
    {
        spawnTime = Random.Range(spawnTimeRange.x, spawnTimeRange.y);

        TfRoot.DestroyAllChild();//清空占位物品
    }

    public void StartSpawn(bool isOn)
    {
        onStartSpawn.Invoke(isOn);
        isStartSpawn = isOn;
    }

    [ContextMenu("StartSpawn")]
    public void StartSpawn()
    {
        StartSpawn(true);
    }

    /// <summary>
    /// 该生成点是否有效
    /// </summary>
    public bool IsAvaliable()
    {
        if (maxSpawnCount > 0)
        {
            if (listSpawnTarget.Count >= maxSpawnCount)
                return false;
        }
        if (minTimeBetweenSpawn > 0)
        {
            if (Time.time - lastSpawnTime < minTimeBetweenSpawn)
            {
                return false;
            }
        }
        return true;
    }

    float lastSpawnTime = 0;//上次生成的时间
    private void Update()
    {
        if (!isStartSpawn)
            return;

        spawnTime -= Time.deltaTime;

        if (spawnTime <= 0)
        {
            SpawnAtOnce();
            spawnTime = Random.Range(spawnTimeRange.x, spawnTimeRange.y);
        }
    }

    public bool SpawnAtOnce()
    {
        //Init Prefab
        GameObject goPre = listPre.GetRandom();
        if (!goPre)
        {
            Debug.LogError("没有预制物！");
            return false;
        }

        GameObject inst = Instantiate(goPre, tfRoot.position, tfRoot.rotation, tfRoot);
        SpawnTarget spawnTarget = inst.GetComponent<SpawnTarget>();
        if (spawnTarget)
            spawnTarget.Init(this);

        lastSpawnTime = Time.time;
        return true;
    }


}
