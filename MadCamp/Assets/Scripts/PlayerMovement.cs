using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PlayerMovement : NetworkBehaviour
{
    public int health;

    public Image[] UIHealth;
    public Text UIPoint;
    public Text UIStage;
    public GameObject UIGameover;
    public GameObject UIGameClear;

    public Slider UIEagleHealth;

    public Transform attackPoint;
    public LayerMask enemyLayers;
    public float maxSpeed;
    public float jumpPower;

    public float attackRange = 0.5f;
    public int attackDamage = 40;

    public GameObject fireBallPrefab;

    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    NetworkAnimator anim;
    CapsuleCollider2D capsuleCollider;

    Vector2 initPosition;
    bool isJumping;
    bool isPassing;
    bool flower;

    bool gameClear;

    [SyncVar(hook = "FlipXHook")]
    bool flipX;
    void FlipXHook(bool flipX)
    {
        this.flipX = flipX;

        if (spriteRenderer != null)
            spriteRenderer.flipX = flipX;
    }

    [Command]
    void CmdFlipX(bool flipX)
    {
        this.flipX = flipX;
    }

    // Server
    GameManager gameManager;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();

        if (isClient)
        {
            isJumping = false;
            isPassing = false;
        }

        if (isServer || hasAuthority)
            rigid = GetComponent<Rigidbody2D>();

        if (hasAuthority)
        {
            anim = GetComponent<NetworkAnimator>();

            initPosition = transform.position;
            flower = false;
        }
        else
        {
            transform.GetChild(0).gameObject.SetActive(false);
            gameObject.layer = 13;
        }

        if (isServer)
        {
            gameManager = FindObjectOfType<GameManager>();

            gameClear = false;
        }
    }

    [Command]
    void CmdAttack()
    {
        //Detect enemies in range of attack
        Vector3 newAttackPoint = attackPoint.position;
        newAttackPoint.x += spriteRenderer.flipX ? -2 : 0;
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(newAttackPoint, attackRange, enemyLayers);

        //Damage them
        foreach (Collider2D enemy in hitEnemies)
            OnAttack(enemy.transform);
    }

    [Command]
    void CmdFireBall(int angle)
    {
        GameObject fireBall = Instantiate(fireBallPrefab, transform.position, Quaternion.identity);
        NetworkServer.Spawn(fireBall);

        Rigidbody2D rigid = fireBall.GetComponent<Rigidbody2D>();
        rigid.velocity = (Vector2)(Quaternion.Euler(0, 0, angle) * Vector2.right) * 20;

        Destroy(fireBall, 3);
    }

    void Update()
    {
        if (!hasAuthority)
            return;

        //Attack
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (flower)
            {
                int horizontal = (int)Input.GetAxisRaw("Horizontal");
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    switch (horizontal)
                    {
                        case -1:
                            CmdFireBall(120);
                            break;

                        case 0:
                            CmdFireBall(90);
                            break;

                        case 1:
                            CmdFireBall(30);
                            break;
                    }
                }
                else
                {
                    switch (horizontal)
                    {
                        case -1:
                            CmdFireBall(180);
                            break;

                        case 0:
                            CmdFireBall(spriteRenderer.flipX ? 180 : 0);
                            break;

                        case 1:
                            CmdFireBall(0);
                            break;
                    }
                }
            }
            else
            {
                //Play an attack animation
                anim.SetTrigger("Attack");
                anim.animator.SetTrigger("Attack");

                CmdAttack();
            }
        }

        //Stop Speed
        if (Input.GetButtonUp("Horizontal"))
        {
            rigid.velocity = new Vector2(rigid.velocity.normalized.x * 0.5f, rigid.velocity.y);
        }

        //Direction Sprite
        if (Input.GetButton("Horizontal"))
        {
            bool newFlipX = Input.GetAxisRaw("Horizontal") == -1;
            if (spriteRenderer.flipX != newFlipX)
                CmdFlipX(newFlipX);
        }

        //Animation
        if (Mathf.Abs(rigid.velocity.x) < 0.3)
            anim.animator.SetBool("isWalking", false);
        else
            anim.animator.SetBool("isWalking", true);

        if (isJumping && !isPassing)
        {
            ContactFilter2D contactFilter = new ContactFilter2D();
            contactFilter.SetLayerMask(LayerMask.GetMask("Platform"));
            Collider2D[] results = new Collider2D[9];

            if(capsuleCollider.OverlapCollider(contactFilter, results) != 0)
            {
                if (gameObject.layer == 10) // Player
                    gameObject.layer = 13; // PlayerPassing
                else
                    gameObject.layer = 14; // PlayerPassingDamaged

                isPassing = true;
            }
        }
        else if (isPassing)
        {
            ContactFilter2D contactFilter = new ContactFilter2D();
            contactFilter.SetLayerMask(LayerMask.GetMask("Platform"));
            Collider2D[] results = new Collider2D[9];

            if (capsuleCollider.OverlapCollider(contactFilter, results) == 0)
            {
                if (gameObject.layer == 13) // PlayerPassing
                    gameObject.layer = 10; // Player
                else
                    gameObject.layer = 11; // PlayerDamaged

                isPassing = false;
            }
        }

        //Jump
        if (Input.GetButtonDown("Jump") && !isJumping)
        {
            rigid.position += Vector2.up * 0.2f;
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            isJumping = true;
            anim.animator.SetBool("isJumping", true);
        }

        if(Input.GetKeyDown(KeyCode.DownArrow) && !isPassing && transform.position.y > 1)
        {
            if (gameObject.layer == 10) // Player
                gameObject.layer = 13; // PlayerPassing
            else
                gameObject.layer = 14; // PlayerPassingDamaged

            isPassing = true;

        }
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
        if(!isPassing && rigid.velocity.y < 0)
        {
            int layerMask = LayerMask.GetMask("Platform", "Player", "PlayerDamaged");
            Collider2D hit = Physics2D.OverlapBox(rigid.position + new Vector2(0, -1.4f), new Vector2(0.8f, 0.5f), 0, layerMask);
            //RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector3.down, 2, layerMask);
            if (hit != null)
            {
                isJumping = false;
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

        transform.position = initPosition;
        rigid.velocity = Vector2.zero;
    }

    [ClientRpc]
    public void RpcHealthDown()
    {
        if (!hasAuthority)
            return;

        health--;
        if (health >= 0)
            UIHealth[health].color = new Color(1, 1, 1, 0.2f);

        if (health <= 0)
            CmdGameover();
    }

    [Command]
    void CmdGameover()
    {
        gameManager.Gameover();
    }

    [Server]
    public void Gameover()
    {
        capsuleCollider.enabled = false;

        RpcGameover();
    }

    [ClientRpc]
    void RpcGameover()
    {
        if(!isServer)
            capsuleCollider.enabled = false;

        if (hasAuthority)
            UIGameover.SetActive(true);

        //Sprite Alpha
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);
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
                if (transform.position.y > collision.transform.position.y + 0.2)
                {
                    OnAttack(collision.transform);
                    RpcStepOn();
                }
                else //Damaged
                    OnDamaged(collision.transform.position);
                break;

            case "Boss":
                //Attack
                if (transform.position.y > collision.transform.position.y + 0.2)
                {
                    OnAttackBoss(collision.transform);
                    RpcStepOn();
                }
                else //Damaged
                    OnDamaged(collision.transform.position);
                break;

            case "Eagle":
                //Attack
                if (transform.position.y > collision.transform.position.y)
                {
                    OnAttackEagle(collision.transform);
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
                gameManager.AddPoint(100);
                Destroy(collision.gameObject);
                break;

            case "Finish":
                // Next Stage
                gameManager.NextStage();
                break;

            case "Boss Bullet":
                if(!gameClear)
                    OnDamaged(collision.transform.position);
                break;

            case "Flower":
                flower = true;
                gameManager.AddPoint(100);
                Destroy(collision.gameObject);
                break;

        }
    }

    [Server]
    void OnAttack(Transform enemy)
    {
        // Point
        gameManager.AddPoint(100);

        // Enemy Die
        EnemyMovement enemyMove = enemy.GetComponent<EnemyMovement>();
        enemyMove.OnDamaged(attackDamage);
    }

    [Server]
    void OnAttackBoss(Transform boss)
    {
        // Point
        gameManager.AddPoint(100);

        BossMovement bossMove = boss.GetComponent<BossMovement>();
        bossMove.OnDamaged(attackDamage);
    }

    [Server]
    void OnAttackEagle(Transform eagle)
    {
        // Point
        gameManager.AddPoint(100);

        EagleMovement eagleMove = eagle.GetComponent<EagleMovement>();
        eagleMove.OnDamaged(attackDamage);
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

        // View Alpha
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);

        if (!isServer)
        {
            //Change Layer (Immortal Active)
            if (gameObject.layer == 10) // Player
                gameObject.layer = 11; // PlayerDamaged
            else
                gameObject.layer = 14; // PlayerPassingDamaged

            Invoke("OffDamaged", 2);
        }
    }

    [Server]
    void OnDamaged(Vector2 targetPos)
    {
        //Health Down
        RpcHealthDown();

        //Change Layer (Immortal Active)
        if (gameObject.layer == 10) // Player
            gameObject.layer = 11; // PlayerDamaged
        else
            gameObject.layer = 14; // PlayerPassingDamaged

        Invoke("OffDamaged", 2);

        RpcOnDamaged(targetPos.x);
    }

    // All
    void OffDamaged()
    {
        if (isClient)
            spriteRenderer.color = new Color(1, 1, 1, 1);

        if (gameObject.layer == 11) // PlayerDamaged
            gameObject.layer = 10; // Player
        else
            gameObject.layer = 13; // PlayerPassing
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    [ClientRpc]
    public void RpcEagleSpawn()
    {
        if (!hasAuthority)
            return;

        UIEagleHealth.gameObject.SetActive(true);
    }

    [ClientRpc]
    public void RpcUIEagleHealth(int health)
    {
        if (!hasAuthority)
            return;

        if (health == 0)
            UIEagleHealth.gameObject.SetActive(false);

        UIEagleHealth.value = health;
    }

    [Server]
    public void GodMode()
    {
        gameClear = true;
    }

    [ClientRpc]
    public void RpcGameClear()
    {
        UIGameClear.SetActive(true);
    }
}