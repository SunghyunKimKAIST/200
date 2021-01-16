using System.Collections;
using UnityEngine;

public class Boss : MonoBehaviour
{
    Rigidbody2D rigid;
    Animator anim;
    SpriteRenderer spriteRenderer;

    public GameObject bulletPrefab;

    float velocity;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        StartCoroutine(PatternSlide(8, 1, 2, 1));
        Invoke("makePatternBullet", 7);
    }

    IEnumerator PatternSlide(int speed, int direction, float slideTime, float intervalTime)
    {
        while (true)
        {
            spriteRenderer.flipX = direction == 1;
            velocity = speed * direction;
            yield return new WaitForSeconds(slideTime / 2);
            rigid.AddForce(Vector2.up * 20, ForceMode2D.Impulse);
            yield return new WaitForSeconds(slideTime / 2);
            velocity = 0;
            yield return new WaitForSeconds(intervalTime);
            direction *= -1;
        }
    }

    IEnumerator PatternBullet(float speed, float intervalTime, float destroyTime)
    {
        while (true)
        {
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
            rigid.velocity = new Vector2(speed, 0);
            Destroy(bullet, destroyTime);
            yield return new WaitForSeconds(intervalTime);
        }
    }

    void makePatternBullet()
    {
        StopAllCoroutines();
        velocity = 0;
        StartCoroutine(PatternBullet(10, 1, 2));
    }

    void FixedUpdate()
    {
        // Move
        rigid.velocity = new Vector2(velocity, rigid.velocity.y);
    }
}
