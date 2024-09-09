using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttribute : MonoBehaviour
{
    public float health;
    public float attack;
    public float healthMax;

    public virtual void UnderAttack(float value)
    {
        
    }
}
