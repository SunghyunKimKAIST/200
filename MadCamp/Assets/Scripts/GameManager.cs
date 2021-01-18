using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour
{
    // Server & Clients
    public GameObject[] Stages;

    // Server
    public GameObject bossPrefab;

    // Server
    int point;
    int stageIndex;
    List<PlayerMovement> players;

    void Start()
    {
        if (!isServer)
            return;

        players = new List<PlayerMovement>(2);
        point = 0;
        stageIndex = 0;
    }

    public bool IsServer { get => isServer; }

    // Server
    public void AddPlayer(PlayerMovement player)
    {
        players.Add(player);
    }

    // Server
    public void AddPoint(int point)
    {
        this.point += point;
        foreach (PlayerMovement player in players)
            player.RpcPoint(this.point);
    }

    [ClientRpc]
    void RpcNextStage(int before)
    {
        Stages[before].SetActive(false);
        Stages[before + 1].SetActive(true);
    }

    [ClientRpc]
    void RpcTimeScale(float timeScale)
    {
        Time.timeScale = timeScale;
    }

    // Server
    public void NextStage()
    {
        //Change Stage
        if (stageIndex < Stages.Length - 1)
        {
            Stages[stageIndex].SetActive(false);
            RpcNextStage(stageIndex);
            stageIndex++;
            Stages[stageIndex].SetActive(true);

            if (stageIndex == 1)
            {
                GameObject bossObject = Instantiate(bossPrefab, new Vector3(4.17f, -0.67f, 0), Quaternion.identity, Stages[1].transform);
                NetworkServer.Spawn(bossObject);
            }

            foreach (PlayerMovement player in players)
            {
                player.RpcStageIndex(stageIndex);
                player.RpcReposition();
            }

            Debug.Log("다음 스테이지");
        }
        else // Game Clear
        {
            //Player Control Lock
            RpcTimeScale(0);
            //Result UI
            Debug.Log("게임 클리어!");
            //Restart Button UI
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isServer)
            return;

        if (collision.gameObject.tag == "Player")
        {
            PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
            player.RpcHealthDown();
            player.RpcReposition();
        }
    }

    // Server
    public void Gameover()
    {
        RpcTimeScale(0);
    }

    // TODO
    [ClientRpc]
    void RpcRestart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }

    public void Restart()
    {
        RpcRestart();
    }
}