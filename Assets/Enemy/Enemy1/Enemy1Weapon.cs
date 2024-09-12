using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy1Weapon : MonoBehaviour
{
    public GameObject enemy;

    void Start()
    {
        enemy = GameObject.FindGameObjectWithTag("Enemy1");
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            other.gameObject.GetComponent<PlayerAttribute>().UnderAttack(enemy.GetComponent<EnemyAttribute>().attack, "UnderLightAttack");
        }
    }
}
