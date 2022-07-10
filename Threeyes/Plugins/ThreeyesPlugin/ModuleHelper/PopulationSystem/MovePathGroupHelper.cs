#if USE_PopulationSystem
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePathGroupHelper : ComponentGroupBase<MovePath>
{

    public void SetMoveSpeed(float speed)
    {
        ForEachChildComponent((w) => w.moveSpeed = speed);
    }

}

#endif