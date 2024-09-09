using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy1ChaseState : IState
{
    private Enemy1FSM enemy1FSM;
    private Enemy1Parameters parameters;

    public Enemy1ChaseState(Enemy1FSM enemy1FSM)
    {
        this.enemy1FSM = enemy1FSM;
        this.parameters = enemy1FSM.parameters;
    }

    public void OnEnter()
    {
        parameters.animatorSpeedTarget = 0f;
        parameters.tween.AddTween((float a) =>
            {
                parameters.animatorSpeedTarget = a;
            }, 0f, 4f, 1f, Tween.TransitionType.QUART, Tween.EaseType.IN);
        parameters.tween.Play();
    }

    public void OnExit()
    {

    }

    public void OnFixedUpdate()
    {

    }

    public void OnUpdate()
    {
        if (parameters.distanceToPlayer > parameters.attackRange)
        {
            parameters.animator.SetFloat("Move", parameters.animatorSpeedTarget);
            enemy1FSM.RotateTowardsPlayer(parameters.directionToPlayer);
        }
        else
        {
            enemy1FSM.ChangeState(Enemy1State.Attack);
        }

        if(parameters.distanceToPlayer > parameters.detectionRange)
        {
            enemy1FSM.ChangeState(Enemy1State.Idle);
        }
    }


}
