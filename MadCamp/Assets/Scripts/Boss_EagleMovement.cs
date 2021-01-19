/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_EagleMovement : MonoBehaviour
{
    public float nextMove;
    public GameObject bulletPrefab;

    Rigidbody2D rigid;
    Animator anim;
    SpriteRenderer spriteRenderer;
    CapsuleCollider2D capsuleCollider;

    int pattern;

    // Start is called before the first frame update
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();

        pattern = 0;
        StartCoroutine(PatternSlide(8, 1, 2, 1));
    }

    // Update is called once per frame
    void FiexedUpdate()
    {
        // Move
        rigid.velocity = new Vector2(nextMove, rigid.velocity.y);

        if (pattern == 1)
        {
            // Platform check
            Vector2 frontVec = new Vector2(rigid.position.x + nextMove * 0.3f, rigid.position.y);
            Debug.DrawRay(frontVec, Vector3.down, new Color(0, 1, 0));
            RaycastHit2D rayHit = Physics2D.Raycast(frontVec, Vector3.down, 1, LayerMask.GetMask("Platform"));

            if (rayHit.collider == null)
            {
                Turn();
            }
        }
    }

    public void OnDamaged()
    {
        StopAllCoroutines();
        nextMove = 0;

        //Sprite Alpha
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);

        //Sprite Flip Y
        spriteRenderer.flipY = true;

        //Die Effect Jump
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);

        pattern++;
        switch (pattern)
        {
            case 1:
                gameObject.layer = 11;
                StartCoroutine(PatternBullet(10, 2, 2));
                Invoke("OnDamaged_Reset", 1);
                break;

            case 2:
                //Collider Disable
                capsuleCollider.enabled = false;

                //Destroy
                Invoke("DeActive", 5);
                break;
        }
    }
}
*/