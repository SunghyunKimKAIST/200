using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class EnemyMovement : NetworkBehaviour
{
    public int health;

    Rigidbody2D rigid;
    NetworkAnimator anim;
    SpriteRenderer spriteRenderer;
    CapsuleCollider2D capsuleCollider;

    float xVelocity;
    [SyncVar(hook = "FlipXHook")]
    bool flipX;
    void FlipXHook(bool flipX)
    {
        this.flipX = flipX;

        if (spriteRenderer != null)
            spriteRenderer.flipX = flipX;
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (isServer)
        {
            rigid = GetComponent<Rigidbody2D>();
            anim = GetComponent<NetworkAnimator>();
            capsuleCollider = GetComponent<CapsuleCollider2D>();

            Invoke("Think", 2);

            flipX = true;
        }
    }

    void FixedUpdate()
    {
        if (!isServer)
            return;

        rigid.velocity = new Vector2(xVelocity, rigid.velocity.y);

        // Platform check
        Vector2 frontVec = new Vector2(rigid.position.x + xVelocity * 0.3f, rigid.position.y);
        Debug.DrawRay(frontVec, Vector3.down, new Color(0, 1, 0));
        RaycastHit2D rayHit = Physics2D.Raycast(frontVec, Vector3.down, 1, LayerMask.GetMask("Platform"));

        if (rayHit.collider == null)
        {
            Turn();
        }
    }

    [Server]
    // 재귀 함수
    void Think()
    {
        // Set Next Active
        xVelocity = Random.Range(-1, 2);

        //Sprite Animation
        anim.animator.SetFloat("WalkSpeed", xVelocity);

        //Flip Sprite
        flipX = xVelocity == 1;


        // Set Next Active
        float nextThinkTime = Random.Range(2f, 5f);
        Invoke("Think", nextThinkTime);
    }

    [Server]
    void Turn()
    {
        xVelocity = xVelocity * -1;
        flipX = xVelocity == 1;

        CancelInvoke();
        Invoke("Think", 2);
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
            // Play hurt animation
            anim.SetTrigger("isDead");
            anim.animator.SetTrigger("isDead");
            Destroy(gameObject, 0.6f);
            return;
        }
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
}