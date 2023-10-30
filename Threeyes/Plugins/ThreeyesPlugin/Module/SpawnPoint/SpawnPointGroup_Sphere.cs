using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threeyes.SpawnPoint
{
    public class SpawnPointGroup_Sphere : SpawnPointGroupBase
    {
        public bool surface = false;//True: point inside sphere; False: point on sphere's surface
        public float sphereRadius = 0.5f;

        public override Pose spawnPose
        {
            get
            {
                Vector3 newPointPos = transform.position + (surface ? Random.onUnitSphere : Random.insideUnitSphere) * sphereRadius;
                Quaternion newRotation = Quaternion.LookRotation(transform.position - newPointPos, Vector3.up);//ToUpdate:LookRotation的upward参数需要更新
                return new Pose(newPointPos, newRotation);
            }
        }
    }
}