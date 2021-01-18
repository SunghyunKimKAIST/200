using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PlayerMovement : NetworkBehaviour
{
    public int health;

    public Image[] UIHealth;
    public Text UIPoint;
    public Text UIStage;
    public GameObject UIRestartBtn;

    public Transform attackPoint;
    public LayerMask enemyLayers;
    public float maxSpeed;
    public float jumpPower;

    public float attackRange = 0.5f;
    public int attackDamage = 40;

    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    NetworkAnimator anim;
    CapsuleCollider2D capsuleCollider;

    // Server
    GameManager gameManager;

    void Start()
    {
        if(isClient)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (isServer || hasAuthority)
            rigid = GetComponent<Rigidbody2D>();

        if (hasAuthority)
        {
            anim = GetComponent<NetworkAnimator>();
            capsuleCollider = GetComponent<CapsuleCollider2D>();
        }
        else
            transform.GetChild(0).gameObject.SetActive(false);

        if (isServer)
            gameManager = FindObjectOfType<GameManager>();
    }

    [Command]
    void CmdAttack()
    {
        //Detect enemies in range of attack
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        //Damage them
        foreach (Collider2D enemy in hitEnemies)
            OnAttack(enemy.transform);
    }

    void Update()
    {
        if (!hasAuthority)
            return;

        //Attack
        if (Input.GetKeyDown(KeyCode.X))
        {
            //Play an attack animation
            anim.SetTrigger("Attack");
            anim.animator.SetTrigger("Attack");

            CmdAttack();
        }

        //Jump
        if (Input.GetButtonDown("Jump") && !anim.animator.GetBool("isJumping"))
        {
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            anim.animator.SetBool("isJumping", true);
        }

        //Stop Speed
        if (Input.GetButtonUp("Horizontal"))
        {
            rigid.velocity = new Vector2(rigid.velocity.normalized.x * 0.5f, rigid.velocity.y);
        }

        //Direction Sprite
        if (Input.GetButton("Horizontal"))
        {
            SyncFlipX(Input.GetAxisRaw("Horizontal") == -1);
        }

        //Animation
        if (Mathf.Abs(rigid.velocity.x) < 0.3)
            anim.animator.SetBool("isWalking", false);
        else
            anim.animator.SetBool("isWalking", true);
    }

    void FixedUpdate()
    {
        if (!hasAuthority)
            return;

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
            int layerMask = LayerMask.GetMask("Platform", "Player", "PlayerDamaged");
            RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector3.down, 1, layerMask);
            if (rayHit.collider != null)
            {
                if (rayHit.distance < 1)
                    anim.animator.SetBool("isJumping", false);
            }
        }
    }

    [ClientRpc]
    public void RpcPoint(int point)
    {
        if (!hasAuthority)
            return;

        UIPoint.text = point.ToString();
    }

    [ClientRpc]
    public void RpcStageIndex(int stageIndex)
    {
        if (!hasAuthority)
            return;

        UIStage.text = "Stage " + (stageIndex + 1);
    }

    [ClientRpc]
    public void RpcReposition()
    {
        if (!hasAuthority)
            return;

        transform.position = new Vector3(0, 0, -1);
        rigid.velocity = Vector2.zero;
    }

    [ClientRpc]
    public void RpcHealthDown()
    {
        if (!hasAuthority)
            return;

        HealthDown();
    }

    // Client only
    void SyncFlipX(bool flipX)
    {
        if(spriteRenderer.flipX != flipX)
        {
            spriteRenderer.flipX = flipX;
            CmdFlipX(flipX);
        }
    }

    [Command]
    void CmdFlipX(bool flipX)
    {
        RpcFlipX(flipX);
    }

    [ClientRpc]
    void RpcFlipX(bool flipX)
    {
        if(!hasAuthority)
            spriteRenderer.flipX = flipX;
    }

    // Client only
    void HealthDown()
    {
        health--;
        if (health >= 0)
            UIHealth[health].color = new Color(1, 1, 1, 0.2f);

        if (health <= 0)
        {
            //Player Die Effect
            OnDie();

            //Retry Button UI
            UIRestartBtn.SetActive(true);
        }
    }

    [ClientRpc]
    void RpcStepOn()
    {
        if (!hasAuthority)
            return;

        rigid.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isServer)
            return;

        switch (collision.gameObject.tag)
        {
            case "Enemy":
                //Attack
                if (rigid.velocity.y < 0 && transform.position.y > collision.transform.position.y)
                {
                    OnAttack(collision.transform);
                    RpcStepOn();
                }
                else //Damaged
                    OnDamaged(collision.transform.position);
                break;

            case "Boss":
                //Attack
                if (transform.position.y > collision.transform.position.y)
                {
                    OnAttackBoss(collision.transform);
                    RpcStepOn();
                }
                else //Damaged
                    OnDamaged(collision.transform.position);
                break;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isServer)
            return;

        switch (collision.gameObject.tag)
        {
            case "Item":
                // bool isApple = collision.gameObject.name.Contains("Apple");

                gameManager.AddPoint(100);

                collision.gameObject.GetComponent<Item>().OffActive();
                break;

            case "Finish":
                // Next Stage
                gameManager.NextStage();
                break;

            case "Boss Bullet":
                OnDamaged(collision.transform.position);
                break;
        }
    }

    // Server
    void OnAttack(Transform enemy)
    {
        // Point
        gameManager.AddPoint(100);

        // Enemy Die
        EnemyMovement enemyMove = enemy.GetComponent<EnemyMovement>();
        enemyMove.OnDamaged(attackDamage);
    }

    // Server
    void OnAttackBoss(Transform boss)
    {
        // Point
        gameManager.AddPoint(100);

        BossMovement bossMove = boss.GetComponent<BossMovement>();
        bossMove.OnDamaged(attackDamage);
    }

    [ClientRpc]
    void RpcOnDamaged(float x)
    {
        if (hasAuthority)
        {
            //Reaction Force
            int dirc = transform.position.x - x > 0 ? 1 : -1;
            rigid.AddForce(new Vector2(dirc, 1) * 7, ForceMode2D.Impulse);

            //Animation
            anim.SetTrigger("doDamaged");
            anim.animator.SetTrigger("doDamaged");
        }

        if (isClient)
        {
            // View Alpha
            spriteRenderer.color = new Color(1, 1, 1, 0.4f);
        }

        //Change Layer (Immortal Active)
        gameObject.layer = 11;

        Invoke("OffDamaged", 2);
    }

    // Server
    void OnDamaged(Vector2 targetPos)
    {
        //Health Down
        RpcHealthDown();

        RpcOnDamaged(targetPos.x);
    }

    // All
    void OffDamaged()
    {
        if (isClient)
            spriteRenderer.color = new Color(1, 1, 1, 1);

        // Player
        gameObject.layer = 10;
    }

    [Command]
    void CmdOnDie()
    {
        RpcOnDie();

        //Player Control Lock
        gameManager.Gameover();
    }

    [ClientRpc]
    void RpcOnDie()
    {
        //Sprite Alpha
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);

        //Sprite Flip Y
        spriteRenderer.flipY = true;
    }

    // Client only
    public void OnDie()
    {
        CmdOnDie();

        //Die Effect Jump
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    // TODO
    [Command]
    void CmdRestart()
    {
        gameManager.Restart();
    }

    // Client only
    public void Restart()
    {
        // CmdRestart();
    }
}