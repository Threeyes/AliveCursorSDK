using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ExplosionForce : MonoBehaviour
{
    public float radius=10;
    public float Force = 5;

    public Color gizmoscolor = Color.yellow;

    public string layerName = "Default";

    public void AddForce()
    {
        try
        {
            List<Collider> listHitCollider = new List<Collider>();

            if (layerName.NotNullOrEmpty())//通过layerName获取
            {
                listHitCollider = Physics.OverlapSphere(this.transform.position, radius, 1 << LayerMask.NameToLayer(layerName)).ToList();
            }
            else//通过特定脚本ExplosionForceTarget获取
            {
                var tempColliders = Physics.OverlapSphere(this.transform.position, radius);
                foreach(var collider in tempColliders)
                {
                    if(collider.GetComponentInParent<ExplosionForceTarget>())
                    {
                        listHitCollider.AddOnce(collider);
                    }
                }
            }

            //Get All Rigidbody
            List<Rigidbody> listRig = new List<Rigidbody>();
            foreach (var collider in listHitCollider)
            {
                Rigidbody rig = collider.attachedRigidbody;
                if(rig)
                {
                    listRig.AddOnce(rig);
                }
            }

            //Add Force
            foreach(var rig in listRig)
            {
                rig.AddForce(-(transform.localPosition - rig.transform.localPosition) * Force, ForceMode.Impulse);
            }
            
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
    }

    void OnDrawGizmos()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = gizmoscolor;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
