using UnityEngine;
using UnityEngine.Networking;

public class EnemyMovement : NetworkBehaviour
{
    public float nextMove;
    public int health;

    Rigidbody2D rigid;
    NetworkAnimator anim;
    SpriteRenderer spriteRenderer;
    CapsuleCollider2D capsuleCollider;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (isServer)
        {
            rigid = GetComponent<Rigidbody2D>();
            anim = GetComponent<NetworkAnimator>();
            capsuleCollider = GetComponent<CapsuleCollider2D>();

            Invoke("Think", 2);
        }
    }

    void FixedUpdate()
    {
        if (!isServer)
            return;

        rigid.velocity = new Vector2(nextMove, rigid.velocity.y);

        // Platform check
        Vector2 frontVec = new Vector2(rigid.position.x + nextMove * 0.3f, rigid.position.y);
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
        nextMove = Random.Range(-1, 2);

        //Sprite Animation
        anim.animator.SetFloat("WalkSpeed", nextMove);

        //Flip Sprite
        if (nextMove != 0 && spriteRenderer.flipX != (nextMove == 1))
            RpcFlipX();

        // Set Next Active
        float nextThinkTime = Random.Range(2f, 5f);
        Invoke("Think", nextThinkTime);
    }

    // Server
    void Turn()
    {
        nextMove = nextMove * -1;
        // TODO
        if (spriteRenderer.flipX != (nextMove == 1))
            RpcFlipX();

        CancelInvoke();
        Invoke("Think", 2);
    }

    // Server
    public void OnDamaged(int damage)
    {
        health -= damage;

        // Play hurt animation

        if(health <= 0)
        {
            OnDie();
        }
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

    [ClientRpc]
    void RpcFlipX()
    {
        spriteRenderer.flipX ^= true;
    }
}