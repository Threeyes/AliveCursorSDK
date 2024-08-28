using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threeyes.GameFramework
{
    /// <summary>
    /// Contain object movement
    /// </summary>
    public interface IObjectMovement
    {
        /// <summary>
        /// Check if the target moving 
        /// </summary>
        bool IsMoving { get; }
        float CurMoveSpeedPercent { get; }
        float CurMoveSpeed { get; }
        float MaxMoveSpeed { get; }
        float LastMoveTime { get; }
    }
}