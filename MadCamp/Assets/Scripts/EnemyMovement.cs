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

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (isServer)
        {
            rigid = GetComponent<Rigidbody2D>();
            anim = GetComponent<NetworkAnimator>();
            capsuleCollider = GetComponent<CapsuleCollider2D>();

            SyncFlipX(true);

            Invoke("Think", 2);
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

    // Server
    // 재귀 함수
    void Think()
    {
        // Set Next Active
        xVelocity = Random.Range(-1, 2);

        //Sprite Animation
        anim.animator.SetFloat("WalkSpeed", xVelocity);

        //Flip Sprite
        SyncFlipX(xVelocity == 1);

        // Set Next Active
        float nextThinkTime = Random.Range(2f, 5f);
        Invoke("Think", nextThinkTime);
    }

    // Server
    void Turn()
    {
        xVelocity = xVelocity * -1;
        SyncFlipX(xVelocity == 1);

        CancelInvoke();
        Invoke("Think", 2);
    }

    [ClientRpc]
    void RpcOnDamaged()
    {
        // Enemy damaged
        gameObject.layer = 12;
        StartCoroutine(OffDamaged(2));
    }

    // Server
    public void OnDamaged(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            OnDie();
            return;
        }

        if (isClient)
            spriteRenderer.color = new Color(1, 1, 1, 0.4f);

        // Play hurt animation
        gameObject.layer = 12;
        StartCoroutine(OffDamaged(2));

        RpcOnDamaged();
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

    // Server
    void OnDie()
    {
        Debug.Log("Enemy died!");
        //Die animation
        anim.animator.SetBool("isDead", true);
        //Disable the enemy
        Destroy(gameObject, 2);
    }

    // Server
    void SyncFlipX(bool flipX)
    {
        if(spriteRenderer.flipX != flipX)
        {
            spriteRenderer.flipX = flipX;
            RpcFlipX(flipX);
        }
    }

    [ClientRpc]
    void RpcFlipX(bool flipX)
    {
        if(!isServer)
            spriteRenderer.flipX = flipX;
    }
}