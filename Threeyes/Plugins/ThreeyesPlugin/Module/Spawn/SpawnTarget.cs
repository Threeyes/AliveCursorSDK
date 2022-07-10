using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 被生成物所挂载的脚本
/// </summary>
public class SpawnTarget : ComponentHelperBase<Transform>
{
    public bool isRandomSize = false;
    public Vector2 vt2RangeUniformSize = new Vector2(0.5f, 1f);
    public virtual void Init(SpawnPoint spawnPoint)
    {
        if (isRandomSize)
            Comp.localScale = Vector3.one * Random.Range(vt2RangeUniformSize.x, vt2RangeUniformSize.y);
    }

    
}
