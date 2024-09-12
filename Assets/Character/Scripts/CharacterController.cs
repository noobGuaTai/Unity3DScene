using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System;

/// <summary>
/// 记录角色可接受的玩家输入
/// </summary>
public struct PlayerCharacterInputs
{
    public float moveAxisForward;// 前后移动
    public float moveAxisRight;// 左右移动
    public Quaternion cameraRotation;// 镜头移动
}
public class CharacterController : MonoBehaviour, ICharacterController
{
    public KinematicCharacterMotor motor;
    public Vector3 relativeMoveInputVector;// 相对于相机的移动向量
    public Vector3 lookInputVector;// 相机方向输入变量

    private Vector3 gravity = new Vector3(0, -10f, 0);
    public Vector3 currentVelocity = Vector3.zero;
    public Vector3 targetMoveVelocity = Vector3.zero;
    private PlayerInputController playerInputController;
    public Vector3 rootMotionSpeed;


    void Start()
    {
        motor = GetComponent<KinematicCharacterMotor>();
        motor.CharacterController = this;
        playerInputController = GetComponent<PlayerInputController>();
    }

    public void SetInputs(ref PlayerCharacterInputs inputs)
    {
        // 限制输入向量的大小到1
        Vector3 moveInputVector = Vector3.ClampMagnitude(new Vector3(inputs.moveAxisForward, 0f, inputs.moveAxisRight), 1f);
        // 相机在玩家所处平面上的投影方向
        Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.cameraRotation * Vector3.forward, motor.CharacterUp).normalized;
        if (cameraPlanarDirection.sqrMagnitude == 0f)// 相机与玩家在同一y轴时
        {
            cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.cameraRotation * Vector3.up, motor.CharacterUp).normalized;
        }
        // 将投影向量转为旋转量
        Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, motor.CharacterUp);

        relativeMoveInputVector = cameraPlanarRotation * moveInputVector;
        lookInputVector = cameraPlanarDirection;
        // print(cameraPlanarRotation);
    }
    #region ICharacterController
    public void AfterCharacterUpdate(float deltaTime)
    {
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        return true;
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void PostGroundingUpdate(float deltaTime)
    {
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
    }
    #endregion

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        if (playerInputController.cameraMode == CameraMode.Lock)// 锁定状态下的旋转自身
        {
            if (lookInputVector != Vector3.zero && playerInputController.playerState != PlayerState.Dash && playerInputController.cameraTargetGroup.m_Targets[1].target != null)
            {
                // 朝镜头前方旋转
                // Vector3 smoothLookInputDirection = Vector3.Lerp(motor.CharacterForward, lookInputVector, 0.5f).normalized;
                // currentRotation = Quaternion.LookRotation(smoothLookInputDirection, motor.CharacterUp);

                // 朝目标旋转
                Vector3 directionToTarget = (playerInputController.cameraTargetGroup.m_Targets[1].target.position - motor.Transform.position).normalized;
                // 如果目标在不同的高度，保持角色水平旋转
                directionToTarget = Vector3.ProjectOnPlane(directionToTarget, motor.CharacterUp).normalized;
                if (directionToTarget != Vector3.zero)
                {
                    Vector3 smoothLookInputDirection = Vector3.Slerp(motor.CharacterForward, directionToTarget, 1 - Mathf.Exp(-10 * deltaTime)).normalized;
                    currentRotation = Quaternion.LookRotation(smoothLookInputDirection, motor.CharacterUp);
                }

            }
            else// 锁定状态下，冲刺
            {
                if (relativeMoveInputVector != Vector3.zero && playerInputController.playerState != PlayerState.UnderAttack)
                {
                    Vector3 targetDirection = relativeMoveInputVector.normalized;
                    Vector3 smoothLookInputDirection = Vector3.Slerp(motor.CharacterForward, targetDirection, 1 - Mathf.Exp(-10 * deltaTime)).normalized;
                    currentRotation = Quaternion.LookRotation(smoothLookInputDirection, motor.CharacterUp);
                }
            }
        }
        else
        {
            // 未锁定状态下的旋转自身
            if (playerInputController.playerState != PlayerState.Attack && playerInputController.playerState != PlayerState.Defense)
            {
                if (relativeMoveInputVector != Vector3.zero)
                {
                    Vector3 targetDirection = relativeMoveInputVector.normalized;
                    Vector3 smoothLookInputDirection = Vector3.Slerp(motor.CharacterForward, targetDirection, 1 - Mathf.Exp(-10 * deltaTime)).normalized;
                    currentRotation = Quaternion.LookRotation(smoothLookInputDirection, motor.CharacterUp);
                }
            }

            if (playerInputController.playerState == PlayerState.Attack && playerInputController.nearestSurroundEnemy != null)// 近战索敌
            {
                // 朝目标旋转
                Vector3 directionToTarget = (playerInputController.nearestSurroundEnemy.transform.position - motor.Transform.position).normalized;
                // 如果目标在不同的高度，保持角色水平旋转
                directionToTarget = Vector3.ProjectOnPlane(directionToTarget, motor.CharacterUp).normalized;
                if (directionToTarget != Vector3.zero)
                {
                    Vector3 smoothLookInputDirection = Vector3.Slerp(motor.CharacterForward, directionToTarget, 1 - Mathf.Exp(-10 * deltaTime)).normalized;
                    currentRotation = Quaternion.LookRotation(smoothLookInputDirection, motor.CharacterUp);
                }
            }

        }
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)// 在这个函数里边Time.deltaTime等于deltaTime
    {
        this.currentVelocity = currentVelocity;
        if (motor.GroundingStatus.IsStableOnGround)// 如果玩家站在地面上
        {
            // if (playerInputController.playerState != PlayerState.Attack)
            // {
            //     _ = motor.GetDirectionTangentToSurface(currentVelocity, motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;
            //     Vector3 inputRight = Vector3.Cross(relativeMoveInputVector, motor.CharacterUp);
            //     Vector3 reorientedInput = Vector3.Cross(motor.GroundingStatus.GroundNormal, inputRight).normalized * relativeMoveInputVector.magnitude;
            //     targetMoveVelocity = reorientedInput * maxMoveSpeed;
            //     if (relativeMoveInputVector != Vector3.zero)
            //         currentVelocity = Vector3.Lerp(currentVelocity, targetMoveVelocity, playerInputController.allInputTime);
            //     else
            //         currentVelocity = Vector3.Lerp(currentVelocity, targetMoveVelocity, 1 - playerInputController.allInputTime);
            // }
            // else
            // {
            //     currentVelocity = Vector3.zero;
            // }
            currentVelocity = rootMotionSpeed;

        }
        else
        {
            currentVelocity += gravity * deltaTime;
        }
    }

    void OnAnimatorMove()
    {
        rootMotionSpeed = playerInputController.animator.deltaPosition / Time.deltaTime;
    }

}
