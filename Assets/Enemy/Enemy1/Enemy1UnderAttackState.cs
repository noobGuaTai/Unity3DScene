using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy1UnderAttackState : IState
{
    private Enemy1FSM enemy1FSM;
    private Enemy1Parameters parameters;

    public Enemy1UnderAttackState(Enemy1FSM enemy1FSM)
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
        
    }


}
