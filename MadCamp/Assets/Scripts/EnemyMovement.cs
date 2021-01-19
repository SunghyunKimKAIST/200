﻿using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float nextMove;
    public int maxHealth = 100;
    public int currentHealth;

    Rigidbody2D rigid;
    Animator anim;
    SpriteRenderer spriteRenderer;
    CapsuleCollider2D capsuleCollider;

    void Start()
    {
        currentHealth = maxHealth;
    }

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();

        Invoke("Think", 2);
    }

    void FixedUpdate()
    {
        // Move
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

    // 재귀 함수
    void Think()
    {
        // Set Next Active
        nextMove = Random.Range(-1, 2);

        //Sprite Animation
        anim.SetFloat("WalkSpeed", nextMove);

        //Flip Sprite
        if (nextMove != 0)
            spriteRenderer.flipX = nextMove == 1;

        // Set Next Active
        float nextThinkTime = Random.Range(2f, 5f);
        Invoke("Think", nextThinkTime);
    }

    void Turn()
    {
        nextMove = nextMove * -1;
        spriteRenderer.flipX = nextMove == 1;

        CancelInvoke();
        Invoke("Think", 2);
    }

    public void OnDamaged(int damage)
    {
        currentHealth = currentHealth - damage;

        // Play hurt animation

        if(currentHealth <= 0)
        {
            OnDie();
        }
    }

    void DeActive()
    {
        gameObject.SetActive(false);
        Destroy(gameObject, 5);
    }

    void OnDie()
    {
        Debug.Log("Enemy died!");

        //Die animation
        anim.SetTrigger("isDead");

        //Disable the enemy
        Destroy(gameObject, 0.4f);
    }
}




/*
public void OnDamaged()
{
    //Sprite Alpha
    spriteRenderer.color = new Color(1, 1, 1, 0.4f);

    //Sprite Flip Y
    spriteRenderer.flipY = true;

    //Collider Disable
    capsuleCollider.enabled = false;

    //Die Effect Jump
    rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);

    //Destroy
    Invoke("DeActive", 5);
}
*/