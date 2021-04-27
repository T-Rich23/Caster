using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : BaseHealth
{
    public static EnemyHealth eH;
    private void Awake()
    {
        eH = this;
        maxHealth = 50;
        currentHealth = maxHealth;
    }
}