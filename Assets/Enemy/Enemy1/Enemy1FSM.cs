using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Enemy1State
{
    Idle,
    Chase,
    Attack
}

[Serializable]
public class Enemy1Parameters
{
    public GameObject player;
    public float detectionRange = 10f; // 检测范围
    public float detectionAngle = 45f; // 检测角度
    public float attackRange = 3f; // 攻击范围
    public float attackCoolDownMax = 6f;
    public bool canAttack = false;
    public float distanceToPlayer;
    public Enemy1State enemyState = Enemy1State.Idle;

    public Transform tf;
    public Animator animator;
    public float rotationSpeed = 5f; // 旋转速度
    public bool isAttacking = false;
    public Tween tween;
    public float animatorSpeedTarget;// 对动画的值插值的中间值
    public Vector3 directionToPlayer;
}
public class Enemy1FSM : MonoBehaviour
{
    public Enemy1Parameters parameters;
    public IState currentState;
    public Dictionary<Enemy1State, IState> state = new Dictionary<Enemy1State, IState>();

    void Awake()
    {
        parameters.tf = transform;
        parameters.animator = GetComponent<Animator>();
        parameters.tween = GetComponent<Tween>();
    }

    void Start()
    {
        state.Add(Enemy1State.Idle, new Enemy1IdleState(this));
        state.Add(Enemy1State.Chase, new Enemy1ChaseState(this));
        state.Add(Enemy1State.Attack, new Enemy1AttackState(this));
        ChangeState(Enemy1State.Idle);

    }

    void Update()
    {
        CalculateDistanceToPlayer();
        currentState.OnUpdate();
    }



    /// <summary>
    /// 旋转敌人朝向玩家
    /// </summary>
    public void RotateTowardsPlayer(Vector3 directionToPlayer)
    {
        // if (parameters.distanceToPlayer > parameters.attackRange)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * parameters.rotationSpeed);
        }
    }

    public void ChangeState(Enemy1State stateType)
    {
        if (currentState != null)
        {
            currentState.OnExit();
        }
        currentState = state[stateType];
        parameters.enemyState = stateType;
        currentState.OnEnter();
    }

    void CalculateDistanceToPlayer()
    {
        // 计算敌人到玩家的方向向量
        parameters.directionToPlayer = parameters.player.transform.position - parameters.tf.position;
        parameters.directionToPlayer.y = 0; // 忽略垂直方向的差异，保持水平检测
        parameters.distanceToPlayer = Vector3.Distance(parameters.tf.position, parameters.player.transform.position);
    }

    private void EnterChaseState()
    {
        currentState.OnExit();
        currentState = state[Enemy1State.Chase];
        parameters.enemyState = Enemy1State.Chase;
        currentState.OnEnter();
    }

    void ClearAttackSign()
    {
        parameters.animator.SetBool("Attack", false);
    }

}
