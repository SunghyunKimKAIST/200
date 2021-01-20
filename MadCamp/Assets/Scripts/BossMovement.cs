using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class BossMovement : NetworkBehaviour
{
    public int health;

    public GameObject bulletPrefab;

    Rigidbody2D rigid;
    NetworkAnimator anim;
    SpriteRenderer spriteRenderer;
    CapsuleCollider2D capsuleCollider;

    GameManager gameManager;

    float xVelocity;
    int patternIndex;

    [SyncVar(hook = "FlipXHook")]
    bool flipX;
    void FlipXHook(bool flipX)
    {
        this.flipX = flipX;

        if(spriteRenderer != null)
            spriteRenderer.flipX = flipX;
    }

    Coroutine currentPattern;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (isServer)
        {
            rigid = GetComponent<Rigidbody2D>();
            anim = GetComponent<NetworkAnimator>();
            capsuleCollider = GetComponent<CapsuleCollider2D>();

            gameManager = FindObjectOfType<GameManager>();

            patternIndex = 0;
            currentPattern = StartCoroutine(PatternSlide(8, 1, 2));

            flipX = true;
        }
    }

    void FixedUpdate()
    {
        if (!isServer)
            return;

        // Move
        rigid.velocity = new Vector2(xVelocity, rigid.velocity.y);

        if (patternIndex == 1)
        {
            // Platform check
            Vector2 frontVec = new Vector2(rigid.position.x + xVelocity * 0.3f, rigid.position.y);
            Debug.DrawRay(frontVec, Vector3.down, new Color(0, 1, 0));
            RaycastHit2D rayHit = Physics2D.Raycast(frontVec, Vector3.down, 1, LayerMask.GetMask("Platform"));

            if (rayHit.collider == null)
            {
                Turn();
            }
        }
    }

    [ClientRpc]
    void RpcOnDamaged()
    {
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);

        if (!isServer)
        {
            // Enemy damaged
            gameObject.layer = 12;
            StartCoroutine(OffDamaged(2));
        }
    }

    [Server]
    public void OnDamaged(int damage)
    {
        health -= damage;

        gameObject.layer = 12;
        StartCoroutine(OffDamaged(2));
        RpcOnDamaged();

        if (health <= 0)
        {
            StopCoroutine(currentPattern);

            // Play hurt animation
            anim.SetTrigger("isDead");
            anim.animator.SetTrigger("isDead");
            Destroy(gameObject, 1);
            StartCoroutine(NextStage(0.6f));

            return;
        }

        if (patternIndex < 1 && health <= 50)
        {
            StopCoroutine(currentPattern);
            xVelocity = 0;

            patternIndex = 1;

            currentPattern = StartCoroutine(PatternBullet(10, 2, 2));
        }
    }

    [Server]
    IEnumerator NextStage(float time)
    {
        yield return new WaitForSeconds(time);

        gameManager.NextStage();
    }

    // All
    IEnumerator OffDamaged(float time)
    {
        yield return new WaitForSeconds(time);

        if (isClient)
            spriteRenderer.color = new Color(1, 1, 1, 1);

        // Enemy
        gameObject.layer = 9;
    }

    [Server]
    IEnumerator PatternSlide(int speed, int direction, float slideTime)
    {
        while (true)
        {
            flipX = direction == 1;
            xVelocity = speed * direction;
            yield return new WaitForSeconds(slideTime / 2);

            rigid.AddForce(Vector2.up * 20, ForceMode2D.Impulse);
            yield return new WaitForSeconds(slideTime / 2);

            direction *= -1;
        }
    }

    [Server]
    IEnumerator PatternBullet(float speed, float intervalTime, float destroyTime)
    {
        yield return new WaitForSeconds(1);
        Think();

        while (true)
        {
            Vector2[] bulletVector = { Vector2.left, Vector2.up, Vector2.right };

            for (int i = 0; i < 3; i++)
            {
                GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                NetworkServer.Spawn(bullet);

                Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
                rigid.velocity = bulletVector[i] * speed;

                Destroy(bullet, destroyTime);
            }

            yield return new WaitForSeconds(intervalTime);
        }
    }

    // 재귀 함수
    [Server]
    void Think()
    {
        // Set Next Active
        xVelocity = Random.Range(-1, 2);

        //Sprite Animation
        anim.animator.SetFloat("WalkSpeed", xVelocity);

        //Flip Sprite
        if (xVelocity != 0)
            spriteRenderer.flipX = xVelocity == 1;

        // Set Next Active
        float nextThinkTime = Random.Range(2f, 5f);
        Invoke("Think", nextThinkTime);
    }

    [Server]
    void Turn()
    {
        xVelocity = xVelocity * -1;
        spriteRenderer.flipX = xVelocity == 1;

        CancelInvoke();
        Invoke("Think", 2);
    }
}