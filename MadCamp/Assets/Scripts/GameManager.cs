using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    public GameObject UIRestartBtn;

    void Update()
    {
        UIPoint.text = (totalPoint + stagePoint).ToString();
    }

    public void NextStage()
    {
        //Change Stage
        if (stageIndex < Stages.Length - 1)
        {
            Stages[stageIndex].SetActive(false);
            stageIndex++;
            Stages[stageIndex].SetActive(true);
            PlayerReposition();

            Debug.Log("다음 스테이지");
            UIStage.text = "STAGE " + (stageIndex + 1);
        }
        else // Game Clear
        {
            //Player Control Lock
            Time.timeScale = 0;
            //Result UI
            Debug.Log("게임 클리어!");
            //Restart Button UI
            Text btnText = UIRestartBtn.GetComponentInChildren<Text>();
            btnText.text = "Game Clear!";
            UIRestartBtn.SetActive(true);
        }

        //Calculate Point
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
            UIRestartBtn.SetActive(true);
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
            PlayerReposition();
            HealthDown();
        }
    }

    void PlayerReposition()
    {

        player.transform.position = new Vector3(0, 0, -1);
        player.VelocityZero();
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }
}  