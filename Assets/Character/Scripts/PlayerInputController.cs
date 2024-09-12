using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using KinematicCharacterController.Examples;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public enum CameraMode
{
    UnLock,
    Lock
}

public enum PlayerState
{
    Idle,
    Run,
    Dash,
    Attack,
    UnderAttack,
    Defense,
    Avoid
}

/// <summary>
/// 预输入类型
/// </summary>
public enum TypeAhead
{
    None,
    Attack,
    Defense,
    Move
}

public class PlayerInputController : MonoBehaviour
{
    public Camera followedCamera;
    public Animator animator;
    public CameraMode cameraMode = CameraMode.UnLock;
    public PlayerState playerState = PlayerState.Idle;
    public float allInputTime = 0f;// 玩家输入的时长(x和y)
    public float xInputTime = 0f;// 玩家输入的时长(x)
    public float zInputTime = 0f;// 玩家输入的时长(z)
    public CinemachineTargetGroup cameraTargetGroup;
    public List<GameObject> visibleEnemy;
    public GameObject nearestDetectEnemy;
    public GameObject nearestSurroundEnemy;
    public Collider attackCollider;

    private CharacterController characterController;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private PlayerCharacterInputs playerCharacterInputs;
    private PlayerAttribute playerAttribute;
    private Transform selfTransform;
    public float forwardSpeed = 0f;  // 未锁定 当前速度(前后)
    public float forwardTargetSpeed = 0f; // 未锁定 目标速度(前后)

    private float verticalSpeed = 0f;  // 锁定目标 当前速度(前后)
    private float verticalTargetSpeed = 0f;  // 锁定目标 当前速度(前后)
    private float horizontalSpeed = 0f;  // 锁定目标 当前速度(左右)
    private float HorizontalTargetSpeed = 0f; // 锁定目标 目标速度(前后)

    private CinemachineVirtualCamera lockCamera;
    private CinemachineFreeLook unLockCamera;

    public bool isPressAttack = false;// 只要点过一下攻击键，就会置true
    private bool isPressDefense = false;// 只要点过一下防御键，就会置true
    private bool isPressAvoid = false;// 只要点过一下闪避键，就会置true

    public TypeAhead typeAhead = TypeAhead.None;// 预输入类型
    private bool hasTypeAhead = false;// 是否有预输入

    void Start()
    {
        selfTransform = transform;
        characterController = GetComponent<CharacterController>();
        playerCharacterInputs = new PlayerCharacterInputs();
        lockCamera = GameObject.Find("LockCamera").GetComponent<CinemachineVirtualCamera>();
        unLockCamera = GameObject.Find("UnLockCamera").GetComponent<CinemachineFreeLook>();
        playerAttribute = GetComponent<PlayerAttribute>();
    }

    void Update()
    {
        HandlePlayerInput();
        UpdateInputTime();
        UpdatePlayerState();
    }

    void FixedUpdate()
    {
        UpdateAnimationRefreshMode();
    }

