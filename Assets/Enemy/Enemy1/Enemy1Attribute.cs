using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy1Attribute : EnemyAttribute
{
    private Enemy1FSM enemy1FSM;

    void Start()
    {
        health = 100f;
        attack = 10f;
        healthMax = 100f;
        enemy1FSM = GetComponent<Enemy1FSM>();
    }

    public override void UnderAttack(float value)
    {
        base.UnderAttack(value);
        enemy1FSM.parameters.animator.SetTrigger("UnderAttack");
        health -= value;
        if (health <= 0f)
            Die();
        Mathf.Clamp(health, 1f, healthMax);
    }

    void Die()
    {
        enemy1FSM.parameters.animator.SetTrigger("Die");
        Destroy(gameObject, 2f);
    }
}
