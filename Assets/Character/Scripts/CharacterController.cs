using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

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
    public float maxMoveSpeed = 5f;// 最大移动速度
    public float moveSharpness = 15f;// 加减速敏锐度
    public float rotationSharpness = 10f;// 旋转敏锐度

    void Start()
    {
        motor = GetComponent<KinematicCharacterMotor>();
        motor.CharacterController = this;
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
        print(cameraPlanarRotation);
    }

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

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        if (lookInputVector != Vector3.zero && rotationSharpness > 0f)
        {
            // Vector3 smoothLookInputDirection = Vector3.Slerp(motor.CharacterForward, lookInputVector, 1 - Mathf.Exp(rotationSharpness * deltaTime)).normalized;
            // currentRotation = Quaternion.LookRotation(smoothLookInputDirection, motor.CharacterUp); 
            // print(smoothLookInputDirection);
            currentRotation = Quaternion.LookRotation(lookInputVector, motor.CharacterUp);
        }
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        Vector3 targetMoveVelocity = Vector3.zero;
        if (motor.GroundingStatus.IsStableOnGround)// 如果玩家站在地面上
        {
            currentVelocity = motor.GetDirectionTangentToSurface(currentVelocity, motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;
            Vector3 inputRight = Vector3.Cross(relativeMoveInputVector, motor.CharacterUp);
            Vector3 reorientedInput = Vector3.Cross(motor.GroundingStatus.GroundNormal, inputRight).normalized * relativeMoveInputVector.magnitude;
            targetMoveVelocity = reorientedInput * maxMoveSpeed;
            // currentVelocity = Vector3.Lerp(currentVelocity, targetMoveVelocity, 1 - Mathf.Exp(moveSharpness * deltaTime));
            currentVelocity = targetMoveVelocity;
        }
    }

}
