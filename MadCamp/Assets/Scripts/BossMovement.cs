using System.Collections;
using UnityEngine;

public class BossMovement : MonoBehaviour
{
    public float nextMove;
    public GameObject bulletPrefab;

    Rigidbody2D rigid;
    Animator anim;
    SpriteRenderer spriteRenderer;
    CapsuleCollider2D capsuleCollider;

    int pattern;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();

        pattern = 0;
        StartCoroutine(PatternSlide(8, 1, 2, 1));
    }

    void FixedUpdate()
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

    void OnDamaged_Reset()
    {
        spriteRenderer.color = new Color(1, 1, 1, 1);
        spriteRenderer.flipY = false;
        gameObject.layer = 10;
    }

    IEnumerator PatternSlide(int speed, int direction, float slideTime, float intervalTime)
    {
        while (true)
        {
            spriteRenderer.flipX = direction == 1;
            nextMove = speed * direction;
            yield return new WaitForSeconds(slideTime / 2);
            rigid.AddForce(Vector2.up * 20, ForceMode2D.Impulse);
            yield return new WaitForSeconds(slideTime / 2);
            nextMove = 0;
            yield return new WaitForSeconds(intervalTime);
            direction *= -1;
        }
    }

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
                Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
                rigid.velocity = bulletVector[i] * speed;
                Destroy(bullet, destroyTime);
            }

            yield return new WaitForSeconds(intervalTime);
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

    void DeActive()
    {
        gameObject.SetActive(false);
        Destroy(gameObject, 5);
    }
}
