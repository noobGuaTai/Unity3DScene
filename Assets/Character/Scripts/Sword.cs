using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    public GameObject player;
    public int ENEMYLAYER = 6;
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == ENEMYLAYER)
        {
            other.gameObject.GetComponent<EnemyAttribute>().UnderAttack(player.GetComponent<PlayerAttribute>().attack);
        }
    }
}
