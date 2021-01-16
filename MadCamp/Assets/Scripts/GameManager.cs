using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public PlayerMovement player;

    int hp;
    public Image[] UIhp;

    void Awake()
    {
        hp = 3;
    }

    public void HealthDown()
    {
        hp--;
        UIhp[hp].color = new Color(1, 1, 1, 0.2f);

        if(hp <= 0)
        {
            player.OnDie();
        }
    }
}
