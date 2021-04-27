using UnityEngine;

public class FirstPersonMovement : MonoBehaviour
{
    public float speed = 5;
    Vector2 velocity;
    Animator Anim;
    MagicScript MGS;
    private void Awake()
    {
        Anim = GetComponent<Animator>();
        MGS = GetComponent<MagicScript>();

    }
    void Update()
    {
        BWalk();
        Walk();
    }
    void Walk()
    {
        if (Input.GetKey(KeyCode.W)|| Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.A))
        { 
            Anim.SetBool("IsWalking", true);
            MGS.enabled = false;
            velocity.y = Input.GetAxis("Vertical") * speed * Time.deltaTime;
            velocity.x = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
            transform.Translate(velocity.x, 0, velocity.y);
        }
        else
        {
            MGS.enabled = true;
            Anim.SetBool("IsWalking", false);
        }
    }
    void BWalk()
    {
        if ( Input.GetKey(KeyCode.S))
        {
            Anim.SetBool("IsBackWalk", true);
            MGS.enabled = false;
            velocity.y = Input.GetAxis("Vertical") * -speed * Time.deltaTime;
            velocity.x = Input.GetAxis("Horizontal") * -speed * Time.deltaTime;
            transform.Translate(-velocity.x, 0, -velocity.y);
        }
        else
        {
            MGS.enabled = true;
            Anim.SetBool("IsBackWalk", false);
        }
    }

   
}
