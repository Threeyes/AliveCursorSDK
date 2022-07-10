using UnityEngine;
using System;
using System.Collections;
/// <summary>
/// 参考：https://docs.unity3d.com/Manual/InverseKinematics.html
/// 人体IK动画
/// 要求：
/// 1.Animator Window——Layer中勾选IK Pass
/// 2.Animation Type为 Humanoid
/// </summary>
[RequireComponent(typeof(Animator))]

public class AnimatorIKHelper : ComponentHelperBase<Animator>
{
    /// <summary>
    /// 全局IK
    /// </summary>
    public bool IKActive { get { return ikActive; } set { ikActive = value; } }

    public bool IsLeftHandActive
    {
        get
        {
            return isLeftHandActive;
        }

        set
        {
            isLeftHandActive = value;
        }
    }

    public bool IsRightHandActive
    {
        get
        {
            return isRightHandActive;
        }

        set
        {
            isRightHandActive = value;
        }
    }

    public bool ikActive = false;
    public bool isLeftHandActive = true;
    public bool isRightHandActive = true;

    public Transform lookObj = null;
    public Transform leftHandTarget = null;
    public Transform rightHandTarget = null;
    public float lookWeight = 1;
    public float leftHandWeight = 1;
    public float rightHandWeight = 1;
    public Vector3 leftHandOffset = new Vector3(0, 0, -0.1f);
    public Vector3 leftHandDeltaRotation = new Vector3(0, 0, 90f);

    public Vector3 rightHandOffset = new Vector3(0, 0, -0.1f);
    public Vector3 rightHandDeltaRotation = new Vector3(0, 0, -90f);


    //a callback for calculating IK
    void OnAnimatorIK()
    {
        if (Comp)
        {
            //if the IK is active, set the position and rotation directly to the goal. 
            if (ikActive)
            {

                // Set the look target position, if one has been assigned
                if (lookObj != null)
                {
                    Comp.SetLookAtWeight(lookWeight);
                    Comp.SetLookAtPosition(lookObj.position);
                }

                // Set the right hand target position and rotation, if one has been assigned
                SetIK(leftHandTarget, AvatarIKGoal.LeftHand, leftHandOffset, leftHandDeltaRotation, IsLeftHandActive, leftHandWeight);
                SetIK(rightHandTarget, AvatarIKGoal.RightHand, rightHandOffset, rightHandDeltaRotation, IsRightHandActive, leftHandWeight);
            }

            //if the IK is not active, set the position and rotation of the hand and head back to the original position
            else
            {
                ResetIK(AvatarIKGoal.LeftHand);
                ResetIK(AvatarIKGoal.RightHand);
                Comp.SetLookAtWeight(0);
            }
        }
    }

    private void SetIK(Transform target, AvatarIKGoal avatarIKGoal, Vector3 offset, Vector3 deltaRotation, bool isActive, float value)
    {
        if (target != null && isActive)
        {
            Comp.SetIKPositionWeight(avatarIKGoal, 1);
            Comp.SetIKRotationWeight(avatarIKGoal, 1);
            Comp.SetIKPosition(avatarIKGoal, target.position + target.forward * offset.z);
            //Comp.SetIKRotation(avatarIKGoal, target.rotation);
            Comp.SetIKRotation(avatarIKGoal, Quaternion.Euler(target.eulerAngles + deltaRotation));
        }
        else
        {
            Comp.SetIKPositionWeight(avatarIKGoal, 0);
            Comp.SetIKRotationWeight(avatarIKGoal, 0);
        }
    }


    private void ResetIK(AvatarIKGoal avatarIKGoal)
    {
        Comp.SetIKPositionWeight(avatarIKGoal, 0);
        Comp.SetIKRotationWeight(avatarIKGoal, 0);
    }

}