using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float maxSpeed;
    public float jumpPower;
    public GameManager gameManager;

    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Animator anim;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        //Jump
        if (Input.GetButtonDown("Jump") && !anim.GetBool("isJumping"))
        {
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            anim.SetBool("isJumping", true);
        }

        //Stop Speed
        if (Input.GetButtonUp("Horizontal"))
        {
            rigid.velocity = new Vector2(rigid.velocity.normalized.x * 0.5f, rigid.velocity.y);
        }

        //Direction Sprite
        if (Input.GetButton("Horizontal"))
        {
            spriteRenderer.flipX = Input.GetAxisRaw("Horizontal") == -1;
        }

        //Animation
        if (Mathf.Abs(rigid.velocity.x) < 0.3)
            anim.SetBool("isWalking", false);
        else
            anim.SetBool("isWalking", true);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "BossHitDetection")
        {
            gameManager.HealthDown();
        }

        if(collision.gameObject.tag == "BossBullet")
        {
            gameManager.HealthDown();
        }
    }

    void FixedUpdate()
    {
        //Move Speed
        float h = Input.GetAxisRaw("Horizontal");
        rigid.AddForce(Vector2.right * h * 4, ForceMode2D.Impulse);

        //Max Speed
        if (rigid.velocity.x > maxSpeed) //Right MaxSpeed
            rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y);
        else if (rigid.velocity.x < maxSpeed*(-1)) //Left MaxSpeed
            rigid.velocity = new Vector2(maxSpeed*(-1), rigid.velocity.y);

        //Landing Platform
        if(rigid.velocity.y < 0)
        {
            Debug.DrawRay(rigid.position, Vector3.down, new Color(0, 1, 0));
            RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector3.down, 1, LayerMask.GetMask("Platform"));
            if (rayHit.collider != null)
            {
                if (rayHit.distance < 0.5f)
                    anim.SetBool("isJumping", false);
            }
        }
    }

    public void OnDie()
    {
        Destroy(gameObject);
    }
}