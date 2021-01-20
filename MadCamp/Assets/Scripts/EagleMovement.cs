using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EagleMovement : NetworkBehaviour
{
    public GameObject fireBallPrefab;

    public float moveSpeed;
    public int maxHealth;

    Rigidbody2D rigid;
    NetworkAnimator anim;
    SpriteRenderer spriteRenderer;
    CapsuleCollider2D capsuleCollider;

    GameManager gameManager;

    List<PlayerMovement> players;
    Vector2 minPosition;

    int health;
    bool skill_50;
    bool skill_10;

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

            StartCoroutine(DefaultAttack());

            health = 100;
            skill_50 = false;
            skill_10 = false;

            gameManager = FindObjectOfType<GameManager>();

            players = FindObjectOfType<GameManager>().players;
            minPosition = Vector2.zero;

            foreach(PlayerMovement player in players)
                player.RpcEagleSpawn();
        }
    }

    void Update()
    {
        if (!isServer)
            return;

        float minDist = float.MaxValue;
        foreach(PlayerMovement player in players)
        {
            float dist = Vector2.Distance(transform.position, player.transform.position);
            if (minDist > dist)
            {
                minDist = dist;
                minPosition = player.transform.position;
            }
        }
        transform.position = Vector2.MoveTowards(transform.position, minPosition, moveSpeed * Time.deltaTime);

        flipX = transform.position.x < minPosition.x;
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
        StartCoroutine(OffDamaged(1));
        RpcOnDamaged();

        foreach (PlayerMovement player in players)
            player.RpcUIEagleHealth(System.Math.Max(health, 0));

        if (health <= 0)
        {
            Debug.Log("Enemy died!");

            foreach (PlayerMovement player in players)
                player.GodMode();

            // Die animation
            anim.SetTrigger("isDead");
            anim.animator.SetTrigger("isDead");

            //Disable the enemy
            Destroy(gameObject, 0.6f);
            gameManager.NextStage();
        }

        //피 50% 남았을 때
        if (health <= (maxHealth * 0.5) && (skill_50 == false))
        {
            skill_50 = true;
            Skill50();
        }

        //피 10% 남았을 때
        if (health <= (maxHealth * 0.1) && (skill_10 == false))
        {
            skill_10 = true;
            Skill50();
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

    [Server]
    IEnumerator DefaultAttack()
    {
        yield return new WaitForSeconds(1);

        while (true)
        {
            GameObject fireball = Instantiate(fireBallPrefab, transform.position, Quaternion.identity);
            NetworkServer.Spawn(fireball);

            Rigidbody2D rigid = fireball.GetComponent<Rigidbody2D>();

            //플레이어 방향으로 날아감
            Vector2 direction = minPosition - (Vector2)transform.position;
            rigid.rotation = Vector2.SignedAngle(Vector2.right, direction);
            rigid.velocity = direction.normalized * 5;

            Destroy(fireball, 20);

            //3초에 한 번 씩 생성
            yield return new WaitForSeconds(3);
        }
    }

    [Server]
    void Skill50()
    {
        int n = 15;
        //화염구 n개 원 모양으로 생성
        for (int i = 0; i < n; i++)
        {
            GameObject fireball = Instantiate(fireBallPrefab, transform.position, Quaternion.identity);
            NetworkServer.Spawn(fireball);

            Rigidbody2D rigid = fireball.GetComponent<Rigidbody2D>();

            //각 방향으로 날아감
            rigid.rotation = 360f / n * i;
            rigid.velocity = (Vector2)(Quaternion.Euler(0, 0, 360f / n * i) * Vector2.right) * 10;

            Destroy(fireball, 20);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isServer)
            return;

        switch (collision.gameObject.tag)
        {
            case "FireBall":
                OnDamaged(5);
                Destroy(collision.gameObject);
                break;
        }
    }
}