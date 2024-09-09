using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy1IdleState : IState
{
    private Enemy1FSM enemy1FSM;
    private Enemy1Parameters parameters;

    public Enemy1IdleState(Enemy1FSM enemy1FSM)
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
        DetectPlayer();
    }

    /// <summary>
    /// 检测玩家是否在敌人前方的范围内
    /// </summary>
    void DetectPlayer()
    {
        if (parameters.distanceToPlayer <= parameters.detectionRange)
        {
            Vector3 forward = parameters.tf.forward;
            float angleToPlayer = Vector3.Angle(forward, parameters.directionToPlayer);

            if (angleToPlayer <= parameters.detectionAngle)
            {
                if (parameters.enemyState != Enemy1State.Chase)
                {
                    enemy1FSM.ChangeState(Enemy1State.Chase);
                }

                // RotateTowardsPlayer(parameters.directionToPlayer);
            }
        }
    }
}
