using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttribute : MonoBehaviour
{
    public float health = 100f;
    public float attack = 10f;
    public float healthMax = 100f;
    public bool defended = false;// 防御状态下被攻击
    public float defendedValue;// 防御状态下被攻击的伤害

    private PlayerInputController playerInputController;

    void Start()
    {
        playerInputController = GetComponent<PlayerInputController>();
    }

    public void UnderAttack(float value, string type)
    {
        if (playerInputController.playerState != PlayerState.Defense)
        {
            playerInputController.animator.SetTrigger(type);
            ChangeHealth(value);
        }
        else
        {
            defended = true;
            defendedValue = value;
        }

    }

    public void ChangeHealth(float value)
    {
        health -= value;
        if (health <= 0f)
            Die();
        Mathf.Clamp(health, 1f, healthMax);
    }

    void Die()
    {

    }
}
