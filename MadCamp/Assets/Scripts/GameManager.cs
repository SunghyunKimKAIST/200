using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int totalPoint;
    public int stagePoint;
    public int stageIndex;
    public int health;
    public PlayerMovement player;

    public GameObject[] Stages;

    public Image[] UIHealth;
    public Text UIPoint;
    public Text UIStage;

    void Update()
    {
        UIPoint.text = (totalPoint + stagePoint).ToString();
    }

    public void NextStage()
    {
        if (stageIndex + 1 >= Stages.Length)
            return;

        Stages[stageIndex].SetActive(false);
        stageIndex++;
        Stages[stageIndex].SetActive(true);
        PlayerReposition();

        UIStage.text = "STAGE " + (stageIndex + 1);

        totalPoint += stagePoint;
        stagePoint = 0;
    }

    public void HealthDown()
    {
        health--;
        UIHealth[health].color = new Color(1, 1, 1, 0.2f);

        if (health <= 0)
        {
            //Player Die Effect
            player.OnDie();

            //Result UI
            Debug.Log("디짐");

            //Retry Button UI
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            //Player Reposition
            if (health > 1)
                PlayerReposition();

            //Health Down
            HealthDown();
        }
    }

    void PlayerReposition()
    {

        player.transform.position = new Vector3(0, 0, -1);
        player.VelocityZero();
    }
}