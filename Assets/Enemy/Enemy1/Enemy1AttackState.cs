using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy1AttackState : IState
{
    private Enemy1FSM enemy1FSM;
    private Enemy1Parameters parameters;


    public Enemy1AttackState(Enemy1FSM enemy1FSM)
    {
        this.enemy1FSM = enemy1FSM;
        this.parameters = enemy1FSM.parameters;
    }

    public void OnEnter()
    {
        parameters.animator.SetFloat("Move", 0f);
    }

    public void OnExit()
    {

    }

    public void OnFixedUpdate()
    {

    }

    public void OnUpdate()
    {
        if (!parameters.animator.GetBool("Attack") && parameters.canAttack && parameters.distanceToPlayer > parameters.attackRange)
        {
            enemy1FSM.ChangeState(Enemy1State.Chase);
            return;
        }

        if (parameters.canAttack)
            Attack();

        if (!parameters.animator.GetBool("Attack"))
            enemy1FSM.RotateTowardsPlayer(parameters.directionToPlayer);
    }

    /// <summary>
    /// 攻击状态下攻击玩家，并启动冷却时间
    /// </summary>
    void Attack()
    {
        parameters.animator.SetBool("Attack", true);
        parameters.canAttack = false;
        float coolDownDuration = Random.Range(3f, parameters.attackCoolDownMax);
        enemy1FSM.StartCoroutine(CoolDown(coolDownDuration));
    }

    /// <summary>
    /// 冷却协程，在冷却时间结束后允许再次攻击
    /// </summary>
    IEnumerator CoolDown(float duration)
    {
        yield return new WaitForSeconds(duration);
        parameters.canAttack = true;
    }
}