    /// <summary>
    /// 新输入系统的触发移动
    /// </summary>
    /// <param name="context"></param>
    public void PlayerMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        forwardTargetSpeed = (moveInput.x != 0 || moveInput.y != 0) ? 3f : 0f;// 很奇怪，moveInput.y是前后移动的输入
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
        if (playerState == PlayerState.Run)
        {
            playerState = PlayerState.Dash;
        }
    }

    public void PlayerAttack(InputAction.CallbackContext context)
    {
        isPressAttack = context.ReadValueAsButton() == true ? true : isPressAttack;
        if (isPressAttack && playerState != PlayerState.Attack)
        {
            animator.SetBool("Attack", true);
        }
    }

    public void PlayerDefense(InputAction.CallbackContext context)
    {
        isPressDefense = context.ReadValueAsButton();
        if (playerState != PlayerState.Attack && isPressDefense)
        {
            animator.SetBool("Defense", isPressDefense);
            playerState = PlayerState.Defense;
        }

        // 退出防御判定
        if (!isPressDefense && playerState == PlayerState.Defense)
        {
            playerState = PlayerState.Idle;
            animator.SetBool("Defense", isPressDefense);
        }
    }

    public void PlayerAvoid(InputAction.CallbackContext context)
    {
        isPressAvoid = context.ReadValueAsButton() == true ? true : isPressAvoid;
        if (isPressAvoid && playerState != PlayerState.Avoid && playerState != PlayerState.UnderAttack)
        {
            animator.SetBool("Avoid", true);
            playerState = PlayerState.Avoid;
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
        if (cameraMode == CameraMode.UnLock || playerState == PlayerState.Dash)
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
            animator.SetFloat("VerticalSpeed", verticalSpeed);
            animator.SetFloat("HorizontalSpeed", horizontalSpeed);
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
            if (nearestDetectEnemy != null)
            {
                LockCamera();
            }
        }
        else
        {
            UnLockCamera();
        }

    }

    public void LockCamera()
    {
        cameraTargetGroup.AddMember(nearestDetectEnemy.transform, 0.5f, 0);
        cameraMode = CameraMode.Lock;
        animator.SetBool("LockCamera", true);
        lockCamera.Priority = 10;
        unLockCamera.Priority = 5;
    }

    public void UnLockCamera()
    {
        cameraTargetGroup.RemoveMember(cameraTargetGroup?.m_Targets[1].target);
        cameraMode = CameraMode.UnLock;
        animator.SetBool("LockCamera", false);
        lockCamera.Priority = 5;
        unLockCamera.Priority = 10;
    }

    /// <summary>
    /// 判断玩家的运动状态
    /// </summary>
    void UpdatePlayerState()
    {
        // 疾跑判断
        if (playerState != PlayerState.Attack && playerState != PlayerState.Defense)
        {
            playerState = playerState != PlayerState.Dash ? (moveInput.x != 0 || moveInput.y != 0) ? PlayerState.Run : PlayerState.Idle : PlayerState.Dash;
            playerState = (moveInput.x == 0 && moveInput.y == 0) ? PlayerState.Idle : playerState;
        }

        forwardTargetSpeed = (playerState == PlayerState.Dash) && (moveInput.x != 0 || moveInput.y != 0) ? 4f : forwardTargetSpeed;

        animator.SetBool("IsDashing", playerState == PlayerState.Dash);
        animator.SetBool("Move", forwardTargetSpeed != 0);


    }

    #region AnimatorEvent
    /// <summary>
    /// 动画开始时/预输入开始时清除攻击预输入标记
    /// </summary>
    void ClearAttackSign()
    {
        isPressAttack = false;
        typeAhead = TypeAhead.None;
        hasTypeAhead = false;
        animator.SetBool("Attack", false);
    }

    /// <summary>
    /// 预输入判定，记录攻击/防御/移动输入,
    /// </summary>
    void StayTypeAhead()
    {
        if ((typeAhead == TypeAhead.None || typeAhead == TypeAhead.Move) && !hasTypeAhead)// 玩家喜欢按住移动键点攻击
        {
            if (isPressAttack)
            {
                typeAhead = TypeAhead.Attack;
                return;
            }
            if (isPressDefense)
            {
                typeAhead = TypeAhead.Defense;
                return;
            }
            if (forwardSpeed != 0)
            {
                typeAhead = TypeAhead.Move;
                return;
            }

        }
    }

    /// <summary>
    /// 根据预输入类型，判断进入哪一个状态
    /// </summary>
    void IsEnterNextState()
    {
        if (typeAhead != TypeAhead.None && !hasTypeAhead)
        {
            switch (typeAhead)
            {
                case TypeAhead.Attack:
                    typeAhead = TypeAhead.None;
                    animator.SetBool("Attack", true);
                    isPressAttack = false;
                    hasTypeAhead = true;
                    break;
                case TypeAhead.Defense:
                    typeAhead = TypeAhead.None;
                    animator.SetBool("Defense", isPressDefense);
                    playerState = PlayerState.Defense;
                    hasTypeAhead = true;
                    break;
                case TypeAhead.Move:
                    typeAhead = TypeAhead.None;
                    playerState = PlayerState.Idle;
                    hasTypeAhead = true;
                    break;
            }
        }
    }

    /// <summary>
    /// 攻击动画结尾退出攻击状态
    /// </summary>
    void ExitAttackStateFinal()
    {
        if (!hasTypeAhead)
        {
            playerState = PlayerState.Idle;
        }
    }

    /// <summary>
    /// 在攻击开始到攻击后摇结束，玩家处于攻击状态
    /// </summary>
    void EnterAttackState()
    {
        playerState = PlayerState.Attack;
    }

    /// <summary>
    /// 长按防御键，一直处于防御状态
    /// </summary>
    void StayDefenseAnimation()
    {
        // animator.speed = animator.GetBool("Defense") ? 0f : 1f;
        if (animator.GetBool("Defense"))
            animator.Play("defense_01", 0, 0.5f);
    }

    void EnableAttackDetermine()
    {
        attackCollider.enabled = true;
    }

    void DisableAttackDetermine()
    {
        attackCollider.enabled = false;
    }

    void NormalDefense()
    {
        if (playerAttribute.defended)
        {
            playerAttribute.ChangeHealth(playerAttribute.defendedValue / 10);
            playerAttribute.defended = false;
            print("normalDefense");
        }
    }

    void PerfectDefense()
    {
        if (playerAttribute.defended)
        {
            playerAttribute.defended = false;
            print("perfectDefense");
        }
    }

    void EnterUnderAttackState()
    {
        playerState = PlayerState.UnderAttack;
    }

    void ExitUnderAttackState()
    {
        playerState = PlayerState.UnderAttack;
    }

    void ExitAvoidState()
    {
        playerState = PlayerState.Idle;
        animator.SetBool("Avoid", false);
    }

    #endregion
}
