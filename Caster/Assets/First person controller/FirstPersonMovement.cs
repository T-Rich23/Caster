using UnityEngine;

public class FirstPersonMovement : MonoBehaviour
{
    public float speed = 5;
    Vector2 velocity;
    Animator Anim;
    private void Awake()
    {
        Anim = GetComponent<Animator>();
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
            velocity.y = Input.GetAxis("Vertical") * speed * Time.deltaTime;
            velocity.x = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
            transform.Translate(velocity.x, 0, velocity.y);
        }
        else
        {
            Anim.SetBool("IsWalking", false);
        }
    }
    void BWalk()
    {
        if ( Input.GetKey(KeyCode.S))
        {
            Anim.SetBool("IsBackWalk", true);
            velocity.y = Input.GetAxis("Vertical") * -speed * Time.deltaTime;
            velocity.x = Input.GetAxis("Horizontal") * -speed * Time.deltaTime;
            transform.Translate(-velocity.x, 0, -velocity.y);
        }
        else
        {
            Anim.SetBool("IsBackWalk", false);
        }
    }

   
}
