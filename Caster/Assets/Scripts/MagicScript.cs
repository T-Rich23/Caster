using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicScript : MonoBehaviour
{
    
   [SerializeField]
    public Transform handEnd;
    public GameObject baseAtk;
    public GameObject midAtk;
    public GameObject heavyAtk;
    public GameObject ultAtk;
    public GameObject leftHand, rightHand;
    [SerializeField]ParticleSystem.MainModule leftHandParticle;
    [SerializeField]ParticleSystem.MainModule rightHandParticle;
    public bool canShoot;
    public float castTime;
    public float nextCast;
    GameObject currSpell;
    Animator anim;
   
    public int destroyTime = 3;
    // Start is called before the first frame update
    void Awake()
    {
        leftHandParticle = leftHand.GetComponent<ParticleSystem>().main;
        rightHandParticle = rightHand.GetComponent<ParticleSystem>().main;
        anim = GetComponent<Animator>();
        currSpell = baseAtk;
        canShoot = true;
    }

    // Update is called once per frame
    void Update()
    {
        Cast();
        CastTime();
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currSpell = baseAtk;
            leftHandParticle.startColor = Color.red;
            rightHandParticle.startColor = Color.red;

        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currSpell = midAtk;
            leftHandParticle.startColor = Color.blue;
            rightHandParticle.startColor = Color.blue;

        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            currSpell = heavyAtk;
            leftHandParticle.startColor = Color.cyan;
            rightHandParticle.startColor = Color.cyan;

        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            currSpell = ultAtk;
            leftHandParticle.startColor = Color.Lerp(Color.blue, Color.magenta, .3f);
            rightHandParticle.startColor = Color.Lerp(Color.blue,Color.magenta,.3f);
        }
       
    }

    public void Cast()
    {
        if (Time.time > nextCast)
        {
            if (Input.GetMouseButtonDown(0) && canShoot)
            {
                anim.SetBool("IsAttacking", true);
                GameObject bullet = Instantiate(currSpell, handEnd.position, handEnd.rotation);
                bullet.GetComponent<Rigidbody>().AddForce(handEnd.forward * 200, ForceMode.Impulse);
                Destroy(bullet, destroyTime);
                nextCast = Time.time + castTime;
            }
            else
            {
                anim.SetBool("IsAttacking", false);
            }
        }
    }
    public void CastTime()
    {
        if (currSpell == baseAtk)
        {
            castTime = .20f;
        }
        if (currSpell == midAtk)
        {
            castTime = .60f;
        }
        if (currSpell == heavyAtk)
        {
            castTime = 1f;
        }
        if (currSpell == ultAtk)
        {
            castTime = 1.4f;
        }
    }

}
