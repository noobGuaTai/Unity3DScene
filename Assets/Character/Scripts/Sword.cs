using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    public int ENEMYLAYER = 6;
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == ENEMYLAYER)
        {
            print("Attack");
        }
    }
}
