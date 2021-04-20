using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseHealth : MonoBehaviour
{
    [SerializeField]
    public int maxHealth;
    public int currentHealth;
    public Transform respawnMarker = null;


    void Start()
    {
        currentHealth = maxHealth;
    }

    public void ApplyDamage(int amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            currentHealth = 0;

            if (respawnMarker)
            {
                if (transform.GetComponent<Rigidbody>() != null)
                    transform.GetComponent<Rigidbody>().velocity = Vector3.zero;

                transform.position = respawnMarker.position;
                currentHealth = maxHealth;
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}