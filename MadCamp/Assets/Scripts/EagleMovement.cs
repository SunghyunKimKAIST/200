using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EagleMovement : MonoBehaviour
{
    public float nextMove;
    public float moveSpeed;
    public float playerRange;
    public PlayerMovement player;

    public int maxHealth = 100;
    public int currentHealth;

    bool skill_50 = false;
    bool skill_10 = false;

    Rigidbody2D rigid;
    Animator anim;
    SpriteRenderer spriteRenderer;
    CapsuleCollider2D capsuleCollider;

    public GameObject fireBall;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();

        StartCoroutine(DefaultAttack());
    }

    void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, player.transform.position, moveSpeed * Time.deltaTime);
    }

    void OnDamaged(int damage)
    {
        currentHealth = currentHealth - damage;
        if (currentHealth <= 0)
        {
            OnDie();
        }
        //기본공격
        
        //피 50% 남았을 때
        if(currentHealth <= (maxHealth * 0.5) && (skill_50 == false))
        {
            //스킬 사용
        }
        //피 10% 남았을 때
        if(currentHealth <= (maxHealth * 0.1) && (skill_10 == false)){
            //스킬 사용
        }
    }
    void OnDie()
    {
        Debug.Log("Enemy died!");

        //Die animation
        anim.SetTrigger("isDead");

        //Disable the enemy
        Destroy(gameObject, 0.4f);
    }

    IEnumerator DefaultAttack()
    {
        while (true)
        {
            GameObject fireball = Instantiate(fireBall, transform.position, Quaternion.identity);
            Rigidbody2D rigid = fireball.GetComponent<Rigidbody2D>();

            //플레이어 방향으로 날아감
            rigid.velocity = Vector3.Normalize(player.transform.position - transform.position) * 10;

            //collider에 닿을 시 파괴
            Destroy(fireball, 5);

            //3초에 한 번 씩 생성
            yield return new WaitForSeconds(3);
        }
    }

    void Skill50()
    {
        for(int i = 0; i < 60; i++)
        {
            //화염구 60개 원 모양으로 생성
            GameObject fireball = Instantiate(fireBall, transform.position, Quaternion.identity);
            Rigidbody2D rigid = fireball.GetComponent<Rigidbody2D>();

            //각 방향으로 날아감
            rigid.velocity = Vector2.MoveTowards(transform.position, angle, moveSpeed * Time.deltaTime);
        }
        //10초 뒤 파괴

    }
}