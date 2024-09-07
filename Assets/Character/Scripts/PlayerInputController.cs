using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using KinematicCharacterController.Examples;
using UnityEngine;
using UnityEngine.InputSystem;

public enum CameraMode
{
    UnLock,
    Lock
}

public enum PlayerMoveState
{
    Idle,
    Run,
    Dash
}

public class PlayerInputController : MonoBehaviour
{
    public Camera followedCamera;
    // public Transform cameraFollowPoint;
    public Animator animator;
    public CameraMode cameraMode = CameraMode.UnLock;
    public PlayerMoveState playerMoveState = PlayerMoveState.Idle;
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float allInputTime = 0f;// 玩家输入的时长(x和y)
    public float xInputTime = 0f;// 玩家输入的时长(x)
    public float zInputTime = 0f;// 玩家输入的时长(z)
    public CinemachineTargetGroup cameraTargetGroup;
    public List<GameObject> visibleEnemy;
    public GameObject nearestEnemy;

    private CharacterController characterController;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private PlayerCharacterInputs playerCharacterInputs;
    private Transform selfTransform;
    public float forwardSpeed = 0f;  // 未锁定 当前速度(前后)
    public float forwardTargetSpeed = 0f; // 未锁定 目标速度(前后)

    private float verticalSpeed = 0f;  // 锁定目标 当前速度(前后)
    private float verticalTargetSpeed = 0f;  // 锁定目标 当前速度(前后)
    private float horizontalSpeed = 0f;  // 锁定目标 当前速度(左右)
    private float HorizontalTargetSpeed = 0f; // 锁定目标 目标速度(前后)

    private CinemachineVirtualCamera lockCamera;
    private CinemachineFreeLook unLockCamera;

    void Start()
    {
        selfTransform = transform;
        // animator = GetComponentInChildren<Animator>();
        characterController = GetComponent<CharacterController>();
        playerCharacterInputs = new PlayerCharacterInputs();
        lockCamera = GameObject.Find("LockCamera").GetComponent<CinemachineVirtualCamera>();
        unLockCamera = GameObject.Find("UnLockCamera").GetComponent<CinemachineFreeLook>();
    }

    void Update()
    {
        HandlePlayerInput();
        UpdateInputTime();
        UpdatePlayerMoveState();
        UpdateAnimationRefreshMode();
    }

    /// <summary>
    /// 新输入系统的触发移动
    /// </summary>
    /// <param name="context"></param>
    public void PlayerMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        forwardTargetSpeed = (moveInput.x != 0 || moveInput.y != 0) ? 1f : 0f;// 很奇怪，moveInput.y是前后移动的输入
        verticalTargetSpeed = moveInput.y;
        HorizontalTargetSpeed = moveInput.x;
    }

    /// <summary>
    /// 新输入系统触发视角转换
    /// </summary>
    /// <param name="context"></param>
    public void PlayerLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void PlayerDash(InputAction.CallbackContext context)
    {
        var isPressed = context.ReadValueAsButton();
        if (playerMoveState == PlayerMoveState.Run)
        {
            playerMoveState = PlayerMoveState.Dash;
        }
    }

    /// <summary>
    /// 把玩家的输入保存到结构体里
    /// </summary>
    public void HandlePlayerInput()
    {
        playerCharacterInputs.moveAxisForward = moveInput.x;
        playerCharacterInputs.moveAxisRight = moveInput.y;
        playerCharacterInputs.cameraRotation = followedCamera.transform.rotation;
        characterController.SetInputs(ref playerCharacterInputs);
    }

    /// <summary>
    /// 根据镜头类型选择更新哪个动画（和数值）
    /// </summary>
    void UpdateAnimationRefreshMode()
    {
        if (cameraMode == CameraMode.UnLock)
        {
            // 对移动动画插值(未锁定视角)
            if (moveInput.x == 0 && moveInput.y == 0)
                forwardSpeed = Mathf.Lerp(forwardSpeed, forwardTargetSpeed, 1 - allInputTime);
            else
                forwardSpeed = Mathf.Lerp(forwardSpeed, forwardTargetSpeed, allInputTime);
            animator.SetFloat("VerticalSpeed", forwardSpeed);
        }
        else
        {
            // 对移动动画插值(锁定视角)
            if (moveInput.y == 0)
                verticalSpeed = Mathf.Lerp(verticalSpeed, verticalTargetSpeed, 1 - xInputTime);
            else
                verticalSpeed = Mathf.Lerp(verticalSpeed, verticalTargetSpeed, xInputTime);
            if (moveInput.x == 0)
                horizontalSpeed = Mathf.Lerp(horizontalSpeed, HorizontalTargetSpeed, 1 - zInputTime);
            else
                horizontalSpeed = Mathf.Lerp(horizontalSpeed, HorizontalTargetSpeed, zInputTime);
            animator.SetFloat("VerticalSpeed", horizontalSpeed);
            animator.SetFloat("HorizontalSpeed", verticalSpeed);
        }
    }

    /// <summary>
    /// 把玩家输入累计起来
    /// </summary>
    void UpdateInputTime()
    {
        if (moveInput != Vector2.zero)
            allInputTime += Time.fixedDeltaTime;
        else
            allInputTime -= Time.fixedDeltaTime * 2;
        allInputTime = Mathf.Clamp01(allInputTime);
        if (moveInput.y != 0)
            xInputTime += Time.fixedDeltaTime;
        else
            xInputTime -= Time.fixedDeltaTime * 2;
        xInputTime = Mathf.Clamp01(xInputTime);
        if (moveInput.x != 0)
            zInputTime += Time.fixedDeltaTime;
        else
            zInputTime -= Time.fixedDeltaTime * 2;
        zInputTime = Mathf.Clamp01(zInputTime);
    }

    /// <summary>
    /// 切换锁定敌人/未锁定状态
    /// </summary>
    /// <param name="context"></param>
    public void LockTarget(InputAction.CallbackContext context)
    {
        if (cameraMode == CameraMode.UnLock)
        {
            if (nearestEnemy != null)
            {
                cameraTargetGroup.AddMember(nearestEnemy.transform, 0.5f, 0);
                cameraMode = CameraMode.Lock;
                animator.SetBool("CameraMode", true);
                lockCamera.Priority = 10;
                unLockCamera.Priority = 5;
            }
        }
        else
        {
            cameraTargetGroup.RemoveMember(cameraTargetGroup?.m_Targets[1].target);
            cameraMode = CameraMode.UnLock;
            animator.SetBool("CameraMode", false);
            lockCamera.Priority = 5;
            unLockCamera.Priority = 10;
        }

    }

    void UpdatePlayerMoveState()
    {
        playerMoveState = playerMoveState != PlayerMoveState.Dash ? (moveInput.x != 0 || moveInput.y != 0) ? PlayerMoveState.Run : PlayerMoveState.Idle : PlayerMoveState.Dash;
        playerMoveState = (moveInput.x == 0 && moveInput.y == 0) ? PlayerMoveState.Idle : playerMoveState;

        forwardTargetSpeed = (playerMoveState == PlayerMoveState.Dash) && (moveInput.x != 0 || moveInput.y != 0) ? 2f : forwardTargetSpeed;

        characterController.maxMoveSpeed = playerMoveState == PlayerMoveState.Dash ? 3.84f * 2 : 3.84f;
    }
}
