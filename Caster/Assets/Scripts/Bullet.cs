using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float radius;

    public void Awake()
    {
        radius = 5f;
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            BaseHealth.bH.ApplyDamage(MagicScript.mS.atkDmg);
            //Explode();
        }
        else
        {
            return;
        }
    }
    void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].CompareTag("Enemy"))
            {
                Debug.Log(colliders[i].name);
                Destroy(gameObject);
            }
        }
    }
}