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
    public void Update()
    {
        if (currentHealth <= 0)
        {
            GameManager.Instance._coins += 50;
            Destroy(gameObject);
        }
    }
}