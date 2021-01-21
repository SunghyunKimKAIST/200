using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour
{
    // Server & Clients
    public GameObject[] Stages;

    // Server
    public GameObject bossPrefab;
    public GameObject EaglePrefab;
    public GameObject FlowerPrefab;

    // Server
    int point;
    int stageIndex;
    public List<PlayerMovement> players;

    void Start()
    {
        if (!isServer)
            return;

        players = new List<PlayerMovement>(2);
        point = 0;
        stageIndex = 0;
    }

    public bool IsServer { get => isServer; }

    [Server]
    public void AddPlayer(PlayerMovement p)
    {
        players.Add(p);
    }

    [Server]
    public void AddPoint(int point)
    {
        this.point += point;
        foreach (PlayerMovement player in players)
            player.RpcPoint(this.point);
    }

    [ClientRpc]
    void RpcNextStage(int before)
    {
        if (isServer)
            return;

        Stages[before].SetActive(false);
        Stages[before + 1].SetActive(true);
    }

    [ClientRpc]
    void RpcTimeScale(float timeScale)
    {
        Time.timeScale = timeScale;
    }

    [Server]
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
            else if(stageIndex == 2)
            {
                GameObject eagleObject = Instantiate(EaglePrefab, new Vector2(17, 12), Quaternion.identity, Stages[2].transform);
                NetworkServer.Spawn(eagleObject);

                GameObject flowerObject = Instantiate(FlowerPrefab, new Vector2(9.322502f, 11.20651f), Quaternion.identity, Stages[2].transform);
                NetworkServer.Spawn(flowerObject);
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
            StartCoroutine(GameClear(3));

            //Result UI
            Debug.Log("게임 클리어!");
        }
    }

    [Server]
    IEnumerator GameClear(float time)
    {
        yield return new WaitForSeconds(time);

        //Player Control Lock
        RpcTimeScale(0);

        foreach (PlayerMovement player in players)
            player.RpcGameClear();
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

    [Server]
    public void Gameover()
    {
        foreach (PlayerMovement player in players)
            player.Gameover();

        RpcTimeScale(0);
    }
}